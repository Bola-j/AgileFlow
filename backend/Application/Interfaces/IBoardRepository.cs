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
        Task<IEnumerable<Board>> GetBoardsByProjectIdAsync(int projectId);
        Task<Board?> GetByIdAsync(int boardId);
        Task AddAsync(Board board);
        Task<bool> ProjectHasBoardAsync(int projectId);
        Task<BoardColumn?> GetColumnByIdAsync(int columnId);
        Task AddColumnAsync(BoardColumn column);
        Task UpdateColumnAsync(BoardColumn column);
        Task DeleteColumnAsync(BoardColumn column);
        Task<int> GetColumnsCountAsync(int boardId); 
        Task UpdateColumnsOrderAsync(List<BoardColumn> columns);
        Task<Board?> GetBoardWithDetailsByIdAsync(int boardId);
    }
}
