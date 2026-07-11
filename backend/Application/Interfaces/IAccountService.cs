using Application.DTOs.Account;

namespace Application.Interfaces;

public interface IAccountService
{
    Task<AccountResponse?> GetMeAsync(string userId);
    Task<AccountResponse?> UpdateMeAsync(string userId, UpdateAccountRequest request);
    Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequest request);
    Task<bool> ChangeEmailAsync(string userId, ChangeEmailRequest request);
}
