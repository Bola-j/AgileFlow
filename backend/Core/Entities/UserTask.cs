using AgileFlow.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class UserTask
    {
        public string AppUserId { get; private set; } = string.Empty;
        public AppUser AppUser { get; private set; } = null!;

        public int ProjectTaskId { get; private set; }
        public ProjectTask ProjectTask { get; private set; } = null!;
        public bool IsDeleted { get; private set; }
        public DateTime? DeletedAt { get; private set; }

        private UserTask() { }

        public UserTask(string appUserId, int projectTaskId)
        {
            AppUserId = appUserId;
            ProjectTaskId = projectTaskId;
        }
        public void Delete()
        {
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
        }
    }
}
