using Application.DTOs.Workspace;
using Application.Interfaces.Repositories;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.Services
{
    public class WorkspaceService : IWorkspaceService
    {
        private readonly IWorkspaceRepository _workspaceRepository;
        private readonly IWorkspaceAuthorizationService _authorizationService;
        private readonly IMapper _mapper;

        public WorkspaceService(
            IWorkspaceRepository workspaceRepository,
            IWorkspaceAuthorizationService authorizationService,
            IMapper mapper)
        {
            _workspaceRepository = workspaceRepository;
            _authorizationService = authorizationService;
            _mapper = mapper;
        }

        public async Task<IEnumerable<WorkspaceSummaryResponse>> GetMyWorkspacesAsync(string userId)
        {
            var workspaces = await _workspaceRepository.GetByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<WorkspaceSummaryResponse>>(workspaces);
        }

        public async Task<WorkspaceResponse?> GetByIdAsync(int id, string userId)
        {
            var workspace = await _workspaceRepository.GetByIdWithDetailsAsync(id);
            if (workspace is null) return null;

            await _authorizationService.EnsureMemberAsync(id, userId);

            return _mapper.Map<WorkspaceResponse>(workspace);
        }

        public async Task<WorkspaceResponse> CreateAsync(CreateWorkspaceRequest request, string userId)
        {

            if (await _workspaceRepository.NameExistsAsync(request.Name, userId))
                throw new InvalidOperationException($"You already have a workspace named '{request.Name}'.");

            var workspace = new Workspace(request.Name, request.Description ?? string.Empty);

            await _workspaceRepository.AddAsync(workspace);

            var userWorkspace = new UserWorkspace(userId, workspace.Id, UserRole.Admin);
            workspace.UserWorkspaces.Add(userWorkspace); 

            await _workspaceRepository.UpdateAsync(workspace);

            var created = await _workspaceRepository.GetByIdWithDetailsAsync(workspace.Id);
            return _mapper.Map<WorkspaceResponse>(created!);
        }

        public async Task<WorkspaceResponse?> UpdateAsync(int id, UpdateWorkspaceRequest request, string userId)
        {
            var workspace = await _workspaceRepository.GetByIdWithDetailsAsync(id);
            if (workspace is null) return null;

            await _authorizationService.EnsureRoleAsync(id, userId, UserRole.Admin);

            if (await _workspaceRepository.NameExistsAsync(request.Name, userId, excludeId: id))
                throw new InvalidOperationException($"You already have a workspace named '{request.Name}'.");

            workspace.UpdateName(request.Name);
            if (request.Description is not null)
                workspace.UpdateDescription(request.Description);

            await _workspaceRepository.UpdateAsync(workspace);

            var updated = await _workspaceRepository.GetByIdWithDetailsAsync(id);
            return _mapper.Map<WorkspaceResponse>(updated!);
        }

        public async Task<bool> DeleteAsync(int id, string userId)
        {
            var workspace = await _workspaceRepository.GetByIdWithDetailsAsync(id);
            if (workspace is null) return false;

            await _authorizationService.EnsureRoleAsync(id, userId, UserRole.Admin);

            await _workspaceRepository.DeleteAsync(workspace);
            return true;
        }
    }
}
