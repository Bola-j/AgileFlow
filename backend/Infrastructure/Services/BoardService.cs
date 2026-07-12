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
        public async Task<CreateBoardResponse> CreateBoardAsync(
            int projectId,
            CreateBoardRequest request,
            string currentUserId)
        {
            await _authorizationService.EnsureProjectRoleAsync(
                projectId,
                currentUserId,
                UserRole.Admin,
                UserRole.TeamLead);

            var board = new Board(request.Name, projectId);
            await _boardRepository.AddAsync(board);

            var todoColumn = new BoardColumn("To Do", board.Id);
            todoColumn.UpdatePosition(0);

            var inProgressColumn = new BoardColumn("In Progress", board.Id);
            inProgressColumn.UpdatePosition(1);

            var doneColumn = new BoardColumn("Done", board.Id);
            doneColumn.UpdatePosition(2);

            await _boardRepository.AddColumnAsync(todoColumn);
            await _boardRepository.AddColumnAsync(inProgressColumn);
            await _boardRepository.AddColumnAsync(doneColumn);

            return _mapper.Map<CreateBoardResponse>(board);
        }

        public async Task<GetBoardDetailsResponse?> GetBoardDetailsAsync(
            int boardId,
            string currentUserId)
        {
            var board = await _boardRepository.GetBoardWithDetailsByIdAsync(boardId);

            if (board is null)
                return null;

            await _authorizationService.EnsureProjectMemberAsync(
                board.ProjectId,
                currentUserId);

            return _mapper.Map<GetBoardDetailsResponse>(board);
        }

        public async Task AddColumnAsync(
            int boardId,
            AddColumnRequest request,
            string currentUserId)
        {
            var board = await _boardRepository.GetByIdAsync(boardId)
                ?? throw new KeyNotFoundException($"Board with id {boardId} not found.");

            await _authorizationService.EnsureProjectRoleAsync(
                board.ProjectId,
                currentUserId,
                UserRole.Admin,
                UserRole.TeamLead);

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
            int boardId,
            UpdateColumnOrderRequest request,
            string currentUserId)
        {
            var board = await _boardRepository.GetBoardWithDetailsByIdAsync(boardId)
                ?? throw new KeyNotFoundException($"Board with id {boardId} not found.");

            await _authorizationService.EnsureProjectRoleAsync(
                board.ProjectId,
                currentUserId,
                UserRole.Admin,
                UserRole.TeamLead);

            for (int i = 0; i < request.OrderedColumnIds.Count; i++)
            {
                var colId = request.OrderedColumnIds[i];

                var column = board.BoardColumns.FirstOrDefault(c => c.Id == colId);

                if (column is not null)
                {
                    column.UpdatePosition(i);
                }
            }
            await _boardRepository.UpdateColumnsOrderAsync(board.BoardColumns.ToList());
        }

        public async Task<IEnumerable<BoardSummaryResponse>> GetProjectBoardsAsync(int projectId,string currentUserId)
        {
            await _authorizationService.EnsureProjectMemberAsync(
                projectId,
                currentUserId);

            var boards = await _boardRepository.GetBoardsByProjectIdAsync(projectId);

            return _mapper.Map<IEnumerable<BoardSummaryResponse>>(boards);
        }
    }
}
