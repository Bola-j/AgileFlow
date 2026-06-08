using AgileFlow.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class Comment : BaseEntity
    {
        public string Content { get; private set; } = string.Empty;
        public int ProjectTaskId { get; private set; }
        public ProjectTask ProjectTask { get; private set; } = null!;
        public string AppUserId { get; private set; } = string.Empty;
        public AppUser AppUser { get; private set; } = null!;


        private Comment() { }

        public Comment(string content, int projectTaskId, string appUserId)
        {
            Content = content;
            ProjectTaskId = projectTaskId;
            AppUserId = appUserId;
        }

        public void UpdateContent(string content) 
        { 
            Content = content; 
            Update();
        }

    }
}
