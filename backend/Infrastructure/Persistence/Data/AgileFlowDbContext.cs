using AgileFlow.Domain.Entities;
using Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace AgileFlow.Infrastructure.Persistence.Data 
{
    public class AgileFlowDbContext : IdentityDbContext<AppUser>
    {
        public AgileFlowDbContext(DbContextOptions<AgileFlowDbContext> options) : base(options)
        {
        }

        public DbSet<Workspace> Workspaces { get; set; }
        public DbSet<UserWorkspace> UserWorkspaces { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Board> Boards { get; set; }
        public DbSet<BoardColumn> BoardColumns { get; set; }
        public DbSet<Sprint> Sprints { get; set; }
        public DbSet<ProjectTask> ProjectTasks { get; set; }
        public DbSet<UserTask> UserTasks { get; set; }
        public DbSet<TaskDependent> TaskDependents { get; set; }
        public DbSet<Commit> Commits { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<TaskActivityLog> TaskActivityLogs { get; set; }
        public DbSet<Notification> Notifications { get; set; }


        public DbSet<RefreshToken> RefreshTokens { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
