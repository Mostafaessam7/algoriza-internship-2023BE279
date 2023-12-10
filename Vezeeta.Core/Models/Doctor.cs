using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vezeeta.Core.Models
{
    public class Doctor : User
    {
        public string? Doctorid { get; set; }
        public float Price { get; set; }
        public int SpecializationID { get; set; }
        public Specialization Specialization { get; set; }
        public List<Appointment> Doctors_Appointments { get; set; }
    }
}
