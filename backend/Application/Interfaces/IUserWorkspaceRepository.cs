using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUserWorkspaceRepository
    {
        Task<UserWorkspace?> GetMembershipAsync(int workspaceId, string userId);

        Task AddAsync(UserWorkspace membership);

        Task UpdateAsync(UserWorkspace membership);
    }
}
