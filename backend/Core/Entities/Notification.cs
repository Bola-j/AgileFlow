using AgileFlow.Core.Entities;
using Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class Notification : BaseEntity
    {
        public string Title { get; private set; } = string.Empty;
        public string Message { get; private set; } = string.Empty;
        public NotificationType Type { get; private set; }
        public bool IsRead { get; private set; }
        public string AppUserId { get; private set; } = string.Empty;
        public AppUser AppUser { get; private set; } = null!;

        private Notification() { }

        public Notification(string title, string message, NotificationType type, string appUserId)
        {
            Title = title;
            Message = message;
            Type = type;
            AppUserId = appUserId;
            IsRead = false;
        }

        public void MarkAsRead() 
        {
            IsRead = true; 
            Update(); 
        }
    }
}
