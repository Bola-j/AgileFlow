using AgileFlow.Core.Entities;

namespace AgileFlow.Core.Interfaces;

public interface IUserRepository
{
    Task<AppUser?> GetByIdAsync(string id);
    Task<AppUser?> GetByEmailAsync(string email);
}
