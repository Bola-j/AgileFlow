using Application.DTOs.Board;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IBoardService
    {
        Task<CreateBoardResponse> CreateBoardAsync(int projectId,CreateBoardRequest request,string currentUserId);

        Task<GetBoardDetailsResponse?> GetBoardDetailsAsync(int boardId, string currentUserId);
        Task<IEnumerable<BoardSummaryResponse>> GetProjectBoardsAsync(int projectId, string currentUserId);
        Task AddColumnAsync(int boardId, AddColumnRequest request, string currentUserId);
        Task UpdateColumnNameAsync(int columnId, UpdateColumnRequest request, string currentUserId);
        Task DeleteColumnAsync(int columnId, string currentUserId);

        Task UpdateColumnsOrderAsync(int boardId, UpdateColumnOrderRequest request, string currentUserId);
    }
}
