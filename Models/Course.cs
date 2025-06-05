using System;

namespace Enrollment_System.Models
{
    public class Course
    {
        public string Code { get; set; }
        public string Title { get; set; }
        public int Units { get; set; }
        public int LecHours { get; set; }
        public int LabHours { get; set; }
        public string CategoryName { get; set; }
        public string CategoryCode { get; set; }
        public string[] Prerequisites { get; set; }
    }
}