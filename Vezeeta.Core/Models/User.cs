using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
namespace Vezeeta.Core.Models
{
    public enum Gender { female, male }
    public enum UserType { admin, doctor, patient }
    public class User : IdentityUser
    {
        public string? UserId { get; set; }
        public string? Fname { get; set; }
        [Required]
        public string? Lname { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]

        public string? Email { get; set; }
        [Required(ErrorMessage = "Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]

        public string? Password { get; set; }
        public string? Image { get; set; }

        public string? PhoneNumber { get; set; }

        public UserType? Type { get; set; }

        [Required]
        public Gender Gender { get; set; }
        [Required]

        public DateTime DateOfBirth { get; set; }
    }
}
