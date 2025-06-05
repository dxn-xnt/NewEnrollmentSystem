using System.Web.Mvc;
using System.Web.Routing;

namespace Enrollment_System
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            
            //Enrollments
            routes.MapRoute(
                name: "ProgramHeadEnrollmentListRoute",
                url: "Head/Enrollment",
                defaults: new { controller = "Enrollment", action = "Index" }
            );
            routes.MapRoute(
                name: "ProgramHeadApproveEnrollmentRoute",
                url: "Head/Enrollment/ApproveEnrollment",
                defaults: new { controller = "Enrollment", action = "ApproveEnrollment" }
            );
            routes.MapRoute(
                name: "ProgramHeadDeclineEnrollmentRoute",
                url: "Admin/Enrollment/DeclineEnrollment",
                defaults: new { controller = "Enrollment", action = "DeclineEnrollment" }
            );
            routes.MapRoute(
                name: "ProgramHeadEnrollmentViewRoute",
                url: "Admin/Enrollment/View",
                defaults: new { controller = "Enrollment", action = "GetEnrolledCoursesWithSchedules" }
            );
            routes.MapRoute(
                name: "ProgramHeadEnrollmentStudentViewRoute",
                url: "Admin/Enrollment/ViewStudent",
                defaults: new { controller = "Enrollment", action = "GetEnrollmentDetailsFromDatabase" }
            );
            routes.MapRoute(
                name: "ProgramHeadSetEnrollmentRoute",
                url: "Admin/Enrollment/SetEnrollmentPeriod",
                defaults: new { controller = "Enrollment", action = "SetEnrollmentPeriod" }
            );
            routes.MapRoute(
                name: "ProgramHeadEndEnrollmentRoute",
                url: "Admin/Enrollment/EndEnrollmentPeriod",
                defaults: new { controller = "Enrollment", action = "EndEnrollmentPeriod" }
            );
            routes.MapRoute(
                name: "ProgramHeadGetEnrollmentStatusRoute",
                url: "Admin/Enrollment/GetCurrentEnrollmentPeriod",
                defaults: new { controller = "Enrollment", action = "GetCurrentEnrollmentPeriod" }
            );
            
            //Admin Routes
            routes.MapRoute(
                name: "ProgramHeadClassManagementRoute",
                url: "Admin/ManageClass",
                defaults: new { controller = "CourseManagement", action = "ClassManagement" }
            );
            routes.MapRoute(
                name: "ProgramHeadStudentManagementRoute",
                url: "Admin/ManageStudent",
                defaults: new { controller = "CourseManagement", action = "StudentManagement" }
            );
            routes.MapRoute(
                name: "ProgramHeadSetScheduleListRoute",
                url: "Admin/Schedules",
                defaults: new { controller = "CourseManagement", action = "Schedules" }
            );
            routes.MapRoute(
                name: "ProgramHeadSaveScheduleRoute",
                url: "Admin/Schedules/SaveSchedule",
                defaults: new { controller = "CourseManagement", action = "SaveSchedule" }
            );
            routes.MapRoute(
                name: "ProgramHeadGetScheduleRoute",
                url: "Admin/Schedules/GetSchedule",
                defaults: new { controller = "CourseManagement", action = "GetSchedule" }
            );
            routes.MapRoute(
                name: "ProgramHeadGetCurriculumCoursesRoute",
                url: "Head/Curriculum/GetCurriculumCourses",
                defaults: new { controller = "CourseManagement", action = "GetCurriculumCourses" }
            );
            routes.MapRoute(
                name: "ProgramHeadViewStudentListRoute",
                url: "Admin/Students",
                defaults: new { controller = "CourseManagement", action = "Students" }
            );
            routes.MapRoute(
                name: "AdminEditCourseRoute",
                url: "Admin/Course/EditCourse",
                defaults: new { controller = "Course", action = "Delete" }
            );
            routes.MapRoute(
                name: "MainAdminRoute",
                url: "Admin/Dashboard",
                defaults: new { controller = "Admin", action = "Dashboard" }
            );
            
            //Course Routes
            routes.MapRoute(
                name: "AdminAddCourseRoute",
                url: "Admin/Course/AddCourse",
                defaults: new { controller = "AddCourse", action = "Index" }
            );
            routes.MapRoute(
                name: "AdminCourseRoute",
                url: "Admin/Course",
                defaults: new { controller = "Course", action = "Index" }
            ); 
            routes.MapRoute(
                name: "AdminDeleteCourseRoute",
                url: "Admin/Course/Delete",
                defaults: new { controller = "Course", action = "DeleteCourse" }
            ); 
            
            //Curriculum Routes
            routes.MapRoute(
                name: "AdminCurriculumRoute",
                url: "Admin/Curriculum",
                defaults: new { controller = "Curriculum", action = "Index" }
            ); 
            routes.MapRoute(
                name: "AdminAssignCourseRoute",
                url: "Admin/Curriculum/AssignCourses",
                defaults: new { controller = "Curriculum", action = "AssignCourses" }
            ); 
            routes.MapRoute(
                name: "AdminUnassignCourseRoute",
                url: "Admin/Curriculum/UnassignCourse",
                defaults: new { controller = "Curriculum", action = "UnassignCourse" }
            ); 
            routes.MapRoute(
                name: "AdminGetCourseRoute",
                url: "Admin/Curriculum/GetCurriculumCourses",
                defaults: new { controller = "Curriculum", action = "GetCurriculumCourses" }
            ); 
            
            //Student Routes
            routes.MapRoute(
                name: "StudentGetCurriculumRoute",
                url: "Student/Enrollment/GetCurriculum",
                defaults: new { controller = "Student", action = "GetCurriculum" }
            );  
            routes.MapRoute(
                name: "StudentAvailableCourseRoute",
                url: "Student/Enrollment/GetAvailableCourse",
                defaults: new { controller = "Student", action = "GetAvailableCourse" }
            ); 
            routes.MapRoute(
                name: "StudentScheduleRoute",
                url: "Student/Schedule",
                defaults: new { controller = "Student", action = "Schedule" }
            );
            routes.MapRoute(
                name: "StudentViewGradeRoute",
                url: "Student/Grades",
                defaults: new { controller = "Student", action = "Grade" }
            );
            routes.MapRoute(
                name: "StudentEnrollmentRoute",
                url: "Student/Enrollment",
                defaults: new { controller = "Student", action = "Enrollment" }
            );
            routes.MapRoute(
                name: "StudentSubmitEnrollmentRoute",
                url: "Student/Enrollment/SubmitForm",
                defaults: new { controller = "Student", action = "Enroll" }
            );
            routes.MapRoute(
                name: "StudentProfileRoute",
                url: "Student/Profile",
                defaults: new { controller = "Student", action = "ProfileView" }
            );
            routes.MapRoute(
                name: "StudentRoute",
                url: "Student/Dashboard",
                defaults: new { controller = "Student", action = "Dashboard" }
            );
            
            //Signin Routes
            routes.MapRoute(
                name: "SignUpRoute",
                url: "Account/SignUp",
                defaults: new { controller = "Account", action = "SignUp" }
            );
            
            //Login Routes
            routes.MapRoute(
                name: "FacultyLoginRoute",
                url: "Account/Faculty/Login",
                defaults: new { controller = "Account", action = "FacultyLogIn" }
            );
            routes.MapRoute(
                name: "LoginRoute",
                url: "Account/Student/LogIn",
                defaults: new { controller = "Account", action = "StudentLogIn" }
            );
            
            //Default
            routes.MapRoute(
                name: "Default",
                url: "",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}