using AgileFlow.Core.Entities;
using AgileFlow.Core.Interfaces;
using Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace AgileFlow.Infrastructure.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly AgileFlowDbContext _dbContext;

    public ProjectRepository(AgileFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Project?> GetByIdAsync(int id) =>
        _dbContext.Projects.FirstOrDefaultAsync(project => project.Id == id);

    public async Task<IReadOnlyList<Project>> GetAllAsync()
    {
        var projects = await _dbContext.Projects.AsNoTracking().ToListAsync();
        return projects;
    }
}
