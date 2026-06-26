using Domain.Common;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace AgileFlow.Domain.Entities
{
    public class AppUser : IdentityUser
    {
        public string First_Name { get; private set; } = String.Empty;
        public string Last_Name { get; private set; } = String.Empty;
        public string? Profile_Picture { get; private set; }
        public DateOnly? DOB { get; private set; }
        public string? Github_Username { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
        public bool IsDeleted { get; private set; }
        public DateTime? DeletedAt { get; private set; }

        private AppUser() { }

        public AppUser(string firstName, string lastName, string email,string? profilePicture = null,DateOnly? dob = null,
                       string? githubUsername = null,string? phone= null)
        {
            First_Name = firstName;
            Last_Name = lastName;
            Email = email;
            Profile_Picture = profilePicture;
            DOB = dob;
            PhoneNumber = phone;
            Github_Username = githubUsername;
            CreatedAt = DateTime.UtcNow;
        }

        public void SetProfilePicture(string profilePicture)
        {
            Profile_Picture = profilePicture;
            Update();
        }

        public void ClearProfilePicture()
        {
            Profile_Picture = null;
            Update();
        }

        public void SetDOB(DateOnly dob)
        {
            DOB = dob;
            Update();
        }

        public void ClearDOB()
        {
            DOB = null;
            Update();
        }

        public void SetGithubUsername(string githubUsername)
        {
            Github_Username = githubUsername;
            Update();
        }
        public void Delete()
        {
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
        }

        public void Update()
        {
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateFirstName(string firstName)
        {
            First_Name = firstName;
            Update();
        }

        public void UpdateLastName(string lastName)
        {
            Last_Name = lastName;
            Update();
        }

        public ICollection<UserWorkspace> UserWorkspaces { get; private set; } = new List<UserWorkspace>();
        public ICollection<UserTask> UserTasks { get; private set; } = new List<UserTask>();
        public ICollection<Commit> Commits { get; private set; } = new List<Commit>();
        public ICollection<Comment> Comments { get; private set; } = new List<Comment>();
        public ICollection<Notification> Notifications { get; private set; } = new List<Notification>();
        public ICollection<TaskActivityLog> TaskActivityLogs { get; private set; } = new List<TaskActivityLog>();
    }

}
