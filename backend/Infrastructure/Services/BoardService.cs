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
            await _authorizationService.EnsureProjectMemberAsync(
            projectId,
            currentUserId);
            var board = await _boardRepository.GetBoardWithDetailsByProjectIdAsync(projectId, sprintId);

            if (board is null)
                return null;

            return _mapper.Map<GetBoardDetailsResponse>(board);
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
