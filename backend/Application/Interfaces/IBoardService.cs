using AgileFlow.Domain.Entities;
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

        Task<GetBoardDetailsResponse?> GetBoardDetailsAsync(int projectId, int sprintId, string currentUserId);
        Task AddColumnAsync(int projectId, AddColumnRequest request, string currentUserId);
        Task UpdateColumnNameAsync(int columnId, UpdateColumnRequest request, string currentUserId);
        Task DeleteColumnAsync(int columnId, string currentUserId);

        Task UpdateColumnsOrderAsync(int projectId, UpdateColumnOrderRequest request, string currentUserId);
    }
}
