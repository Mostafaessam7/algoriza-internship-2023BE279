using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vezeeta.Core.Models
{
    public enum DayOfWeek { Saturday, Sunday, Monday, Tuesday, Wednesday, Thursday, Friday }
    public class Appointment
    {
        public int Id { get; set; }
        public string DoctorID { get; set; }
        public Doctor Doctor { get; set; }

        public DayOfWeek Day { get; set; }
        List<TimeSlot> TimeSlots { get; set; }

    }
}
