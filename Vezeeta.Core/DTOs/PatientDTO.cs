using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vezeeta.Core.Models;

namespace Vezeeta.Core.DTOs
{
    public class PatientDTO
    {
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

        [Required]
        public Gender Gender { get; set; }
        [Required]

        public DateTime DateOfBirth { get; set; }
    }
}
