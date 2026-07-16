using AgileFlow.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IBoardRepository
    {
        Task<Board?> GetByIdAsync(int boardId);
        Task<Board?> GetByProjectIdAsync(int projectId);
        Task<BoardColumn?> GetColumnByIdAsync(int columnId);
        Task AddColumnAsync(BoardColumn column);
        Task UpdateColumnAsync(BoardColumn column);
        Task DeleteColumnAsync(BoardColumn column);
        Task<int> GetColumnsCountAsync(int boardId); 
        Task<bool> ColumnHasActiveTasksAsync(int columnId);
        Task UpdateColumnsOrderAsync(List<BoardColumn> columns);
        Task<List<BoardColumn>> GetColumnsByProjectIdAsync(int projectId);
        Task<Board?> GetBoardWithDetailsByProjectIdAsync(int projectId,int sprintId);
    }
}
