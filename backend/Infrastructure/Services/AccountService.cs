using AgileFlow.Application.Interfaces;
using AgileFlow.Domain.Entities;
using Application.DTOs.Account;
using Application.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Services;

public class AccountService : IAccountService
{
    private readonly IUserRepository _userRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly IMapper _mapper;

    public AccountService(
        IUserRepository userRepository,
        UserManager<AppUser> userManager,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _userManager = userManager;
        _mapper = mapper;
    }

    public async Task<AccountResponse?> GetMeAsync(string userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        return user is null ? null : _mapper.Map<AccountResponse>(user);
    }

    public async Task<AccountResponse?> UpdateMeAsync(string userId, UpdateAccountRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null) return null;

        user.UpdateFirstName(request.FirstName);
        user.UpdateLastName(request.LastName);
        user.PhoneNumber = request.PhoneNumber;
        user.SetProfilePicture(request.ProfilePicture ?? string.Empty);

        if (request.Dob.HasValue)
            user.SetDOB(request.Dob.Value);

        if (string.IsNullOrWhiteSpace(request.GithubUsername))
        {
            user.SetGithubUsername(string.Empty);
        }
        else
        {
            user.SetGithubUsername(request.GithubUsername);
        }

        if (request.Dob is null && user.DOB is not null)
        {
            user.ClearDOB();
        }

        if (request.ProfilePicture is null && user.Profile_Picture is not null)
        {
            user.ClearProfilePicture();
        }

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));

        return _mapper.Map<AccountResponse>(user);
    }
}
