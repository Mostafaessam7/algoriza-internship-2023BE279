using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vezeeta.Core.Models
{
    public enum Status { pending, completed, canceled }
    public class Booking
    {
        public int? BookingID { get; set; }
        public string? PatientID { get; set; }
        public User? Patient
        { get; set; }

        public string? DoctorID { get; set; }
        public Doctor? Doctor
        { get; set; }

        public int? DiscountId { get; set; } 
        public Discount? Discount { get; set; }

        public float? FinalPrice { get; set; }

        public int? TimeSlotID { get; set; }
        public TimeSlot? Timeslot { get; set; }
        public Status? BookingStatus { get; set; }



    }
}
