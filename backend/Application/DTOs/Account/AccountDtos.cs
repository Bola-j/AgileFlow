using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Account
{
    public class AccountResponse
    {
        public string UserId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? ProfilePicture { get; set; }
        public DateOnly? Dob { get; set; }
        public string? GithubUsername { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class UpdateAccountRequest
    {
        [Required(ErrorMessage = "FirstName is required.")]
        [MaxLength(50, ErrorMessage = "FirstName cannot exceed 50 characters.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "LastName is required.")]
        [MaxLength(50, ErrorMessage = "LastName cannot exceed 50 characters.")]
        public string LastName { get; set; } = string.Empty;

        [Phone(ErrorMessage = "PhoneNumber must be a valid phone number.")]
        public string? PhoneNumber { get; set; }

        [MaxLength(500, ErrorMessage = "ProfilePicture cannot exceed 500 characters.")]
        public string? ProfilePicture { get; set; }

        public DateOnly? Dob { get; set; }

        [MaxLength(100, ErrorMessage = "GithubUsername cannot exceed 100 characters.")]
        public string? GithubUsername { get; set; }
    }
}
