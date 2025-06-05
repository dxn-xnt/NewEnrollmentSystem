using System;

namespace Enrollment_System.Models
{
    public class CurrentEnrollment
    {
        public int Id { get; set; }
        public string AcademicYear { get; set; }
        public string Semester { get; set; }
        public string Status { get; set; }
    }
}