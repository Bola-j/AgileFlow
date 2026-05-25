using AgileFlow.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgileFlow.Infrastructure.Persistence;

public class AgileFlowDbContext : DbContext
{
    public AgileFlowDbContext(DbContextOptions<AgileFlowDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Board> Boards => Set<Board>();
    public DbSet<Sprint> Sprints => Set<Sprint>();
    public DbSet<Issue> Issues => Set<Issue>();
}
