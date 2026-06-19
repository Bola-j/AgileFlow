using AgileFlow.Domain.Entities;

namespace AgileFlow.Application.Interfaces;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(int id);
    Task<IReadOnlyList<Project>> GetAllAsync();
}

