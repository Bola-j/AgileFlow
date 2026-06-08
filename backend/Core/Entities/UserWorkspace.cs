using AgileFlow.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class UserWorkspace
    {
        public string AppUserId { get; private set; }
        public AppUser AppUser { get; private set; }
        public int WorkspaceId { get; private set; }
        public Workspace Workspace { get; private set; }
        public DateTime JoinedAt { get; private set; }
        public bool IsDeleted { get; private set; }
        public DateTime? DeletedAt { get; private set; }

        private UserWorkspace() { }

        public UserWorkspace(string appUserId, int workspaceId)
        {
            AppUserId = appUserId;
            WorkspaceId = workspaceId;
            JoinedAt = DateTime.UtcNow;
        }
        public void Delete()
        {
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
        }
    }
}
