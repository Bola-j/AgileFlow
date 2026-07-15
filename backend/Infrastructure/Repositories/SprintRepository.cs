using AgileFlow.Domain.Entities;
using AgileFlow.Infrastructure.Persistence.Data;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class SprintRepository : ISprintRepository
{
    private readonly AgileFlowDbContext _context;

    public SprintRepository(AgileFlowDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Sprint>> GetByProjectIdAsync(int projectId)
    {
        return await _context.Sprints
            .Where(s => s.ProjectId == projectId && !s.IsDeleted)
            .Include(s => s.Tasks.Where(t => !t.IsDeleted))
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<Sprint?> GetByIdAsync(int id)
    {
        return await _context.Sprints
            .Where(s => s.Id == id && !s.IsDeleted)
            .Include(s => s.Project)
            .Include(s => s.Tasks.Where(t => !t.IsDeleted))
            .FirstOrDefaultAsync();
    }

    public async Task<bool> HasActiveSprintInProjectAsync(int projectId, int? excludeId = null)
    {
        return await _context.Sprints.AnyAsync(s =>
            s.ProjectId == projectId &&
            s.Status == SprintStatus.Active &&
            !s.IsDeleted &&
            (excludeId == null || s.Id != excludeId));
    }

    public async Task<bool> HasTasksDueAfterAsync(int sprintId, DateTime endDate)
    {
        var end = endDate.Date;
        return await _context.ProjectTasks.AnyAsync(t =>
            t.SprintId == sprintId &&
            !t.IsDeleted &&
            t.DueDate.Date > end);
    }

    public async Task<Sprint> AddAsync(Sprint sprint)
    {
        await _context.Sprints.AddAsync(sprint);
        await _context.SaveChangesAsync();
        return sprint;
    }

    public async Task UpdateAsync(Sprint sprint)
    {
        _context.Sprints.Update(sprint);
        await _context.SaveChangesAsync();
    }
}
