using AgileFlow.Core.Entities;
using AgileFlow.Core.Interfaces;
using AgileFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgileFlow.Infrastructure.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly AgileFlowDbContext _dbContext;

    public ProjectRepository(AgileFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Project?> GetByIdAsync(Guid id) =>
        _dbContext.Projects.FirstOrDefaultAsync(project => project.Id == id);

    public Task<IReadOnlyList<Project>> GetAllAsync() =>
        _dbContext.Projects.AsNoTracking().ToListAsync();
}
