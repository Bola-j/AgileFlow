using AgileFlow.Core.Entities;

namespace AgileFlow.Core.Interfaces;

public interface IIssueRepository
{
    Task<IReadOnlyList<Issue>> GetByProjectAsync(Guid projectId);
    Task<Issue?> GetByIdAsync(Guid id);
}
