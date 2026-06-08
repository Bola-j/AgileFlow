using Core.Entities;
using Core.Enums;

namespace AgileFlow.Core.Entities 
{
    public class Project : BaseEntity
    {
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; }
        public ProjectStatus Status { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public int WorkspaceId { get; private set; }
        public Workspace Workspace { get; private set; } = null!;

        private Project() { }

        public Project(string name, ProjectStatus status, DateTime startDate, int workspaceId,
                       string description, DateTime endDate)
        {
            Name = name;
            Status = status;
            StartDate = startDate;
            WorkspaceId = workspaceId;
            Description = description;
            EndDate = endDate;
        }

        public void UpdateName(string name)
        {
            Name = name;
            Update();
        }

        public void UpdateDescription(string description)
        {
            Description = description;
            Update();
        }

        public void UpdateStatus(ProjectStatus status)
        {
            Status = status;
            Update();
        }

        public void UpdateEndDate(DateTime endDate)
        {
            EndDate = endDate;
            Update();
        }
        public ICollection<Board> Boards { get; private set; } = new List<Board>();
        public ICollection<Sprint> Sprints { get; private set; } = new List<Sprint>();
    }
}