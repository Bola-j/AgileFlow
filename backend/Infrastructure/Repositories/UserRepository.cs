using AgileFlow.Core.Entities;
using AgileFlow.Core.Interfaces;
using AgileFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgileFlow.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AgileFlowDbContext _dbContext;

    public UserRepository(AgileFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<User?> GetByIdAsync(Guid id) =>
        _dbContext.Users.FirstOrDefaultAsync(user => user.Id == id);

    public Task<User?> GetByEmailAsync(string email) =>
        _dbContext.Users.FirstOrDefaultAsync(user => user.Email == email);
}
