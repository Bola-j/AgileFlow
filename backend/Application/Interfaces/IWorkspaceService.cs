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
         Task  AddMemberAsync(int workspaceId, AddWorkspaceMemberRequest request, string currentUserId);

        Task UpdateMemberRoleAsync(int workspaceId, string memberUserId, UpdateWorkspaceMemberRoleRequest request, string userId);

        Task RemoveMemberAsync(int workspaceId, string memberUserId, string currentUserId);
        Task UpdateMemberProfileByAdminAsync(int workspaceId, string memberUserId, UpdateMemberProfileByAdminRequest request, string currentUserId);
        Task<WorkspaceMemberDetailResponse> GetWorkspaceMemberDetailAsync(int workspaceId, string memberUserId, string currentUserId);
    }
}