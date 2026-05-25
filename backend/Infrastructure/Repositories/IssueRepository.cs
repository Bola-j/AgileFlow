using AgileFlow.Core.Entities;
using AgileFlow.Core.Interfaces;
using AgileFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgileFlow.Infrastructure.Repositories;

public class IssueRepository : IIssueRepository
{
    private readonly AgileFlowDbContext _dbContext;

    public IssueRepository(AgileFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<IReadOnlyList<Issue>> GetByProjectAsync(Guid projectId) =>
        _dbContext.Issues.AsNoTracking().Where(issue => issue.ProjectId == projectId).ToListAsync();

    public Task<Issue?> GetByIdAsync(Guid id) =>
        _dbContext.Issues.AsNoTracking().FirstOrDefaultAsync(issue => issue.Id == id);
}
