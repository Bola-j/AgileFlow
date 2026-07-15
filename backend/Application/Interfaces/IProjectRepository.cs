using AgileFlow.Domain.Entities;

namespace AgileFlow.Application.Interfaces;

public interface IProjectRepository
{
    Task<IEnumerable<Project>> GetAllByWorkspaceIdAsync(int workspaceId);
    Task<Project?> GetByIdAsync(int id);
    Task<bool> NameExistsInWorkspaceAsync(string name, int workspaceId, int? excludeId = null);
    Task<bool> ExistsAsync(int id);
    Task<bool> HasSprintsEndingAfterAsync(int projectId, DateTime endDate);
    Task<bool> HasTasksDueAfterAsync(int projectId, DateTime endDate);
    Task<Project> AddAsync(Project project);
    Task UpdateAsync(Project project);
    Task DeleteAsync(Project project);
}

