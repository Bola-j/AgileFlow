using AgileFlow.Domain.Entities;

namespace Application.Interfaces;

public interface ISprintRepository
{
    Task<IEnumerable<Sprint>> GetByProjectIdAsync(int projectId);
    Task<Sprint?> GetByIdAsync(int id);
    Task<bool> HasActiveSprintInProjectAsync(int projectId, int? excludeId = null);
    Task<bool> HasTasksDueAfterAsync(int sprintId, DateTime endDate);
    Task<Sprint> AddAsync(Sprint sprint);
    Task UpdateAsync(Sprint sprint);
}
