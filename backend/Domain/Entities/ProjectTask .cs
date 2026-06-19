using AgileFlow.Domain.Entities;
using Domain.Enums;
using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Domain.Entities;

namespace AgileFlow.Domain.Entities
{
    public class ProjectTask : BaseEntity
    {
        public string Title { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public ProjectTaskStatus Status { get; private set; }
        public ProjectTaskPriority Priority { get; private set; }
        public ProjectTaskApprovalStatus? ApprovalStatus { get; private set; }
        public DateTime DueDate { get; private set; }
        public int SprintId { get; private set; }
        public Sprint? Sprint { get; private set; }

        public int ColumnId { get; private set; }
        public BoardColumn Column { get; private set; } 


        private ProjectTask() { }

        public ProjectTask(string title, ProjectTaskStatus status, ProjectTaskPriority priority,
                           int columnId, DateTime dueDate,string description,int sprintId)
        {
            Title = title;
            Status = status;
            Priority = priority;
            ColumnId = columnId;
            Description = description;
            DueDate = dueDate;
            SprintId = sprintId;
        }

        public void UpdateTitle(string title) 
        { 
            Title = title; 
            Update(); 
        }
        public void UpdateDescription(string description) 
        { 
            Description = description; 
            Update(); 
        }
        public void UpdateStatus(ProjectTaskStatus status) 
        {
            Status = status; 
            Update(); 
        }
        public void UpdatePriority(ProjectTaskPriority priority) 
        { 
            Priority = priority; 
            Update(); 
        }
        public void UpdateApprovalStatus(ProjectTaskApprovalStatus approvalStatus) 
        { 
            ApprovalStatus = approvalStatus; 
            Update(); 
        }
        public void UpdateDueDate(DateTime dueDate)
        {
            DueDate = dueDate;
            Update(); 
        }
        public void UpdateColumn(int columnId) 
        { 
            ColumnId = columnId; 
            Update(); }
        public void UpdateSprint(int sprintId) 
        { 
            SprintId = sprintId; 
            Update(); 
        }

        public ICollection<UserTask> UserTasks { get; private set; } = new List<UserTask>();
        public ICollection<Commit> Commits { get; private set; } = new List<Commit>();
        public ICollection<Comment> Comments { get; private set; } = new List<Comment>();
        public ICollection<TaskActivityLog> TaskActivityLogs { get; private set; } = new List<TaskActivityLog>();
        public ICollection<TaskDependent> TaskDependents { get; private set; } = new List<TaskDependent>();
    }
}
