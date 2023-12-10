using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Vezeeta.Core.Models;
using Vezeeta.Core.Repositories;
using Vezeeta.Data;
using Vezeeta.Presentation.API.Models;
using DayOfWeek = Vezeeta.Core.Models.DayOfWeek;

namespace Vezeeta.Infrastructure.RepositoriesImplementation
{
    public class DoctorRepository : IDoctorRepository
    {
        private readonly VezeetaContext _context;

        public DoctorRepository(VezeetaContext context) { _context = context; }


        public HttpStatusCode ConfirmCheckUp(int bookingID)
        {
            var booking = _context.Bookings.Where(d => d.BookingID == bookingID).FirstOrDefault();
            if (booking != null)
            {
                booking.BookingStatus = Status.completed;
                _context.SaveChanges();
                return HttpStatusCode.OK;
            }
            else return HttpStatusCode.NotFound;
        }

        public HttpStatusCode Delete(int timeslotID)
        {
            var checkTimeSlot = _context.Bookings.
                Where(b => b.TimeSlotID == timeslotID).FirstOrDefault();

            if (checkTimeSlot != null)
            {
                return HttpStatusCode.Unauthorized;
            }
            else 
            {
                var timeslot = _context.TimeSlots.FirstOrDefault(t => t.SlotId == timeslotID);
                _context.TimeSlots.Remove(timeslot);
                _context.SaveChanges();
                return HttpStatusCode.OK;
            }
        }

        public HttpStatusCode UpdateAppointment(int timeslotID, TimeSpan time, string doctorID) 
        {
            var bookingCheck = _context.Bookings.Where(d => d.Timeslot.SlotId == timeslotID &&
          d.BookingStatus == Status.pending);

            if (bookingCheck.Any())
            {
                return HttpStatusCode.Unauthorized;
            }
            else
            {
                var timeslot = _context.TimeSlots.Where(a => a.SlotId == timeslotID).FirstOrDefault();
                if (timeslot == null)
                {
                    return HttpStatusCode.NotFound;
                }
                else
                {
                    timeslot.Time = time;
                    _context.SaveChanges();
                    return HttpStatusCode.OK;
                }
            }
        }

        public dynamic GetAll(string doctorId, DayOfWeek searchDate, int pageSize = 10, int pageNumber = 1)
        {
            var query = _context.Bookings
                .Join(
                    _context.Doctors,
                    booking => booking.DoctorID,
                    doctor => doctor.Id,
                    (booking, doctor) => new { Booking = booking, Doctor = doctor }
                )
                .Join(
                    _context.Users,
                    joined => joined.Booking.PatientID,
                    user => user.Id,
                    (joined, user) => new { Joined = joined, User = user }
                )
                .Join(
                    _context.TimeSlots,
                    joinedUser => joinedUser.Joined.Booking.TimeSlotID,
                    timeSlot => timeSlot.SlotId,
                    (joinedUser, timeSlot) => new { JoinedUser = joinedUser, TimeSlot = timeSlot }
                )
                .Join(
                    _context.Appointments,
                    joinedTimeSlot => joinedTimeSlot.TimeSlot.AppointmentID,
                    appointment => appointment.Id,
                    (joinedTimeSlot, appointment) => new { JoinedTimeSlot = joinedTimeSlot, Appointment = appointment }
                )
                .Where(joinedAppointment =>
                    joinedAppointment.JoinedTimeSlot.JoinedUser.Joined.Doctor.Id == doctorId &&
                  joinedAppointment.Appointment.Day == searchDate

                )
                .Select(joinedAppointment => new
                {
                    FullName = joinedAppointment.JoinedTimeSlot.JoinedUser.User.Fname + " " +
                               joinedAppointment.JoinedTimeSlot.JoinedUser.User.Lname,
                    Image = joinedAppointment.JoinedTimeSlot.JoinedUser.User.Image,
                    Gender = joinedAppointment.JoinedTimeSlot.JoinedUser.User.Gender,
                    PhoneNumber = joinedAppointment.JoinedTimeSlot.JoinedUser.User.PhoneNumber,
                    Email = joinedAppointment.JoinedTimeSlot.JoinedUser.User.Email,
                    Age = DateTime.Today.Year - joinedAppointment.JoinedTimeSlot.JoinedUser.User.DateOfBirth.Year,
                    DateTime = searchDate
                })
                .OrderByDescending(appointment => appointment.DateTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return query;
        }

        public HttpStatusCode Add(AddAppointmentDTO appointmentInfo)
        {
            var doctor = _context.Doctors.FirstOrDefault<Doctor>(d => d.Id == appointmentInfo.DoctorId);
            int lastAppointmentID = _context.Appointments.OrderByDescending(a => a.Id).FirstOrDefault()?.Id ?? 0;
            if (doctor != null)
            {
                doctor.Price = appointmentInfo.Price;

                foreach (KeyValuePair<DayOfWeek, List<TimeSpan>> entry in appointmentInfo.Times)
                {
                    DayOfWeek dayWeek = entry.Key;
                    List<TimeSpan> timeSlots = entry.Value;
                    _context.Appointments.Add(new Appointment() { Day = dayWeek, DoctorID = doctor.Id });
                    lastAppointmentID++;

                    _context.SaveChanges();

                    foreach (var item in timeSlots)
                    {
                        _context.TimeSlots.Add(new TimeSlot() { AppointmentID = lastAppointmentID, Time = item });
                        _context.SaveChanges();
                    }
                }
                _context.SaveChanges();
                return HttpStatusCode.Accepted;
            }
            return HttpStatusCode.Unauthorized;
        }
    }
}
