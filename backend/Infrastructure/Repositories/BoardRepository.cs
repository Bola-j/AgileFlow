using AgileFlow.Domain.Entities;
using AgileFlow.Infrastructure.Persistence.Data;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class BoardRepository : IBoardRepository
    {
        private readonly AgileFlowDbContext _context;

        public BoardRepository(AgileFlowDbContext context)
        {
            _context = context;
        }

        public async Task<Board?> GetByIdAsync(int id)
        {
            return await _context.Boards.FindAsync(id);
        }

        public async Task<Board?> GetByProjectIdAsync(int projectId)
        {
            return await _context.Boards
                .FirstOrDefaultAsync(b => b.ProjectId == projectId);
        }

        public async Task AddAsync(Board board)
        {
            await _context.Boards.AddAsync(board);
            await _context.SaveChangesAsync();
        }

        public async Task<BoardColumn?> GetColumnByIdAsync(int columnId)
        {
            return await _context.BoardColumns
                .Include(c => c.Board) 
                .FirstOrDefaultAsync(c => c.Id == columnId);
        }

        public async Task AddColumnAsync(BoardColumn column)
        {
            await _context.BoardColumns.AddAsync(column);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateColumnAsync(BoardColumn column)
        {
            _context.BoardColumns.Update(column);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteColumnAsync(BoardColumn column)
        {
            column.Delete();
            _context.BoardColumns.Update(column);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetColumnsCountAsync(int boardId)
        {
            return await _context.BoardColumns.CountAsync(c => c.BoardId == boardId && !c.IsDeleted);
        }

        public async Task<bool> ColumnHasActiveTasksAsync(int columnId)
        {
            return await _context.ProjectTasks.AnyAsync(t => t.ColumnId == columnId && !t.IsDeleted);
        }

        public async Task UpdateColumnsOrderAsync(List<BoardColumn> columns)
        {
            foreach (var column in columns)
            {
                _context.Entry(column).Property(c => c.Position).IsModified = true;
            }
            await _context.SaveChangesAsync();
        }
        public async Task<List<BoardColumn>> GetColumnsByProjectIdAsync(int projectId)
        {
            return await _context.BoardColumns
                .Where(c => c.Board.ProjectId == projectId && !c.IsDeleted)
                .OrderBy(c => c.Position)
                .ToListAsync();
        }

        public async Task<Board?> GetBoardWithDetailsByProjectIdAsync(int projectId,int sprintId)
        {
            return await _context.Boards
                .AsSplitQuery()
                .Include(b => b.BoardColumns.Where(c => !c.IsDeleted).OrderBy(c => c.Position))
                    .ThenInclude(c => c.Tasks.Where(t => !t.IsDeleted && t.SprintId == sprintId))
                        .ThenInclude(t => t.UserTasks.Where(ut => !ut.IsDeleted))
                            .ThenInclude(ut => ut.AppUser)
                .Include(b => b.BoardColumns.Where(c => !c.IsDeleted).OrderBy(c => c.Position))
                    .ThenInclude(c => c.Tasks.Where(t => !t.IsDeleted && t.SprintId == sprintId))
                        .ThenInclude(t => t.TaskDependents)
                            .ThenInclude(td => td.DependedTask)
                .FirstOrDefaultAsync(b => b.ProjectId == projectId);
        }
    }
}
