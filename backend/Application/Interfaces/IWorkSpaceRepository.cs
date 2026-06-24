using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IWorkspaceRepository
    {
        Task<IEnumerable<Workspace>> GetAllAsync();
        Task<Workspace?> GetByIdAsync(int id);
        Task<Workspace?> GetByIdWithDetailsAsync(int id);
        Task<IEnumerable<Workspace>> GetByUserIdAsync(string userId);
        Task<bool> NameExistsAsync(string name, string userId, int? excludeId = null);
        Task<bool> ExistsAsync(int id);
        Task<Workspace> AddAsync(Workspace workspace);
        Task UpdateAsync(Workspace workspace);
        Task DeleteAsync(Workspace workspace);
    }
}