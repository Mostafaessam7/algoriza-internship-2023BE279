using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net;
using Vezeeta.Core.DTOs;
using Vezeeta.Core.Models;
using Vezeeta.Core.Repositories;
using Vezeeta.Data;

namespace Vezeeta.Infrastructure.RepositoriesImplementation
{
    public class PatientRepository : IPatientRepository
    {
        private readonly VezeetaContext context;
        private readonly UserManager<IdentityUser> _userManager;

        public PatientRepository(VezeetaContext context, UserManager<IdentityUser> userManager)
        {
            this.context = context;
            _userManager = userManager;
        }

        public HttpStatusCode booking(string patientID, int SlotID, int DiscountID = 4)
        {
            int finalPrice = 0;
            if (SlotID != 0)
            {
                var query = from appointment in context.Appointments
                            join timeslot in context.TimeSlots
                            on appointment.Id equals timeslot.AppointmentID
                            select new
                            {
                                appoint = appointment,
                                time = timeslot
                            };
                var result = query.ToList();

                var targetedTimeSlot = result.Where(a => a.time.SlotId == SlotID);
                var doctorID = targetedTimeSlot.Select(d => d.appoint.DoctorID).FirstOrDefault();

                if (!IsTimeSlotBooked(doctorID, SlotID))
                {
                    var countOfRequests = context.Bookings.Where(b => b.PatientID == patientID).Count();
                    var doctorPrice = context.Doctors.FirstOrDefault(d => d.Id == doctorID).Price;
                    if (IsDiscountEligible(DiscountID, patientID, countOfRequests, doctorID))
                    {
                        var discount = context.Discounts.FirstOrDefault(d => d.DiscountID == DiscountID);
                        finalPrice = CalculateFinalPrice((int)doctorPrice, discount.ValueOfDiscount, discount.DiscountType);
                    }
                    else
                        finalPrice = (int)doctorPrice;

                    var booking = new Booking()
                    {
                        DoctorID = doctorID,
                        TimeSlotID = targetedTimeSlot.Select(t => t.time.SlotId).FirstOrDefault(),
                        PatientID = patientID,
                        BookingStatus = Status.pending,
                        DiscountId = DiscountID,
                        FinalPrice = finalPrice
                    };
                    context.Bookings.Add(booking);
                    context.SaveChanges();

                    return HttpStatusCode.OK;
                }
                else
                {
                    return HttpStatusCode.Conflict;
                }
            }
            else
            {
                return HttpStatusCode.BadRequest;
            }
        }

        public HttpStatusCode CancelBooking(string patientID, int BookingID)
        {

            if (BookingID != 0)
            {
                Booking booking = context.Bookings.Where<Booking>(b => b.BookingID == BookingID).FirstOrDefault();

                if (booking != null && booking.PatientID == patientID)
                {
                    booking.BookingStatus = Status.canceled;
                    context.SaveChanges();
                    return HttpStatusCode.OK;
                }
                else
                    return HttpStatusCode.Unauthorized;
            }
            else
                return HttpStatusCode.BadRequest;
        }

        public dynamic GetAllBookings(string userId)
        {
            var userBookings = from booking in context.Bookings
                               join doctor in context.Doctors on
                               booking.DoctorID equals doctor.Id
                               join users in context.Users
                               on doctor.Id equals users.Id
                               join specialization in context.Specializations
                               on doctor.SpecializationID equals specialization.SpecializationID
                               join timeslot in context.TimeSlots
                               on booking.TimeSlotID equals timeslot.SlotId
                               join appointment in context.Appointments
                               on timeslot.AppointmentID equals appointment.Id
                               where booking.PatientID == userId
                               select new
                               {
                                   DoctorImage = doctor.Image,
                                   DoctorName = doctor.Fname + " " + doctor.Lname,
                                   Specialization = specialization.SpecializationName,
                                   Day = appointment.Day,
                                   Time = timeslot.Time,
                                   Price = doctor.Price,
                                   DiscountCode = booking.DiscountId,
                                   FinalPrice = booking.FinalPrice,
                                   Status = booking.BookingStatus
                               };

            return userBookings;
        }

        public dynamic GetAllDoctors(int page, int pageSize, string search)
        {
            var doctors = context.Doctors
     .Select(doctor => new
     {
         doctor.Id,
         doctor.Price,

         specialization = context.Specializations
                     .Where(s => s.SpecializationID == doctor.SpecializationID)
                     .Select(s => s.SpecializationName)
                     .FirstOrDefault(),
         doctorAppointments = context.Appointments
                     .Where(a => a.DoctorID == doctor.Id)
                     .Join(
                         context.TimeSlots,
                         appointment => appointment.Id,
                         timeSlot => timeSlot.AppointmentID,
                         (appointment, timeSlot) => new
                         {
                             day = appointment.Day.ToString(),
                             timeSlot.Time
                         })
                     .ToList(),
         doctor.Fname,
         doctor.Lname,
         doctor.Email,
         doctor.Image,
         doctor.PhoneNumber,
         gender = doctor.Gender.ToString()
     })
             .ToList();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchedDocs = from d in doctors
                                   where d.Fname.Contains(search) ||
                                   d.Email.Contains(search) ||
                                   d.specialization.Contains(search) ||
                                   d.gender.Contains(search) ||
                                   d.PhoneNumber.Contains(search) ||
                                   d.doctorAppointments.Select(a => a.day).Contains(search)
                                   select d;

                var paginationDoc = searchedDocs.Skip((page - 1) * pageSize).Take(pageSize);
                if (searchedDocs != null)
                {
                    return paginationDoc;
                }
                else
                    return doctors.Skip((page - 1) * pageSize).Take(pageSize);
            }
            else
                return doctors.Skip((page - 1) * pageSize).Take(pageSize);
        }


        public async Task<string> Register(PatientDTO patient)
        {
            if (patient != null)
            {
                User user = new User()
                {
                    UserName = patient.Email,
                    Fname = patient.Fname,
                    Lname = patient.Lname,
                    Email = patient.Email,
                    DateOfBirth = patient.DateOfBirth,
                    Image = patient.Image,
                    PhoneNumber = patient.PhoneNumber,
                    Gender = patient.Gender,
                    Password = patient.Password,
                    Type = UserType.patient
                };
                var result = await _userManager.CreateAsync(user, user.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Patient");
                    context.Users.Add(user);


                    context.SaveChanges();
                    return user.Id;
                }
                else
                    return "";
            }
            else
                return null;
        }

        private int CalculateFinalPrice(int doctorPrice, int discountValue, DiscountType type)
        {
            if (type == DiscountType.percentage)
            {
                return doctorPrice - ((int)(doctorPrice * discountValue));
            }
            else
            {
                return doctorPrice - discountValue;
            }
        }

        private bool IsDiscountEligible(int discountID, string patientID, int countOfRequests, string doctorID)
        {
            if (discountID != 0)
            {
                var discount = context.Discounts.FirstOrDefault(d => d.DiscountID == discountID);

                if (discount != null && discount.NumOfRequests == countOfRequests)
                {
                    var doctor = context.Doctors.FirstOrDefault(d => d.Id == doctorID);
                    return true;
                }
            }
            return false;
        }

        private bool IsTimeSlotBooked(string doctorID, int timeSlotID)
        {
            return context.Bookings.Any(b => b.DoctorID == doctorID && b.TimeSlotID == timeSlotID && b.BookingStatus == Status.pending);
        }

    }
}
