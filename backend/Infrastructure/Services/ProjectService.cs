using AgileFlow.Application.Interfaces;
using Application.DTOs.Project;
using Application.Interfaces.Repositories;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgileFlow.Domain.Entities;

namespace Infrastructure.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IWorkspaceRepository _workspaceRepository;
        private readonly IWorkspaceAuthorizationService _authorizationService;
        private readonly IBoardRepository _boardRepository;
        private readonly IMapper _mapper;

        public ProjectService(
            IProjectRepository projectRepository,
            IWorkspaceRepository workspaceRepository,
            IBoardRepository boardRepository,
            IWorkspaceAuthorizationService authorizationService,
            IMapper mapper)
        {
            _projectRepository = projectRepository;
            _workspaceRepository = workspaceRepository;
            _boardRepository = boardRepository;
            _authorizationService = authorizationService;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProjectResponse>> GetByWorkspaceIdAsync(int workspaceId, string userId)
        {

            var workspace = await _workspaceRepository.GetByIdWithDetailsAsync(workspaceId);
            if (workspace is null)
                throw new KeyNotFoundException($"Workspace with id {workspaceId} not found.");

            await _authorizationService.EnsureMemberAsync(workspaceId, userId);

            var projects = await _projectRepository.GetAllByWorkspaceIdAsync(workspaceId);
            return _mapper.Map<IEnumerable<ProjectResponse>>(projects);
        }

        public async Task<ProjectResponse?> GetByIdAsync(int id, string userId)
        {
            var project = await _projectRepository.GetByIdAsync(id);
            if (project is null) return null;

            await _authorizationService.EnsureMemberAsync(project.WorkspaceId, userId);

            return _mapper.Map<ProjectResponse>(project);
        }

        public async Task<ProjectResponse> CreateAsync(CreateProjectRequest request, string userId)
        {

            var workspace = await _workspaceRepository.GetByIdWithDetailsAsync(request.WorkspaceId);
            if (workspace is null)
                throw new KeyNotFoundException($"Workspace with id {request.WorkspaceId} not found.");

            await _authorizationService.EnsureRoleAsync(request.WorkspaceId, userId, UserRole.Admin, UserRole.TeamLead);

            if (request.EndDate.Date <= request.StartDate.Date)
                throw new InvalidOperationException("EndDate must be after StartDate.");

            if (await _projectRepository.NameExistsInWorkspaceAsync(request.Name, request.WorkspaceId))
                throw new InvalidOperationException($"A project named '{request.Name}' already exists in this workspace.");

            var project = new Project(
                name: request.Name,
                status: request.Status,
                startDate: request.StartDate,
                workspaceId: request.WorkspaceId,
                description: request.Description ?? string.Empty,
                endDate: request.EndDate);

            var created = await _projectRepository.AddAsync(project);
            return _mapper.Map<ProjectResponse>(created);
        }

        public async Task<ProjectResponse?> UpdateAsync(int id, UpdateProjectRequest request, string userId)
        {
            var project = await _projectRepository.GetByIdAsync(id);
            if (project is null) return null;

            await _authorizationService.EnsureRoleAsync(project.WorkspaceId, userId, UserRole.Admin, UserRole.TeamLead);

            if (request.EndDate.Date <= project.StartDate.Date)
                throw new InvalidOperationException("EndDate must be after the project's StartDate.");

            if (await _projectRepository.HasSprintsEndingAfterAsync(id, request.EndDate))
                throw new InvalidOperationException("Project EndDate cannot be before any sprint EndDate.");

            if (await _projectRepository.HasTasksDueAfterAsync(id, request.EndDate))
                throw new InvalidOperationException("Project EndDate cannot be before any task DueDate.");

            if (await _projectRepository.NameExistsInWorkspaceAsync(request.Name, project.WorkspaceId, excludeId: id))
                throw new InvalidOperationException($"A project named '{request.Name}' already exists in this workspace.");

            project.UpdateName(request.Name);
            project.UpdateDescription(request.Description ?? string.Empty);
            project.UpdateStatus(request.Status);
            project.UpdateEndDate(request.EndDate);

            await _projectRepository.UpdateAsync(project);
            return _mapper.Map<ProjectResponse>(project);
        }

        public async Task<bool> DeleteAsync(int id, string userId)
        {
            var project = await _projectRepository.GetByIdAsync(id);
            if (project is null) return false;

            await _authorizationService.EnsureRoleAsync(project.WorkspaceId, userId, UserRole.Admin, UserRole.TeamLead);

            await _projectRepository.DeleteAsync(project);
            return true;
        }
    }
}
