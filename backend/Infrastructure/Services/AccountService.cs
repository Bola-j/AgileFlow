using AgileFlow.Application.Interfaces;
using Application.DTOs.Account;
using Application.Interfaces;
using AutoMapper;

namespace Infrastructure.Services;

public class AccountService : IAccountService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public AccountService(
        IUserRepository userRepository,
        IMapper mapper)
    {
        _userRepository = userRepository;
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

    public async Task<AccountResponse?> UpdateProfilePictureAsync(string userId, string profilePictureUrl)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null || user.IsDeleted) return null;

        user.SetProfilePicture(profilePictureUrl);
        await _userRepository.UpdateAsync(user);
        return _mapper.Map<AccountResponse>(user);
    }
}
