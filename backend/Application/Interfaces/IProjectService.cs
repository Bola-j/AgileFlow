using Application.DTOs.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IProjectService
    {
        Task<IEnumerable<ProjectResponse>> GetByWorkspaceIdAsync(int workspaceId, string userId);
        Task<ProjectResponse?> GetByIdAsync(int id, string userId);
        Task<ProjectResponse> CreateAsync(CreateProjectRequest request, string userId);
        Task<ProjectResponse?> UpdateAsync(int id, UpdateProjectRequest request, string userId);
        Task<bool> DeleteAsync(int id, string userId);
    }
}
