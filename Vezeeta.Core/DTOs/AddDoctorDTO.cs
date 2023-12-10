using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vezeeta.Core.Models;

namespace Vezeeta.Core.DTOs
{
    public class AddDoctorDTO
    {

        [Required]
        public string? Image { get; set; }
        [Required]
        public string? Fname { get; set; }
        [Required]
        public string? Lname { get; set; }
        [Required]
        public string? Email { get; set; }
        [Required]
        public string? Phone { get; set; }
        [Required]
        public int SpecializationID { get; set; }
        [Required]
        public float Price { get; set; }
        public Gender? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }


    }
}
