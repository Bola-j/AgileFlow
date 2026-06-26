using Application.DTOs.Workspace;

namespace Application.Interfaces
{
    public interface IWorkspaceService
    {
        Task<IEnumerable<WorkspaceSummaryResponse>> GetMyWorkspacesAsync(string userId);

        Task<WorkspaceResponse?> GetByIdAsync(int id, string userId);

        Task<WorkspaceResponse> CreateAsync(CreateWorkspaceRequest request, string userId);
        Task<WorkspaceResponse?> UpdateAsync(int id, UpdateWorkspaceRequest request, string userId);
        Task<bool> DeleteAsync(int id, string userId);
    }
}