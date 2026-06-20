namespace Domain.Enums;

/// <summary>
/// Role a user holds within a specific Workspace.
/// Stored as int in UserWorkspaces.Role column.
/// </summary>
public enum UserRole
{
    Developer = 0,
    TeamLead = 1,
    Admin = 2,
}