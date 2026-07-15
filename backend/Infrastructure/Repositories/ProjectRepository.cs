using AgileFlow.Domain.Entities;
using AgileFlow.Application.Interfaces;
using AgileFlow.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace AgileFlow.Infrastructure.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly AgileFlowDbContext _context;

    public ProjectRepository(AgileFlowDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Project>> GetAllByWorkspaceIdAsync(int workspaceId)
    {
        return await _context.Projects
            .Where(p => p.WorkspaceId == workspaceId && !p.IsDeleted)
            .ToListAsync();
    }

    public async Task<Project?> GetByIdAsync(int id)
    {
        return await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
    }

    public async Task<bool> NameExistsInWorkspaceAsync(string name, int workspaceId, int? excludeId = null)
    {
        return await _context.Projects.AnyAsync(p =>
            p.Name == name &&
            p.WorkspaceId == workspaceId &&
            !p.IsDeleted &&
            (excludeId == null || p.Id != excludeId));
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Projects.AnyAsync(p => p.Id == id && !p.IsDeleted);
    }

    public async Task<bool> HasSprintsEndingAfterAsync(int projectId, DateTime endDate)
    {
        var end = endDate.Date;
        return await _context.Sprints.AnyAsync(s =>
            s.ProjectId == projectId &&
            !s.IsDeleted &&
            s.EndDate.Date > end);
    }

    public async Task<bool> HasTasksDueAfterAsync(int projectId, DateTime endDate)
    {
        var end = endDate.Date;
        return await _context.ProjectTasks.AnyAsync(t =>
            t.Sprint != null &&
            t.Sprint.ProjectId == projectId &&
            !t.IsDeleted &&
            !t.Sprint.IsDeleted &&
            t.DueDate.Date > end);
    }

    public async Task<Project> AddAsync(Project project)
    {
        await _context.Projects.AddAsync(project);
        await _context.SaveChangesAsync();
        return project;
    }

    public async Task UpdateAsync(Project project)
    {
        _context.Projects.Update(project);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Project project)
    {
        project.Delete();
        _context.Projects.Update(project);
        await _context.SaveChangesAsync();
    }
}


