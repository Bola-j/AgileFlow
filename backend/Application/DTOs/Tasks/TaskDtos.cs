using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Tasks;

public class CreateTaskRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    public ProjectTaskStatus Status { get; set; } = ProjectTaskStatus.Todo;

    public ProjectTaskPriority Priority { get; set; } = ProjectTaskPriority.Medium;

    [Required]
    public DateTime DueDate { get; set; }

    [Required]
    public int ColumnId { get; set; }

    public List<string> AssigneeUserIds { get; set; } = new();
}

public class UpdateTaskRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    public ProjectTaskStatus Status { get; set; }

    public ProjectTaskPriority Priority { get; set; }

    [Required]
    public DateTime DueDate { get; set; }
}

public class UpdateTaskStatusRequest
{
    public ProjectTaskStatus Status { get; set; }
}

public class MoveTaskRequest
{
    [Required]
    public int ColumnId { get; set; }
}

public class AssignTaskRequest
{
    [Required]
    public string UserId { get; set; } = string.Empty;
}

public class TaskAssigneeResponse
{
    public string UserId { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string FullName { get; set; } = string.Empty;
}

public class TaskSummaryResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public int SprintId { get; set; }
    public int ColumnId { get; set; }
    public List<TaskAssigneeResponse> Assignees { get; set; } = new();
    public List<string> VisibilityReasons { get; set; } = new();
}

public class TaskDetailResponse : TaskSummaryResponse
{
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<TaskDependencyResponse> Dependencies { get; set; } = new();
}

public class AddTaskDependencyRequest
{
    [Required]
    public int DependencyTaskId { get; set; }
}

public class TaskDependencyResponse
{
    public int DependencyTaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class TaskActivityLogResponse
{
    public int Id { get; set; }
    public string FieldChanged { get; set; } = string.Empty;
    public string OldValue { get; set; } = string.Empty;
    public string NewValue { get; set; } = string.Empty;
    public string AppUserId { get; set; } = string.Empty;
    public string AppUserName { get; set; } = string.Empty; 
    public DateTime CreatedAt { get; set; }
}
