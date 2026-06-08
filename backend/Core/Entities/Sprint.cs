using Core.Entities;
using Core.Enums;

namespace AgileFlow.Core.Entities 
{
    public class Sprint : BaseEntity
    {
        public string Name { get; private set; } = string.Empty;
        public string Goal { get; private set; }
        public SprintStatus Status { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public int ProjectId { get; private set; }
        public Project Project { get; private set; } = null!;


        private Sprint() { }

        public Sprint(string name, SprintStatus status, DateTime startDate, int projectId,
                      string goal, DateTime endDate)
        {
            Name = name;
            Status = status;
            StartDate = startDate;
            ProjectId = projectId;
            Goal = goal;
            EndDate = endDate;
        }

        public void UpdateName(string name)
        {
            Name = name;
            Update();
        }

        public void UpdateGoal(string goal)
        {
            Goal = goal;
            Update();
        }

        public void UpdateStatus(SprintStatus status)
        {
            Status = status;
            Update();
        }

        public void UpdateEndDate(DateTime endDate)
        {
            EndDate = endDate;
            Update();
        }

        public ICollection<ProjectTask> Tasks { get; private set; } = new List<ProjectTask>();
    }
}
