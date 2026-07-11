using AgileFlow.Domain.Entities;
using Domain.Enums;

namespace Domain.Entities;

/// <summary>
/// Join table between AppUser and Workspace.
/// Role is scoped per workspace — the same user can be Admin in one workspace and Developer in another.
/// </summary>
public class UserWorkspace
{
    public string AppUserId { get; private set; } = string.Empty;
    public AppUser AppUser { get; private set; } = null!;
    public int WorkspaceId { get; private set; }
    public Workspace Workspace { get; private set; } = null!;
    public UserRole Role { get; private set; }          // ← NEW
    public DateTime JoinedAt { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    private UserWorkspace() { }

    public UserWorkspace(string appUserId, int workspaceId, UserRole role = UserRole.Developer)
    {
        AppUserId = appUserId;
        WorkspaceId = workspaceId;
        Role = role;
        JoinedAt = DateTime.UtcNow;
    }

    private void Update()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateRole(UserRole role)
    {
        Role = role;
        Update();
    }
    public void Delete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        Update();
    }

    public void Restore(UserRole role)
    {
        IsDeleted = false;
        DeletedAt = null;
        Role = role;
        Update();
    }
}