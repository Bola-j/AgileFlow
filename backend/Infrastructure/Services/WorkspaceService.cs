using AgileFlow.Application.Interfaces;
using Application.DTOs.Workspace;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.Services
{
    public class WorkspaceService : IWorkspaceService
    {
        private static readonly UserRole[] WorkspaceManagerRoles = { UserRole.Admin, UserRole.TeamLead };

        private readonly IWorkspaceRepository _workspaceRepository;
        private readonly IUserWorkspaceRepository _userWorkspaceRepository;
        private readonly IWorkspaceAuthorizationService _authorizationService;
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;

        public WorkspaceService(
            IWorkspaceRepository workspaceRepository,
            IUserWorkspaceRepository userWorkspaceRepository,
            IWorkspaceAuthorizationService authorizationService,
            IMapper mapper, IUserRepository userRepository)
        {
            _workspaceRepository = workspaceRepository;
            _userWorkspaceRepository = userWorkspaceRepository;
            _authorizationService = authorizationService;
            _mapper = mapper;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<WorkspaceSummaryResponse>> GetMyWorkspacesAsync(string userId)
        {
            var workspaces = await _workspaceRepository.GetByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<WorkspaceSummaryResponse>>(workspaces);
        }

        public async Task<WorkspaceResponse?> GetByIdAsync(int id, string userId)
        {
            var workspace = await _workspaceRepository.GetByIdWithDetailsAsync(id);
            if (workspace is null) return null;

            await _authorizationService.EnsureMemberAsync(id, userId);

            return _mapper.Map<WorkspaceResponse>(workspace);
        }

        public async Task<WorkspaceResponse> CreateAsync(CreateWorkspaceRequest request, string userId)
        {

            if (await _workspaceRepository.NameExistsAsync(request.Name, userId))
                throw new InvalidOperationException($"You already have a workspace named '{request.Name}'.");

            var workspace = new Workspace(request.Name, request.Description ?? string.Empty);

            await _workspaceRepository.AddAsync(workspace);

            var userWorkspace = new UserWorkspace(userId, workspace.Id, UserRole.Admin);
            workspace.UserWorkspaces.Add(userWorkspace); 

            await _workspaceRepository.UpdateAsync(workspace);

            var created = await _workspaceRepository.GetByIdWithDetailsAsync(workspace.Id);
            return _mapper.Map<WorkspaceResponse>(created!);
        }

        public async Task<WorkspaceResponse?> UpdateAsync(int id, UpdateWorkspaceRequest request, string userId)
        {
            var workspace = await _workspaceRepository.GetByIdWithDetailsAsync(id);
            if (workspace is null) return null;

            await _authorizationService.EnsureRoleAsync(id, userId, WorkspaceManagerRoles);

            if (await _workspaceRepository.NameExistsAsync(request.Name, userId, excludeId: id))
                throw new InvalidOperationException($"You already have a workspace named '{request.Name}'.");

            workspace.UpdateName(request.Name);
            if (request.Description is not null)
                workspace.UpdateDescription(request.Description);

            await _workspaceRepository.UpdateAsync(workspace);

            var updated = await _workspaceRepository.GetByIdWithDetailsAsync(id);
            return _mapper.Map<WorkspaceResponse>(updated!);
        }

        public async Task<bool> DeleteAsync(int id, string userId)
        {
            var workspace = await _workspaceRepository.GetByIdWithDetailsAsync(id);
            if (workspace is null) return false;

            await _authorizationService.EnsureRoleAsync(id, userId, WorkspaceManagerRoles);

            await _workspaceRepository.DeleteAsync(workspace);
            return true;
        }

        public async Task AddMemberAsync(int workspaceId, AddWorkspaceMemberRequest request, string currentUserId)
        {
            await _authorizationService.EnsureRoleAsync(workspaceId, currentUserId, WorkspaceManagerRoles);
            var targetUser = await ResolveUserAsync(request.Email, request.UserId);
            if (targetUser is null)
                throw new KeyNotFoundException($"User with email '{request.Email}' does not exist in the system.");
            var workspace = await _workspaceRepository.GetByIdAsync(workspaceId);
            if (workspace is null)
                throw new KeyNotFoundException("Workspace not found.");
            var membership = await _userWorkspaceRepository.GetMembershipAsync(workspaceId, targetUser.Id);
            if (membership is not null)
            {
                if (!membership.IsDeleted)
                    throw new InvalidOperationException("User is already an active member of this workspace.");
                membership.Restore(request.Role);
                await _userWorkspaceRepository.UpdateAsync(membership);
                return;
            }
            var newMembership = new UserWorkspace(targetUser.Id, workspaceId, request.Role);
            await _userWorkspaceRepository.AddAsync(newMembership);
        }


        public async Task UpdateMemberRoleAsync(int workspaceId, string memberUserId, UpdateWorkspaceMemberRoleRequest request, string currentUserId)
        {
            await _authorizationService.EnsureRoleAsync(workspaceId, currentUserId, WorkspaceManagerRoles);

            var membership = await _userWorkspaceRepository.GetMembershipAsync(workspaceId, memberUserId);
            if (membership is null || membership.IsDeleted)
                throw new KeyNotFoundException("Member not found in this workspace.");
            if (IsWorkspaceManager(membership.Role) && !IsWorkspaceManager(request.Role))
            {
                var workspace = await _workspaceRepository.GetByIdWithDetailsAsync(workspaceId);
                var activeManagerCount = workspace!.UserWorkspaces.Count(uw => IsWorkspaceManager(uw.Role) && !uw.IsDeleted);

                if (activeManagerCount <= 1)
                    throw new InvalidOperationException("Cannot demote the last Admin or TeamLead in this workspace.");
            }

            membership.UpdateRole(request.Role);
            await _userWorkspaceRepository.UpdateAsync(membership);
        }

        public async Task RemoveMemberAsync(int workspaceId, string memberUserId, string currentUserId)
        {
            await _authorizationService.EnsureRoleAsync(workspaceId, currentUserId, WorkspaceManagerRoles);
            memberUserId = await ResolveUserIdAsync(memberUserId);
            if (memberUserId == currentUserId)
                throw new InvalidOperationException("You cannot remove yourself from the workspace.");
            var membership = await _userWorkspaceRepository.GetMembershipAsync(workspaceId, memberUserId);
            if (membership is null || membership.IsDeleted)
                throw new KeyNotFoundException("Member not found in this workspace.");
            if (IsWorkspaceManager(membership.Role))
            {
                var workspace = await _workspaceRepository.GetByIdWithDetailsAsync(workspaceId);
                var activeManagerCount = workspace!.UserWorkspaces.Count(uw => IsWorkspaceManager(uw.Role) && !uw.IsDeleted);

                if (activeManagerCount <= 1)
                    throw new InvalidOperationException("Cannot remove the last Admin or TeamLead from this workspace.");
            }
            membership.Delete();
            await _userWorkspaceRepository.UpdateAsync(membership);
        }

        public async Task UpdateMemberProfileByAdminAsync(int workspaceId, string memberUserId, UpdateMemberProfileByAdminRequest request, string currentUserId)
        {
            await _authorizationService.EnsureRoleAsync(workspaceId, currentUserId, WorkspaceManagerRoles);
            var membership = await _userWorkspaceRepository.GetMembershipAsync(workspaceId, memberUserId);
            if (membership is null || membership.IsDeleted)
                throw new KeyNotFoundException("The target user is not an active member of this workspace.");
            var user = await _userRepository.GetByIdAsync(memberUserId);
            if (user is null || user.IsDeleted)
                throw new KeyNotFoundException("User not found in the system.");
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
            else if (request.ClearDob)
                user.ClearDOB();

            if (request.GithubUsername is not null)
            {
                if (string.IsNullOrWhiteSpace(request.GithubUsername))
                    user.SetGithubUsername(string.Empty);
                else
                    user.SetGithubUsername(request.GithubUsername);
            }

            await _userRepository.UpdateAsync(user);
        }

        private static bool IsWorkspaceManager(UserRole role)
        {
            return role is UserRole.Admin or UserRole.TeamLead;
        }

        private async Task<AgileFlow.Domain.Entities.AppUser?> ResolveUserAsync(string? email, string? userId)
        {
            if (!string.IsNullOrWhiteSpace(email))
                return await _userRepository.GetByEmailAsync(email.Trim());

            if (!string.IsNullOrWhiteSpace(userId))
                return await _userRepository.GetByIdAsync(userId.Trim());

            throw new ArgumentException("Member email is required.");
        }

        private async Task<string> ResolveUserIdAsync(string userIdOrEmail)
        {
            var decoded = Uri.UnescapeDataString(userIdOrEmail).Trim();
            if (!decoded.Contains('@'))
                return decoded;

            var user = await _userRepository.GetByEmailAsync(decoded);
            return user?.Id ?? throw new KeyNotFoundException($"User with email '{decoded}' does not exist in the system.");
        }

        public async Task<WorkspaceMemberDetailResponse> GetWorkspaceMemberDetailAsync(int workspaceId, string memberUserId, string currentUserId)
        {
            await _authorizationService.EnsureMemberAsync(workspaceId, currentUserId);
            var membership = await _userWorkspaceRepository.GetMembershipAsync(workspaceId, memberUserId);
            if (membership is null || membership.IsDeleted)
                throw new KeyNotFoundException("Member not found or inactive in this workspace.");
            return _mapper.Map<WorkspaceMemberDetailResponse>(membership);
        }
    }
}
