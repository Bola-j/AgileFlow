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

    //public async Task<AccountResponse?> UpdateMeAsync(string userId, UpdateAccountRequest request)
    //{
    //    var user = await _userRepository.GetByIdAsync(userId);
    //    if (user is null) return null;

    //    user.UpdateFirstName(request.FirstName);
    //    user.UpdateLastName(request.LastName);
    //    user.PhoneNumber = request.PhoneNumber;
    //    user.SetProfilePicture(request.ProfilePicture ?? string.Empty);

    //    if (request.Dob.HasValue)
    //        user.SetDOB(request.Dob.Value);

    //    if (string.IsNullOrWhiteSpace(request.GithubUsername))
    //    {
    //        user.SetGithubUsername(string.Empty);
    //    }
    //    else
    //    {
    //        user.SetGithubUsername(request.GithubUsername);
    //    }

    //    if (request.Dob is null && user.DOB is not null)
    //    {
    //        user.ClearDOB();
    //    }

    //    if (request.ProfilePicture is null && user.Profile_Picture is not null)
    //    {
    //        user.ClearProfilePicture();
    //    }

    //    var result = await _userManager.UpdateAsync(user);
    //    if (!result.Succeeded)
    //        throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));

    //    return _mapper.Map<AccountResponse>(user);
    //}


    public async Task<AccountResponse?> UpdateMeAsync(string userId, UpdateAccountRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null || user.IsDeleted) return null;

        if (!string.IsNullOrWhiteSpace(request.FirstName))
            user.UpdateFirstName(request.FirstName);

        if (!string.IsNullOrWhiteSpace(request.LastName))
            user.UpdateLastName(request.LastName);

        if (request.PhoneNumber is not null)
        {
            if (string.IsNullOrWhiteSpace(request.PhoneNumber))
                user.ClearPhoneNumber();
            else
                user.SetPhoneNumber(request.PhoneNumber);
        }

        if (request.ProfilePicture is not null)
        {
            if (string.IsNullOrWhiteSpace(request.ProfilePicture))
                user.ClearProfilePicture();
            else
                user.SetProfilePicture(request.ProfilePicture);
        }

        if (request.Dob.HasValue)
            user.SetDOB(request.Dob.Value);
        else if (request.Dob == null)
            user.ClearDOB();

        if (request.GithubUsername is not null)
        {
            if (string.IsNullOrWhiteSpace(request.GithubUsername))
                user.SetGithubUsername(string.Empty);
            else
                user.SetGithubUsername(request.GithubUsername);
        }

        await _userRepository.UpdateAsync(user);
        return _mapper.Map<AccountResponse>(user);
    }


    public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null || user.IsDeleted) return false;

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));

        return true;
    }

    public async Task<bool> ChangeEmailAsync(string userId, ChangeEmailRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null || user.IsDeleted) 
            return false;

        var existingUser = await _userManager.FindByEmailAsync(request.NewEmail);
        if (existingUser != null && existingUser.Id != user.Id)
            throw new InvalidOperationException("Email is already taken by another user.");

        var setEmailResult = await _userManager.SetEmailAsync(user, request.NewEmail);
        if (!setEmailResult.Succeeded)
            throw new InvalidOperationException(string.Join("; ", setEmailResult.Errors.Select(e => e.Description)));

        var setUserNameResult = await _userManager.SetUserNameAsync(user, request.NewEmail);
        if (!setUserNameResult.Succeeded)
            throw new InvalidOperationException(string.Join("; ", setUserNameResult.Errors.Select(e => e.Description)));

        await _userRepository.UpdateAsync(user);
        return true;
    }
}
