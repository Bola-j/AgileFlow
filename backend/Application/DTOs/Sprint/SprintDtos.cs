using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Sprint
{
    public class CreateSprintRequest
    {
        [Required(ErrorMessage = "Name is required.")]
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Goal is required.")]
        [MaxLength(500, ErrorMessage = "Goal cannot exceed 500 characters.")]
        public string Goal { get; set; } = string.Empty;

        [Required(ErrorMessage = "StartDate is required.")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "EndDate is required.")]
        public DateTime EndDate { get; set; }
    }

    public class UpdateSprintRequest
    {
        [Required(ErrorMessage = "Name is required.")]
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Goal is required.")]
        [MaxLength(500, ErrorMessage = "Goal cannot exceed 500 characters.")]
        public string Goal { get; set; } = string.Empty;

        [Required(ErrorMessage = "EndDate is required.")]
        public DateTime EndDate { get; set; }
    }

    public class SprintResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Goal { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int ProjectId { get; set; }
        public int TaskCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class SprintProgressResponse
    {
        public int SprintId { get; set; }
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public decimal ProgressPercentage { get; set; }
    }
}
