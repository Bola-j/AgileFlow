using AgileFlow.Core.Entities;

namespace AgileFlow.Core.Interfaces;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(int id);
    Task<IReadOnlyList<Project>> GetAllAsync();
}
