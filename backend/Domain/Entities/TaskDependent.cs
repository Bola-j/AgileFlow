using AgileFlow.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class TaskDependent
    {
        public int TaskId { get; private set; }
        public ProjectTask Task { get; private set; } = null!;

        public int DependedTaskId { get; private set; }
        public ProjectTask DependedTask { get; private set; } = null!;

        private TaskDependent() { }

        public TaskDependent(int taskId, int dependedTaskId)
        {
            TaskId = taskId;
            DependedTaskId = dependedTaskId;
        }
    }
}
