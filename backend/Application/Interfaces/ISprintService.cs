using Application.DTOs.Sprint;

namespace Application.Interfaces;

public interface ISprintService
{
    Task<IEnumerable<SprintResponse>> GetByProjectIdAsync(int projectId, string userId);
    Task<SprintResponse?> GetByIdAsync(int id, string userId);
    Task<SprintResponse> CreateAsync(int projectId, CreateSprintRequest request, string userId);
    Task<SprintResponse?> UpdateAsync(int id, UpdateSprintRequest request, string userId);
    Task<SprintResponse?> StartAsync(int id, string userId);
    Task<SprintResponse?> CompleteAsync(int id, string userId);
    Task<SprintProgressResponse?> GetProgressAsync(int id, string userId);
}
