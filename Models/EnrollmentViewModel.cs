using System.Collections.Generic;
using Enrollment_System.Models;

namespace Enrollment_System.Models
{
    public class EnrollmentViewModel
    {
        public Student Student { get; set; }
        public Enrollment Enrollment { get; set; }
        public List<EnrollingCourse> EnrollingCourses{ get; set; }
    }
}

