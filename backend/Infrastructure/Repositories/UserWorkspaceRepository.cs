using AgileFlow.Application.Interfaces;
using AgileFlow.Infrastructure.Persistence.Data;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class UserWorkspaceRepository : IUserWorkspaceRepository
    {
        private readonly AgileFlowDbContext _context;

        public UserWorkspaceRepository(AgileFlowDbContext context)
        {
            _context = context;
        }

        public async Task<UserWorkspace?> GetMembershipAsync(int workspaceId, string userId)
        {
            return await _context.UserWorkspaces
                .IgnoreQueryFilters()
                .Include(uw => uw.AppUser)
                .FirstOrDefaultAsync(uw =>
                    uw.WorkspaceId == workspaceId &&
                    uw.AppUserId == userId &&
                    !uw.Workspace.IsDeleted &&
                    !uw.AppUser.IsDeleted);
        }

        public async Task AddAsync(UserWorkspace membership)
        {
            await _context.UserWorkspaces.AddAsync(membership);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(UserWorkspace membership)
        {
            _context.UserWorkspaces.Update(membership);
            await _context.SaveChangesAsync();
        }
    }
}
