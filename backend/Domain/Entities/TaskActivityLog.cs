using AgileFlow.Domain.Entities;
using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class TaskActivityLog : BaseEntity
    {
        public string FieldChanged { get; private set; } = string.Empty;
        public string NewValue { get; private set; } = string.Empty;
        public string OldValue { get; private set; } = string.Empty;
        public int ProjectTaskId { get; private set; }
        public ProjectTask ProjectTask { get; private set; } = null!;
        public string AppUserId { get; private set; } = string.Empty;
        public AppUser AppUser { get; private set; } = null!;

        private TaskActivityLog() { }

        public TaskActivityLog(string fieldChanged, int projectTaskId, string appUserId,
                               string oldValue, string newValue)
        {
            FieldChanged = fieldChanged;
            ProjectTaskId = projectTaskId;
            AppUserId = appUserId;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
