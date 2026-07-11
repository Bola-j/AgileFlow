using AgileFlow.Application.Interfaces;
using AgileFlow;
using AgileFlow.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using AgileFlow.Domain.Entities;

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

    public async Task UpdateAsync(AppUser user)
    {
        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync();
    }
}

