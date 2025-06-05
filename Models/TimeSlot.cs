using System;

namespace Enrollment_System.Models
{
    public class TimeSlot
    {
        public int ScheduleId { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Day { get; set; }
    }
}