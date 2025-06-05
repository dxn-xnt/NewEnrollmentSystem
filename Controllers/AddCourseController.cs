using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Enrollment_System.Models;
using Npgsql;
using Enrollment_System.Controllers.Service;

namespace Enrollment_System.Controllers
{
    public class AddCourseController : Controller
    {
        private readonly string _connectionString;
        private readonly BaseControllerServices _baseController;

        public AddCourseController(BaseControllerServices baseController)
        {
            _baseController = baseController;
            _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DbConnection"].ConnectionString;
        }

        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.CourseCategories = _baseController.GetCourseCategoriesFromDatabase();
            ViewBag.CoursesForPrereq = _baseController.GetCoursesFromDatabase();
            return View("~/Views/Admin/AddCourse.cshtml");
        }

        [HttpPost]
        [Route("/Admin/Course/AddCourse")]
        public ActionResult Index(Course course) 
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrEmpty(course.Code) || 
                    string.IsNullOrEmpty(course.Title) ||
                    course.Units <= 0)
                {
                    return Json(new { 
                        mess = 0, 
                        error = "Course code, title, and units are required fields.",
                        field = "Crs_Code"
                    });
                }

                using (var db = new NpgsqlConnection(_connectionString))
                {
                    db.Open();
                    using (var transaction = db.BeginTransaction())
                    {
                        try
                        {
                            // Check if course exists
                            var existsCmd = new NpgsqlCommand(
                                "SELECT COUNT(*) FROM COURSE WHERE CRS_CODE = @Code", db, transaction);
                            existsCmd.Parameters.AddWithValue("@Code", course.Code);
                            if (Convert.ToInt32(existsCmd.ExecuteScalar()) > 0)
                            {
                                return Json(new { 
                                    mess = 2, 
                                    error = "Course Code Already Exists",
                                    field = "Course Code"
                                });
                            }
                            var exists2Cmd = new NpgsqlCommand(
                                "SELECT COUNT(*) FROM COURSE WHERE CRS_TITLE = @Code", db, transaction);
                            exists2Cmd.Parameters.AddWithValue("@Code", course.Title);
                            if (Convert.ToInt32(exists2Cmd.ExecuteScalar()) > 0)
                            {
                                return Json(new { 
                                    mess = 2, 
                                    error = "Course Title Already Exists",
                                    field = "Course Title"
                                });
                            }

                            // Insert course - USING DATABASE COLUMN NAMES
                            var insertCmd = new NpgsqlCommand(@"
                                INSERT INTO COURSE 
                                (CRS_CODE, CRS_TITLE, CRS_UNITS, CRS_LEC, CRS_LAB, CTG_CODE)
                                VALUES (@Code, @Title, @Units, @LecHours, @LabHours, @CategoryCode)
                                RETURNING CRS_CODE", db, transaction);
                            
                            insertCmd.Parameters.AddWithValue("@Code", course.Code);
                            insertCmd.Parameters.AddWithValue("@Title", course.Title);
                            insertCmd.Parameters.AddWithValue("@Units", course.Units);
                            insertCmd.Parameters.AddWithValue("@LecHours", course.LecHours);
                            insertCmd.Parameters.AddWithValue("@LabHours", course.LabHours);
                            insertCmd.Parameters.AddWithValue("@CategoryCode", course.CategoryCode);

                            var courseCode = (string)insertCmd.ExecuteScalar();

                            if (course.Prerequisites != null)
                            {
                                foreach (var prereq in course.Prerequisites)
                                {
                                    var preqCode = prereq?.ToString()?.Trim();
        
                                    if (!string.IsNullOrWhiteSpace(preqCode))
                                    {
                                        using (var prereqCmd = new NpgsqlCommand(
                                                   "INSERT INTO PREREQUISITE (CRS_CODE, PREQ_CRS_CODE) " +
                                                   "VALUES (@courseCode, @prereqCode)", db, transaction))
                                        {
                                            prereqCmd.Parameters.AddWithValue("@courseCode", courseCode);
                                            prereqCmd.Parameters.AddWithValue("@prereqCode", preqCode);
                                            prereqCmd.ExecuteNonQuery();
                                        }
                                    }
                                }
                            }

                            transaction.Commit();
                            return Json(new {
                                mess = 1,
                                message = "Course created successfully",
                                redirectUrl = Url.Action("Index", "Course")
                            });
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            return Json(new { 
                                mess = 0, 
                                error = ex.Message 
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { 
                    mess = 0, 
                    error = ex.Message 
                });
            }
        }
    }
}