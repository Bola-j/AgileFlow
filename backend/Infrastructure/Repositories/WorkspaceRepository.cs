using AgileFlow.Infrastructure.Persistence.Data;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;

namespace Infrastructure.Repositories
{
    public class WorkspaceRepository : IWorkspaceRepository
    {
        private readonly AgileFlowDbContext _context;

        public WorkspaceRepository(AgileFlowDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Workspace>> GetAllAsync()
        {
            return await _context.Workspaces
                .Where(w => !w.IsDeleted)
                .ToListAsync();
        }

        public async Task<Workspace?> GetByIdAsync(int id)
        {
            return await _context.Workspaces
                .FirstOrDefaultAsync(w => w.Id == id && !w.IsDeleted);
        }

        public async Task<Workspace?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Workspaces
                .Where(w => w.Id == id && !w.IsDeleted)
                .Include(w => w.Projects.Where(p => !p.IsDeleted))
                .Include(w => w.UserWorkspaces.Where(uw => !uw.IsDeleted))
                    .ThenInclude(uw => uw.AppUser)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Workspace>> GetByUserIdAsync(string userId)
        {
            return await _context.Workspaces
                .Where(w => !w.IsDeleted &&
                            w.UserWorkspaces.Any(uw => uw.AppUserId == userId && !uw.IsDeleted))
                .Include(w => w.Projects.Where(p => !p.IsDeleted))
                .Include(w => w.UserWorkspaces.Where(uw => !uw.IsDeleted))
                    .ThenInclude(uw => uw.AppUser)
                .ToListAsync();
        }

        public async Task<bool> NameExistsAsync(string name, string userId, int? excludeId = null)
        {
            return await _context.Workspaces.AnyAsync(w =>
                w.Name == name &&
                !w.IsDeleted &&
                w.UserWorkspaces.Any(uw => uw.AppUserId == userId && !uw.IsDeleted) &&
                (excludeId == null || w.Id != excludeId));
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Workspaces.AnyAsync(w => w.Id == id && !w.IsDeleted);
        }

        public async Task<Workspace> AddAsync(Workspace workspace)
        {
            await _context.Workspaces.AddAsync(workspace);
            await _context.SaveChangesAsync();
            return workspace;
        }

        public async Task UpdateAsync(Workspace workspace)
        {
            _context.Workspaces.Update(workspace);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Workspace workspace)
        {
            workspace.Delete();
            _context.Workspaces.Update(workspace);
            await _context.SaveChangesAsync();
        }
    }
}