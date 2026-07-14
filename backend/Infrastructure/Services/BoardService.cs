using AgileFlow.Domain.Entities;
using Application.DTOs.Board;
using Application.Interfaces;
using AutoMapper;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class BoardService : IBoardService
    {
        private readonly IBoardRepository _boardRepository;
        private readonly IWorkspaceAuthorizationService _authorizationService;
        private readonly IMapper _mapper;

        public BoardService(
            IBoardRepository boardRepository,
            IWorkspaceAuthorizationService authorizationService,
            IMapper mapper)
        {
            _boardRepository = boardRepository;
            _authorizationService = authorizationService;
            _mapper = mapper;
        }

        public async Task<GetBoardDetailsResponse?> GetBoardDetailsAsync(
            int projectId,
            int sprintId,
            string currentUserId)
        {
            var workspaceId = await _authorizationService.GetWorkspaceIdForProjectAsync(projectId);
            var membership = await _authorizationService.EnsureMemberAsync(workspaceId, currentUserId);
            var board = await _boardRepository.GetBoardWithDetailsByProjectIdAsync(projectId, sprintId);

            if (board is null)
                return null;

            var response = _mapper.Map<GetBoardDetailsResponse>(board);
            if (membership.Role is UserRole.Admin or UserRole.TeamLead)
                return response;

            var visibilityReasons = BuildMemberVisibilityReasons(board, currentUserId);
            foreach (var column in response.Columns)
            {
                column.Tasks = column.Tasks
                    .Where(task => visibilityReasons.ContainsKey(task.Id))
                    .ToList();

                foreach (var task in column.Tasks)
                {
                    task.VisibilityReasons = visibilityReasons[task.Id];
                }
            }

            return response;
        }

        private static Dictionary<int, List<string>> BuildMemberVisibilityReasons(Board board, string currentUserId)
        {
            var tasks = board.BoardColumns
                .SelectMany(column => column.Tasks)
                .Where(task => !task.IsDeleted)
                .ToList();

            var assignedTaskIds = tasks
                .Where(task => task.UserTasks.Any(assignment =>
                    !assignment.IsDeleted &&
                    assignment.AppUserId == currentUserId))
                .Select(task => task.Id)
                .ToHashSet();

            var reasons = new Dictionary<int, List<string>>();
            foreach (var task in tasks)
            {
                if (assignedTaskIds.Contains(task.Id))
                    AddVisibilityReason(reasons, task.Id, "AssignedToYou");

                if (task.TaskDependents.Any(dependency => assignedTaskIds.Contains(dependency.DependedTaskId)))
                    AddVisibilityReason(reasons, task.Id, "DependsOnYourTask");

                if (tasks.Any(assignedTask =>
                    assignedTaskIds.Contains(assignedTask.Id) &&
                    assignedTask.TaskDependents.Any(dependency => dependency.DependedTaskId == task.Id)))
                {
                    AddVisibilityReason(reasons, task.Id, "MandatoryForYourTask");
                }
            }

            return reasons;
        }

        private static void AddVisibilityReason(Dictionary<int, List<string>> reasons, int taskId, string reason)
        {
            if (!reasons.TryGetValue(taskId, out var taskReasons))
            {
                taskReasons = new List<string>();
                reasons[taskId] = taskReasons;
            }

            if (!taskReasons.Contains(reason))
                taskReasons.Add(reason);
        }

        public async Task AddColumnAsync(
            int projectId,
            AddColumnRequest request,
            string currentUserId)
        {

            await _authorizationService.EnsureProjectRoleAsync(
                projectId,
                currentUserId,
                UserRole.Admin,
                UserRole.TeamLead);

            var board = await _boardRepository.GetByProjectIdAsync(projectId)
                ?? throw new KeyNotFoundException($"Board for project {projectId} was not found.");

            int currentColumnsCount = await _boardRepository.GetColumnsCountAsync(board.Id);

            var newColumn = new BoardColumn(request.ColumnName, board.Id);
            newColumn.UpdatePosition(currentColumnsCount);
            await _boardRepository.AddColumnAsync(newColumn);
        }

        public async Task UpdateColumnNameAsync(
            int columnId,
            UpdateColumnRequest request,
            string currentUserId)
        {
            var column = await _boardRepository.GetColumnByIdAsync(columnId)
                ?? throw new KeyNotFoundException($"Column with id {columnId} not found.");

            if (column.IsDeleted)
                throw new InvalidOperationException("Column is deleted.");

            await _authorizationService.EnsureProjectRoleAsync(
                column.Board.ProjectId,
                currentUserId,
                UserRole.Admin,
                UserRole.TeamLead);

            column.UpdateName(request.NewName);

            await _boardRepository.UpdateColumnAsync(column);
        }

        public async Task DeleteColumnAsync(int columnId, string currentUserId)
        {
            var column = await _boardRepository.GetColumnByIdAsync(columnId)
                ?? throw new KeyNotFoundException($"Column with id {columnId} not found.");

            if (column.IsDeleted) 
                throw new InvalidOperationException("Column is already deleted.");

            await _authorizationService.EnsureProjectRoleAsync(column.Board.ProjectId, currentUserId, UserRole.Admin, UserRole.TeamLead);
            await _boardRepository.DeleteColumnAsync(column);
        }

        public async Task UpdateColumnsOrderAsync(
            int projectId,
            UpdateColumnOrderRequest request,
            string currentUserId)
        {
            await _authorizationService.EnsureProjectRoleAsync(
                projectId,
                currentUserId,
                UserRole.Admin,
                UserRole.TeamLead);

            var columns = await _boardRepository.GetColumnsByProjectIdAsync(projectId);

            if (!columns.Any())
                throw new KeyNotFoundException($"No columns found for project {projectId}.");

            for (int i = 0; i < request.OrderedColumnIds.Count; i++)
            {
                var colId = request.OrderedColumnIds[i];
                var column = columns.FirstOrDefault(c => c.Id == colId);
                if (column is null)
                    throw new InvalidOperationException($"Column with id {colId} does not belong to this board.");
                column.UpdatePosition(i);
            }

            await _boardRepository.UpdateColumnsOrderAsync(columns);
        }
    }
}
