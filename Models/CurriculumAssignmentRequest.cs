using System.Collections.Generic;

namespace Enrollment_System.Models
{
    public class CurriculumAssignmentRequest
    {
        public Curriculum Curriculum { get; set; }
        public List<CurriculumCourse> CurriculumCourses { get; set; }
    }
}