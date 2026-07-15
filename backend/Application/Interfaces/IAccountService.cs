using Application.DTOs.Account;

namespace Application.Interfaces;

public interface IAccountService
{
    Task<AccountResponse?> GetMeAsync(string userId);
    Task<AccountResponse?> UpdateMeAsync(string userId, UpdateAccountRequest request);
    Task<AccountResponse?> UpdateProfilePictureAsync(string userId, string profilePictureUrl);
}
