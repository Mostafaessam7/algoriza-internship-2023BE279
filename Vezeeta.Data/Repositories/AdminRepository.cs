using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Vezeeta.Core.DTOs;
using Vezeeta.Core.Models;
using Vezeeta.Core.Repositories;
using Vezeeta.Data;

namespace Vezeeta.Infrastructure.RepositoriesImplementation
{
    public class AdminRepository : IAdminRepository
    {
        private readonly VezeetaContext _context;
        private readonly UserManager<IdentityUser> _userManager;


        public AdminRepository(VezeetaContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public int NumOfDoctors()
        {
            return _context.Doctors.Count<Doctor>();
        }

        public int NumOfPatients()
        {
            return _context.AllUsers.Count<User>(d => d.Type == UserType.patient);
        }

        public dynamic NumOfRequests()
        {
            int totalRequests = _context.Bookings.Count();
            int pendingRequests = _context.Bookings.Count(d => d.BookingStatus == Core.Models.Status.pending);
            int canceledRequests = _context.Bookings.Count(d => d.BookingStatus == Core.Models.Status.canceled);
            int completedRequests = _context.Bookings.Count(d => d.BookingStatus == Core.Models.Status.completed);
            return new { totalRequests, pendingRequests, canceledRequests, completedRequests };
        }

        public dynamic Top10Doctors()
        {
            var result = _context.Bookings
                .Join(
                _context.Doctors,
                booking => booking.DoctorID,
                doctor => doctor.Id,
                (booking, doctor) => new { Booking = booking, Doctor = doctor })

                .Join(
                _context.Users,
                joined => joined.Doctor.Id,
                user => user.Id,
                (joined, user) => new { Joined = joined, User = user })

                .GroupBy(
                joinedUser => joinedUser.User.Fname + " " + joinedUser.User.Lname,
                joinedUser => joinedUser.Joined.Booking.BookingID,
                (fullname, count) => new
                {
                    FullName = fullname,
                    Requests = count.Count()
                })

                .OrderByDescending(result => result.Requests).Take(10);

            return result;
        }

        public dynamic Top5Specializations()
        {

            var result = _context.Bookings
            .Join(
                _context.Doctors,
                booking => booking.DoctorID,
                doctor => doctor.Id,
                (booking, doctor) => new { Booking = booking, Doctor = doctor })
            .Join(
                _context.Specializations,
                joined => joined.Doctor.SpecializationID,
                specialization => specialization.SpecializationID,
                (joined, specialization) => new { Joined = joined, Specialization = specialization })
            .GroupBy(
                joinedSpecialization => joinedSpecialization.Specialization.SpecializationName,
                joinedSpecialization => joinedSpecialization.Joined.Booking.BookingID,
                (specializationName, count) => new
                {
                    SpecializationName = specializationName,
                    Count = count.Count()
                })
            .OrderByDescending(result => result.Count)

            .Take(5);

            return result;
        }
        public dynamic GetAllDoctors(int page, int pageSize, string search)
        {
            var doctors = _context.Doctors
           .Select(doctor => new
            {
                doctor.Doctorid,
                doctor.Price,

                specialization = _context.Specializations
                             .Where(s => s.SpecializationID == doctor.SpecializationID)
                             .Select(s => s.SpecializationName)
                             .FirstOrDefault(),
                doctorAppointments = _context.Appointments
                             .Where(a => a.DoctorID == doctor.Doctorid)
                             .Join(_context.TimeSlots,appointment => appointment.Id,timeSlot => timeSlot.AppointmentID,
                             (appointment, timeSlot) => new
                             {
                                day = appointment.Day.ToString(),
                                timeSlot.Time
                             }).ToList(),
                doctor.Fname
               ,doctor.Lname,
               doctor.Email,
               doctor.Image,
               doctor.PhoneNumber,
               gender = doctor.Gender
               .ToString()})
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

        public dynamic GetDoctorById(string doctorId)
        {
            var doctor = _context.Doctors.Where(d => d.Doctorid == doctorId).FirstOrDefault();

            if (doctor != null)
            {
                var specializationOfDoctor = _context.Specializations.Where(s => s.SpecializationID == doctor.SpecializationID).FirstOrDefault();
                return new
                {
                    doctor.Image,
                    fullname = (doctor.Fname + " " + doctor.Lname),
                    doctor.Email,
                    doctor.PhoneNumber,
                    specializationOfDoctor.SpecializationName,
                    doctor.Gender
                };
            }
            else
                return HttpStatusCode.NotFound;
        }

        private string generatePassword()
        {
            Random random = new Random();
            const string letters = "abcdefghijklmnopqrstuvwxyz";
            const string UpperLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string specialChars = "!@";
            const string numbers = "0123456789";
            StringBuilder password = new StringBuilder();

            for (int i = 0; i < 6; i++)
            {
                int letterIndex = random.Next(letters.Length);
                password.Append(letters[letterIndex]);
            }

            for (int i = 0; i < 6; i++)
            {
                int letterIndex = random.Next(UpperLetters.Length);
                password.Append(UpperLetters[letterIndex]);
            }

            int specialCharIndex = random.Next(specialChars.Length);
            password.Append(specialChars[specialCharIndex]);

            int numberIndex = random.Next(numbers.Length);
            password.Append(numbers[numberIndex]);

            for (int i = 0; i < 4; i++)
            {
                int index = random.Next(letters.Length + specialChars.Length + numbers.Length);
                if (index < letters.Length)
                {
                    password.Append(letters[index]);
                }
                else if (index < letters.Length + specialChars.Length)
                {
                    password.Append(specialChars[index - letters.Length]);
                }
                else
                {
                    password.Append(numbers[index - (letters.Length + specialChars.Length)]);
                }
            }

            return password.ToString();
        }

        public async Task<string> AddDoctor(AddDoctorDTO doctor)
        {
            if (doctor != null)
            {
                User doc = new User()
                {
                    UserName = doctor.Email,
                    Fname = doctor.Fname,
                    Lname = doctor.Lname,
                    Email = doctor.Email,
                    DateOfBirth = (DateTime)doctor.DateOfBirth,
                    Image = doctor.Image,
                    PhoneNumber = doctor.Phone,
                    Gender = (Gender)doctor.Gender,
                    Password = generatePassword(),

                    Type = UserType.doctor
                };
                var result = await _userManager.CreateAsync(doc, doc.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(doc, "Doctor");
                    _context.Users.Add(doc);

                    _context.Database.ExecuteSqlRaw($"insert into Doctors values (\'{doc.Id}\',1,{doctor.Price},{doctor.SpecializationID});");

                    return doc.Id;
                }
                else
                    return "";
            }
            else
                return null;
        }
        public HttpStatusCode EditDoctor(string doctorID, AddDoctorDTO doctor)
        {
            var findDoctor = _context.Doctors.FirstOrDefault(d => d.Id == doctorID);
            if (findDoctor != null)
            {

                findDoctor.Fname = doctor.Fname;
                findDoctor.Lname = doctor.Lname;
                findDoctor.Email = doctor.Email;
                findDoctor.DateOfBirth = (DateTime)doctor.DateOfBirth;
                findDoctor.Image = doctor.Image;
                findDoctor.PhoneNumber = doctor.Phone;
                findDoctor.Gender = (Gender)doctor.Gender;
                findDoctor.Type = UserType.doctor;

                _context.SaveChanges();

                _context.Database.ExecuteSqlRaw($"update Doctors set price ={doctor.Price} and specializationID = {doctor.SpecializationID} where doctorid= {doctorID} );");

                _context.SaveChanges();
                return HttpStatusCode.OK;
            }
            else
                return HttpStatusCode.NotFound;

        }

        public async Task<HttpStatusCode> DeleteDoctor(string doctorID)
        {
            //check if doctor exists 

            var findDoctor = _context.Doctors.FirstOrDefault(d => d.Id == doctorID);
            var UserDoctor = _context.Users.FirstOrDefault(u => u.Id == doctorID);
            var countRequestsForDoctor = _context.Bookings.Where(u => u.DoctorID == doctorID).Count();
            if (findDoctor != null && UserDoctor != null && countRequestsForDoctor == 0)
            {

                _context.Doctors.Remove(findDoctor);
                _context.Users.Remove(UserDoctor);
                await _userManager.DeleteAsync(UserDoctor);
                _context.SaveChanges();
                return HttpStatusCode.OK;
            }
            else return HttpStatusCode.Unauthorized;

        }

        public dynamic GetallPatients(int page, int pageSize, string search)
        {
            var patient = _context.Users.Where(p => p.Type == UserType.patient).Select(
                s => new { s.Fname, s.Lname, s.Email, s.DateOfBirth, s.Image, s.Gender });
            return patient;
        }

        public dynamic getPatientByID(string patientId)
        {
            var patient = _context.Users.Where(p => p.Id == patientId && p.Type == UserType.patient).Select(
                           s => new { s.Fname, s.Lname, s.Email, s.DateOfBirth, s.Image, s.Gender });
            return patient;
        }

        public HttpStatusCode AddDiscount(DiscountDTO discountInfo)
        {
            Discount discount = new Discount()
            {
                discountName = discountInfo.DiscountCode,
                DiscountType = discountInfo.DiscountType,
                ValueOfDiscount = discountInfo.Value,
                NumOfRequests = discountInfo.NoOfReq,
                DiscountActivity = DiscountActivity.active
            };
            _context.Discounts.Add(discount);
            _context.SaveChanges();
            return HttpStatusCode.OK;
        }

        public HttpStatusCode EditDiscount(int discountID, DiscountDTO discountInfo)
        {
            var findDiscount = _context.Discounts.FirstOrDefault(d => d.DiscountID == discountID);

            if (findDiscount != null)
            {
                if (!string.IsNullOrEmpty(discountInfo.DiscountCode))
                {
                    findDiscount.discountName = discountInfo.DiscountCode;
                }

                if (discountInfo.DiscountType != null && discountInfo.DiscountType != 0)
                {
                    findDiscount.DiscountType = discountInfo.DiscountType;
                }

                if (discountInfo.Value != null && discountInfo.Value != 0)
                {
                    findDiscount.ValueOfDiscount = discountInfo.Value;
                }

                if (discountInfo.NoOfReq != null && discountInfo.NoOfReq != 0)
                {
                    findDiscount.NumOfRequests = discountInfo.NoOfReq;
                }
                _context.SaveChanges();
                return HttpStatusCode.OK;
            }
            return HttpStatusCode.NotFound;
        }


        public HttpStatusCode DeleteDiscount(int discountID)
        {
            var discount = _context.Discounts.FirstOrDefault(d => d.DiscountID == discountID);
            if (discount != null)
            {
                _context.Discounts.Remove(discount);
                _context.SaveChanges();
                return HttpStatusCode.OK;
            }
            else
                return HttpStatusCode.NotFound;
        }

        public HttpStatusCode DeactivateDiscount(int discountID)
        {
            var discount = _context.Discounts.FirstOrDefault(d => d.DiscountID == discountID);
            if (discount != null)
            {
                discount.DiscountActivity = DiscountActivity.deactive;
                _context.SaveChanges();
                return HttpStatusCode.OK;
            }
            else
                return HttpStatusCode.NotFound;
        }
    }
}
