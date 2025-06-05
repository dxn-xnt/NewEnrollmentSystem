using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Enrollment_System.Models
{
    public class Schedule
    {
        public int Id { get; set; }
        public string CourseCode { get; set; }
        public int RoomId { get; set; }
        public string BlockSectionCode { get; set; }
        public int ProfessorId { get; set; }
    }
}