using AgileFlow.Domain.Entities;

namespace AgileFlow.Application.Interfaces;

public interface IUserRepository
{
    Task<AppUser?> GetByIdAsync(string id);
    Task<AppUser?> GetByEmailAsync(string email);
    Task UpdateAsync(AppUser user);
}
