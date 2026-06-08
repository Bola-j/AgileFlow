using AgileFlow.Core.Entities;
using Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class Commit : BaseEntity
    {
        public string Message { get; private set; } = string.Empty;
        public string BranchName { get; private set; } = string.Empty;
        public string CommitHash { get; private set; } = string.Empty;
        public string URL { get; private set; } = string.Empty;
        public CommitStatus Status { get; private set; }
        public int ProjectTaskId { get; private set; }
        public ProjectTask ProjectTask { get; private set; } = null!;
        public string AppUserId { get; private set; } = string.Empty;
        public AppUser AppUser { get; private set; } = null!;

        private Commit() { }

        public Commit(string message, string branchName, string commitHash,
                      string url, CommitStatus status, int projectTaskId, string appUserId)
        {
            Message = message;
            BranchName = branchName;
            CommitHash = commitHash;
            URL = url;
            Status = status;
            ProjectTaskId = projectTaskId;
            AppUserId = appUserId;
        }

        public void UpdateStatus(CommitStatus status) 
        { 
            Status = status; 
            Update(); 
        }

    }
}
