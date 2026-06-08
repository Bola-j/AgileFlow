using AgileFlow.Core.Entities;
using AgileFlow.Core.Interfaces;
using Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace AgileFlow.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AgileFlowDbContext _dbContext;

    public UserRepository(AgileFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<AppUser?> GetByIdAsync(string id) =>
        _dbContext.Users.FirstOrDefaultAsync(user => user.Id == id);

    public Task<AppUser?> GetByEmailAsync(string email) =>
        _dbContext.Users.FirstOrDefaultAsync(user => user.Email == email);
}
