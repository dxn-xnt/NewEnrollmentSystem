using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Mvc;
using Enrollment_System.Models;
using Npgsql;
using Enrollment_System.Controllers.Service;

namespace Enrollment_System.Controllers
{
    public class CourseController : Controller
    {
        private readonly string _connectionString;

        public CourseController(BaseControllerServices baseController)
        {
            _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DbConnection"].ConnectionString;
        }

        public ActionResult Index()
        {
            var courses = GetCoursesFromDatabase();
            ViewBag.Prerequisites = GetPrerequisitesFromDatabase();
            return View("~/Views/Admin/Courses.cshtml",courses);
        }
      
        // GET: /Course/Edit/{id}
        public ActionResult Edit(string id)
        {
            var course = GetCourseById(id);
            if (course == null) return HttpNotFound();

            return View("~/Views/Admin/EditCourse.cshtml", course);
        }
        
        [HttpPost]
        [Route("/Admin/Course/Delete")]
        public ActionResult DeleteCourse(string code)
        {
            try
            {
                if (string.IsNullOrEmpty(code))
                {
                    return Json(new { 
                        success = false, 
                        error = "Course code is required",
                        field = "general"
                    });
                }

                using (var db = new NpgsqlConnection(_connectionString))
                {
                    db.Open();
                    using (var transaction = db.BeginTransaction())
                    {
                        try
                        {
                            // 1. Check if course exists
                            var courseCheckCmd = new NpgsqlCommand(
                                "SELECT COUNT(*) FROM COURSE WHERE CRS_CODE = @code", 
                                db, transaction);
                            courseCheckCmd.Parameters.AddWithValue("@code", code);
                            
                            if (Convert.ToInt32(courseCheckCmd.ExecuteScalar()) == 0)
                            {
                                return Json(new { 
                                    success = false, 
                                    error = "Course not found",
                                    field = "Course Code"
                                });
                            }

                            // 2. Check if course is used as a prerequisite
                            var prereqCheckCmd = new NpgsqlCommand(
                                "SELECT COUNT(*) FROM PREREQUISITE WHERE PREQ_CRS_CODE = @code", 
                                db, transaction);
                            prereqCheckCmd.Parameters.AddWithValue("@code", code);
                            
                            if (Convert.ToInt32(prereqCheckCmd.ExecuteScalar()) > 0)
                            {
                                return Json(new { 
                                    success = false, 
                                    error = "Cannot delete course because it's used as a prerequisite for other courses",
                                    field = "Course Code"
                                });
                            }

                            // 3. Check if course is assigned to any curriculum
                            var curriculumCheckCmd = new NpgsqlCommand(
                                "SELECT COUNT(*) FROM CURRICULUM_COURSE WHERE CRS_CODE = @code", 
                                db, transaction);
                            curriculumCheckCmd.Parameters.AddWithValue("@code", code);
                            
                            if (Convert.ToInt32(curriculumCheckCmd.ExecuteScalar()) > 0)
                            {
                                return Json(new { 
                                    success = false, 
                                    error = "Cannot delete course because it's assigned to one or more curricula",
                                    field = "Course Code"
                                });
                            }

                            // 4. Delete the course
                            var deleteCmd = new NpgsqlCommand(
                                "DELETE FROM COURSE WHERE CRS_CODE = @code", 
                                db, transaction);
                            deleteCmd.Parameters.AddWithValue("@code", code);
                            
                            int rowsAffected = deleteCmd.ExecuteNonQuery();

                            if (rowsAffected == 0)
                            {
                                transaction.Rollback();
                                return Json(new { 
                                    success = false, 
                                    error = "Failed to delete course",
                                    field = "general"
                                });
                            }

                            transaction.Commit();
                            return Json(new {
                                success = true,
                                message = "Course deleted successfully"
                            });
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            return Json(new { 
                                success = false, 
                                error = ex.Message,
                                field = "general"
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    error = ex.Message,
                    field = "general"
                });
            }
        }

        // GET: /Course/GetAll
        [HttpGet]
        public JsonResult GetAllCourses()
        {
            var courses = new List<dynamic>();
            try
            {
                using (var db = new NpgsqlConnection(_connectionString))
                {
                    db.Open();
                    using (var cmd = new NpgsqlCommand("SELECT CRS_CODE, CRS_TITLE FROM COURSE", db))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                courses.Add(new
                                {
                                    code = reader["CRS_CODE"].ToString(),
                                    title = reader["CRS_TITLE"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Error fetching courses: {ex.Message}");
            }

            return Json(courses, JsonRequestBehavior.AllowGet);
        }
        
        private List<Prerequisite> GetPrerequisitesFromDatabase()
        {
            var prerequisites = new List<Prerequisite>();

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT \"crs_code\", \"preq_crs_code\" FROM \"prerequisite\"", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            prerequisites.Add(new Prerequisite
                            {
                                CourseCode = reader.GetString(0),
                                PrerequisiteCourseCode = reader.GetString(1)
                            });
                        }
                    }
                }
            }

            return prerequisites;
        }
        
        private List<Course> GetCoursesFromDatabase()
        {
            var courses = new List<Course>();

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(@"
                    SELECT DISTINCT
                        c.CRS_CODE, 
                        c.CRS_TITLE, 
                        COALESCE(cat.CTG_NAME, 'General') AS Category,
                        c.CRS_UNITS, 
                        c.CRS_LEC, 
                        c.CRS_LAB
                    FROM COURSE c
                    LEFT JOIN COURSE_CATEGORY cat ON c.CTG_CODE = cat.CTG_CODE
                    LEFT JOIN PREREQUISITE p ON p.CRS_CODE = c.CRS_CODE", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            courses.Add(new Course
                            {
                                Code = reader["CRS_CODE"]?.ToString(),
                                Title = reader["CRS_TITLE"]?.ToString(),
                                CategoryName = reader["Category"]?.ToString(),
                                Units = reader["CRS_UNITS"] != DBNull.Value ? Convert.ToInt32(reader["CRS_UNITS"]) : 0,
                                LecHours = reader["CRS_LEC"] != DBNull.Value ? Convert.ToInt32(reader["CRS_LEC"]) : 0,
                                LabHours = reader["CRS_LAB"] != DBNull.Value ? Convert.ToInt32(reader["CRS_LAB"]) : 0
                            });
                        }
                    }
                }
            }

            return courses;
        }

        private object GetCourseById(string id)
        {
            return null;
        }
    }
}