using System;

namespace Enrollment_System.Models
{
    public class Enrollment
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public string Date { get; set; }
        public int YearLevel { get; set; }
        public int Semester { get; set; }
        public int StudentId { get; set; }
        public string AcademicYear { get; set; }
        public string StudentName { get; set; }
        public string Program { get; set; }
        public string ProgramName { get; set; }
    }
}