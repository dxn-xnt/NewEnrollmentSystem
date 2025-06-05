using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Enrollment_System.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string HomeAddress { get; set; }
        public string CityAddress { get; set; }
        public DateTime Birthdate { get; set; }
        public string District { get; set; }
        public string Contact { get; set; }
        public string Email { get; set; }
        public int YearLevel { get; set; }
        public int Semester { get; set; }
        public string Program { get; set; }
        public string Major { get; set; }
        public Boolean IsFirstGen { get; set; }
        public string Status { get; set; }
        public string Password { get; set; }
        public string ProgramCode { get; set; }
        public string BlockSection { get; set; }
        
    }
}