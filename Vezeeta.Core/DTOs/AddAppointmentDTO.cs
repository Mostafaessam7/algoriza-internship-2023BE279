using DayOfWeek = Vezeeta.Core.Models.DayOfWeek;

namespace Vezeeta.Presentation.API.Models
{
    public class AddAppointmentDTO
    {
        public string? DoctorId { get; set; }
        public float Price { get; set; }
        public IDictionary<DayOfWeek, List<TimeSpan>>? Times { get; set; }


    }
}
