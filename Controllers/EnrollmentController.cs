using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using Enrollment_System.Models;
using Npgsql;
using Enrollment_System.Controllers.Service;

namespace Enrollment_System.Controllers
{
    public class EnrollmentController : Controller
    {
        private readonly string _connectionString;
        private readonly BaseControllerServices _baseController;

        public EnrollmentController(BaseControllerServices baseController)
        {
            _baseController = baseController;
            _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DbConnection"].ConnectionString;
        }

        public ActionResult Index()
        {
            var enrollments = _baseController.GetEnrollmentsFromDatabase();
            ViewBag.AcademicYears = _baseController.GetAcademicYearsFromDatabase();
            ViewBag.Semesters = _baseController.GetSemesterFromDatabase();
            return View("~/Views/Admin/EnrollmentApproval.cshtml", enrollments);
        }
        
        [HttpPost]
        [Route("/Admin/Enrollment/SetEnrollmentPeriod")]
        public ActionResult SetEnrollmentPeriod(string academicYear, int semId, bool isActive)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(academicYear))
                {
                    return Json(new { 
                        success = false,
                        message = "Validation failed",
                        error = "Academic year is required",
                        field = "academicYear"
                    });
                }

                if (semId <= 0)
                {
                    return Json(new { 
                        success = false,
                        message = "Validation failed",
                        error = "Invalid semester ID",
                        field = "semId"
                    });
                }

                using (var db = new NpgsqlConnection(_connectionString))
                {
                    db.Open();
                    
                    // Verify semester exists
                    var checkSemesterQuery = "SELECT COUNT(*) FROM SEMESTER WHERE SEM_ID = @SemId";
                    var checkSemesterCmd = new NpgsqlCommand(checkSemesterQuery, db);
                    checkSemesterCmd.Parameters.AddWithValue("@SemId", semId);
                    var semesterExists = Convert.ToInt32(checkSemesterCmd.ExecuteScalar()) > 0;

                    if (!semesterExists)
                    {
                        return Json(new { 
                            success = false,
                            message = "Validation failed",
                            error = "Semester does not exist",
                            field = "semId"
                        });
                    }

                    // Check if this period already exists
                    var checkQuery = @"
                        SELECT COUNT(*) 
                        FROM CURRENT_ENROLLMENT 
                        WHERE AY_CODE = @AcademicYear 
                        AND SEM_ID = @SemId";
                        
                    var checkCmd = new NpgsqlCommand(checkQuery, db);
                    checkCmd.Parameters.AddWithValue("@AcademicYear", academicYear);
                    checkCmd.Parameters.AddWithValue("@SemId", semId);
                    var exists = Convert.ToInt32(checkCmd.ExecuteScalar()) > 0;

                    if (exists)
                    {
                        // Update existing period
                        var updateQuery = @"
                            UPDATE CURRENT_ENROLLMENT 
                            SET CUREN_STATUS = @Status
                            WHERE AY_CODE = @AcademicYear
                            AND SEM_ID = @SemId";
                            
                        var updateCmd = new NpgsqlCommand(updateQuery, db);
                        updateCmd.Parameters.AddWithValue("@Status", isActive ? "ongoing" : "completed");
                        updateCmd.Parameters.AddWithValue("@AcademicYear", academicYear);
                        updateCmd.Parameters.AddWithValue("@SemId", semId);
                        updateCmd.ExecuteNonQuery();
                    }
                    else
                    {
                        // Insert new period
                        var insertQuery = @"
                            INSERT INTO CURRENT_ENROLLMENT (
                                AY_CODE, 
                                SEM_ID, 
                                CUREN_STATUS
                            ) VALUES (
                                @AcademicYear,
                                @SemId,
                                @Status
                            )";
                            
                        var insertCmd = new NpgsqlCommand(insertQuery, db);
                        insertCmd.Parameters.AddWithValue("@AcademicYear", academicYear);
                        insertCmd.Parameters.AddWithValue("@SemId", semId);
                        insertCmd.Parameters.AddWithValue("@Status", isActive ? "ongoing" : "completed");
                        insertCmd.ExecuteNonQuery();
                    }

                    // If this period is being activated, deactivate others
                    if (isActive)
                    {
                        var deactivateQuery = @"
                            UPDATE CURRENT_ENROLLMENT 
                            SET CUREN_STATUS = 'completed'
                            WHERE NOT (AY_CODE = @AcademicYear AND SEM_ID = @SemId)
                            AND CUREN_STATUS = 'ongoing'";
                            
                        var deactivateCmd = new NpgsqlCommand(deactivateQuery, db);
                        deactivateCmd.Parameters.AddWithValue("@AcademicYear", academicYear);
                        deactivateCmd.Parameters.AddWithValue("@SemId", semId);
                        deactivateCmd.ExecuteNonQuery();
                    }

                    return Json(new {
                        success = true,
                        message = "Enrollment period set successfully"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false,
                    message = "Server error",
                    error = ex.Message
                });
            }
        }
        
        [HttpPost]
        [Route("/Admin/Enrollment/EndEnrollmentPeriod")]
        public ActionResult EndEnrollmentPeriod()
        {
            try
            {
                using (var db = new NpgsqlConnection(_connectionString))
                {
                    db.Open();
                    
                    // Get the current active enrollment period
                    var getCurrentQuery = @"
                        SELECT AY_CODE, SEM_ID 
                        FROM CURRENT_ENROLLMENT 
                        WHERE CUREN_STATUS = 'ongoing' 
                        LIMIT 1";
                        
                    using (var getCurrentCmd = new NpgsqlCommand(getCurrentQuery, db))
                    using (var reader = getCurrentCmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                        {
                            return Json(new { 
                                success = false,
                                message = "No active enrollment period found",
                                error = "There is no currently active enrollment period to end"
                            });
                        }

                        reader.Read();
                        var academicYear = reader.GetString(0);
                        var semId = reader.GetInt32(1);
                        reader.Close();

                        // Update the status to completed
                        var updateQuery = @"
                            UPDATE CURRENT_ENROLLMENT 
                            SET CUREN_STATUS = 'completed'
                            WHERE AY_CODE = @AcademicYear
                            AND SEM_ID = @SemId";
                            
                        using (var updateCmd = new NpgsqlCommand(updateQuery, db))
                        {
                            updateCmd.Parameters.AddWithValue("@AcademicYear", academicYear);
                            updateCmd.Parameters.AddWithValue("@SemId", semId);
                            int rowsAffected = updateCmd.ExecuteNonQuery();

                            if (rowsAffected == 0)
                            {
                                return Json(new { 
                                    success = false,
                                    message = "Update failed",
                                    error = "No records were updated"
                                });
                            }

                            return Json(new {
                                success = true,
                                message = "Enrollment period ended successfully",
                                academicYear = academicYear,
                                semId = semId
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false,
                    message = "Server error",
                    error = ex.Message
                });
            }
        }

        [HttpGet]
        [Route("/Admin/Enrollment/GetCurrentEnrollmentPeriod")]
        public ActionResult GetCurrentEnrollmentPeriod()
        {
            try
            {
                using (var db = new NpgsqlConnection(_connectionString))
                {
                    db.Open();
                    
                    // First check for active enrollment period
                    var activeQuery = @"
                        SELECT 
                            ce.AY_CODE AS AcademicYear,
                            ce.SEM_ID AS SemesterId,
                            s.SEM_NAME AS SemesterName,
                            ce.CUREN_STATUS AS Status,
                            (ce.CUREN_STATUS = 'ongoing') AS IsActive
                        FROM CURRENT_ENROLLMENT ce
                        JOIN SEMESTER s ON ce.SEM_ID = s.SEM_ID
                        WHERE ce.CUREN_STATUS = 'ongoing'
                        LIMIT 1";
                        
                    using (var activeCmd = new NpgsqlCommand(activeQuery, db))
                    using (var activeReader = activeCmd.ExecuteReader())
                    {
                        if (activeReader.Read())
                        {
                            return Json(new {
                                success = true,
                                message = "Current enrollment period found",
                                period = new {
                                    AcademicYear = activeReader["AcademicYear"].ToString(),
                                    SemesterId = Convert.ToInt32(activeReader["SemesterId"]),
                                    SemesterName = activeReader["SemesterName"].ToString(),
                                    Status = activeReader["Status"].ToString(),
                                    IsActive = Convert.ToBoolean(activeReader["IsActive"])
                                }
                            }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    // If no active period, check for completed periods
                    var completedQuery = @"
                        SELECT 
                            ce.AY_CODE AS AcademicYear,
                            ce.SEM_ID AS SemesterId,
                            s.SEM_NAME AS SemesterName,
                            ce.CUREN_STATUS AS Status
                        FROM CURRENT_ENROLLMENT ce
                        JOIN SEMESTER s ON ce.SEM_ID = s.SEM_ID
                        WHERE ce.CUREN_STATUS = 'completed'
                        ORDER BY ce.AY_CODE DESC, ce.SEM_ID DESC
                        LIMIT 1";
                        
                    using (var completedCmd = new NpgsqlCommand(completedQuery, db))
                    using (var completedReader = completedCmd.ExecuteReader())
                    {
                        if (completedReader.Read())
                        {
                            return Json(new {
                                success = true,
                                message = "No active enrollment period found",
                                period = new {
                                    AcademicYear = completedReader["AcademicYear"].ToString(),
                                    SemesterId = Convert.ToInt32(completedReader["SemesterId"]),
                                    SemesterName = completedReader["SemesterName"].ToString(),
                                    Status = completedReader["Status"].ToString(),
                                    IsActive = false
                                }
                            }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(new { 
                                success = true,
                                message = "No enrollment period found",
                                period = (object)null
                            }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false,
                    message = "Server error",
                    error = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }
                                
        [HttpPost]
        [Route("/Admin/Enrollment/AcceptStudent")]
        public JsonResult ApproveEnrollment(int enrollmentId)
        {
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(
                               "UPDATE ENROLLMENT SET ENROL_STATUS = 'Approved' WHERE ENROL_ID = @enrollmentId", conn))
                    {
                        cmd.Parameters.AddWithValue("@enrollmentId", enrollmentId);
                        int rowsAffected = cmd.ExecuteNonQuery();
                
                        if (rowsAffected > 0)
                        {
                            return Json(new { success = true, message = "Enrollment approved successfully" });
                        }
                        return Json(new { success = false, message = "Enrollment not found" });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Route("/Admin/Enrollment/RejectStudent")]
        public JsonResult DeclineEnrollment(int enrollmentId)
        {
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(
                               "UPDATE ENROLLMENT SET ENROL_STATUS = 'Rejected' WHERE ENROL_ID = @enrollmentId", conn))
                    {
                        cmd.Parameters.AddWithValue("@enrollmentId", enrollmentId);
                        int rowsAffected = cmd.ExecuteNonQuery();
                
                        if (rowsAffected > 0)
                        {
                            return Json(new { success = true, message = "Enrollment rejected successfully" });
                        }
                        return Json(new { success = false, message = "Enrollment not found" });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        
        
        [HttpGet]
        [Route("/Admin/Enrollment/ViewStudent")]
        public ActionResult GetEnrollmentDetailsFromDatabase(int enrollmentId)
        {
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(@"
                        SELECT 
                            e.ENROL_ID,
                            e.ENROL_STATUS,
                            e.ENROL_DATE,
                            e.ENROL_YR_LEVEL,
                            e.ENROL_SEM,
                            e.STUD_ID,
                            e.AY_CODE,
                            s.STUD_FNAME,
                            s.STUD_LNAME,
                            p.PROG_CODE,
                            p.PROG_TITLE
                        FROM ENROLLMENT e
                        JOIN STUDENT s ON e.STUD_ID = s.STUD_ID
                        JOIN PROGRAM p ON s.PROG_CODE = p.PROG_CODE
                        WHERE e.ENROL_ID = @enrollmentId", conn))
                    {
                        cmd.Parameters.AddWithValue("@enrollmentId", enrollmentId);
                
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var enrollment = new Enrollment
                                {
                                    Id = Convert.ToInt32(reader["ENROL_ID"]),
                                    Status = reader["ENROL_STATUS"]?.ToString(),
                                    Date = Convert.ToDateTime(reader["ENROL_DATE"]).ToString("yyyy-MM-dd"),
                                    StudentId = Convert.ToInt32(reader["STUD_ID"]),
                                    AcademicYear = reader["AY_CODE"]?.ToString(),
                                    Semester = reader["ENROL_SEM"] != DBNull.Value ? Convert.ToInt32(reader["ENROL_SEM"]) : 0,
                                    YearLevel = reader["ENROL_YR_LEVEL"] != DBNull.Value ? Convert.ToInt32(reader["ENROL_YR_LEVEL"]) : 0,
                                    StudentName = $"{reader["STUD_FNAME"]} {reader["STUD_LNAME"]}",
                                    Program = reader["PROG_CODE"]?.ToString(),
                                    ProgramName = reader["PROG_TITLE"]?.ToString()
                                };
                                return Json( new {enrollment = enrollment, success = true, message = "Successfully returned a student" }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                }
                return Json(new { error = "Enrollment not found" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        
        [HttpGet]
        [Route("/Admin/Enrollment/View")]
        public ActionResult GetEnrolledCoursesWithSchedules(int? enrollmentId)
        {
            try
            {
                // Validate input
                if (!enrollmentId.HasValue)
                {
                    return Json(new { 
                        success = false,
                        message = "Invalid enrollment ID"
                    }, JsonRequestBehavior.AllowGet);
                }

                using (var db = new NpgsqlConnection(_connectionString))
                {
                    db.Open();
                    
                    var cmd = new NpgsqlCommand(@"
                        SELECT 
                            ec.ENROL_ID AS EnrollmentId,
                            c.CRS_CODE AS Code,
                            c.CRS_TITLE AS Title,
                            c.CRS_UNITS AS Units,
                            c.CRS_LEC AS LecHours,
                            c.CRS_LAB AS LabHours,
                            ct.CTG_NAME AS CategoryName,
                            ct.CTG_CODE AS CategoryCode,
                            s.SCHD_ID AS ScheduleId,
                            (
                                SELECT ARRAY_AGG(p.PREQ_CRS_CODE)
                                FROM PREREQUISITE p
                                WHERE p.CRS_CODE = c.CRS_CODE
                            ) AS Prerequisites,
                            ts.TSL_DAY AS Day,
                            ts.TSL_START_TIME AS StartTime,
                            ts.TSL_END_TIME AS EndTime,
                            r.ROOM_CODE AS RoomNumber,
                            p.PROF_NAME AS InstructorName,
                            bs.BSEC_NAME AS SectionName
                        FROM ENROLLING_COURSE ec
                        INNER JOIN COURSE c ON ec.CRS_CODE = c.CRS_CODE
                        LEFT JOIN COURSE_CATEGORY ct ON c.CTG_CODE = ct.CTG_CODE
                        LEFT JOIN SCHEDULE s ON ec.SCHD_ID = s.SCHD_ID
                        LEFT JOIN TIME_SLOT ts ON s.SCHD_ID = ts.SCHD_ID
                        LEFT JOIN ROOM r ON s.ROOM_ID = r.ROOM_ID
                        LEFT JOIN PROFESSOR p ON s.PROF_ID = p.PROF_ID
                        LEFT JOIN BLOCK_SECTION bs ON s.BSEC_CODE = bs.BSEC_CODE
                        WHERE ec.ENROL_ID = @EnrollmentId
                        ORDER BY c.CRS_CODE, ts.TSL_DAY, ts.TSL_START_TIME", db);

                    cmd.Parameters.AddWithValue("@EnrollmentId", enrollmentId);

                    var courses = new List<dynamic>();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            courses.Add(new
                            {
                                EnrollmentId = Convert.ToInt32(reader["EnrollmentId"]),
                                Code = reader["Code"].ToString(),
                                Title = reader["Title"].ToString(),
                                Units = Convert.ToInt32(reader["Units"]),
                                LecHours = Convert.ToInt32(reader["LecHours"]),
                                LabHours = Convert.ToInt32(reader["LabHours"]),
                                CategoryName = reader["CategoryName"].ToString(),
                                CategoryCode = reader["CategoryCode"].ToString(),
                                Prerequisites = reader["Prerequisites"] as string[] ?? Array.Empty<string>(),
                                ScheduleId = reader.IsDBNull(reader.GetOrdinal("ScheduleId")) ? 0 : Convert.ToInt32(reader["ScheduleId"]),
                                Day = reader.IsDBNull(reader.GetOrdinal("Day")) ? null : reader["Day"].ToString(),
                                StartTime = reader.IsDBNull(reader.GetOrdinal("StartTime")) ? TimeSpan.Zero : (TimeSpan)reader["StartTime"],
                                EndTime = reader.IsDBNull(reader.GetOrdinal("EndTime")) ? TimeSpan.Zero : (TimeSpan)reader["EndTime"],
                                RoomNumber = reader.IsDBNull(reader.GetOrdinal("RoomNumber")) ? null : reader["RoomNumber"].ToString(),
                                InstructorName = reader.IsDBNull(reader.GetOrdinal("InstructorName")) ? null : reader["InstructorName"].ToString(),
                                SectionName = reader.IsDBNull(reader.GetOrdinal("SectionName")) ? null : reader["SectionName"].ToString()
                            });
                        }
                    }

                    // Group by course and format times
                    var groupedCourses = courses
                        .GroupBy(c => c.Code)
                        .Select(g => new
                        {
                            EnrollmentId = g.First().EnrollmentId,
                            Category = g.First().CategoryName,
                            Code = g.Key,
                            LabHours = g.First().LabHours,
                            LecHours = g.First().LecHours,
                            Prerequisites = g.First().Prerequisites,
                            Title = g.First().Title,
                            Units = g.First().Units,
                            ScheduleDetails = g.Where(x => x.ScheduleId != 0)
                                            .GroupBy(s => s.ScheduleId)
                                            .Select(sg => new
                                            {
                                                InstructorName = sg.First().InstructorName ?? "TBA",
                                                RoomNumber = sg.First().RoomNumber ?? "TBA",
                                                ScheduleId = sg.Key,
                                                SectionName = sg.First().SectionName ?? "TBA",
                                                TimeSlots = sg.Select(s => new
                                                {
                                                    Day = s.Day,
                                                    EndTime = new { 
                                                        Hours = s.EndTime.Hours,
                                                        Minutes = s.EndTime.Minutes
                                                    },
                                                    ScheduleId = s.ScheduleId,
                                                    StartTime = new {
                                                        Hours = s.StartTime.Hours,
                                                        Minutes = s.StartTime.Minutes
                                                    }
                                                })
                                                .OrderBy(s => s.Day)
                                                .ThenBy(s => s.StartTime.Hours)
                                                .ThenBy(s => s.StartTime.Minutes)
                                                .ToList()
                                            })
                                            .ToList()
                        })
                        .OrderBy(c => c.Code)
                        .ToList();

                    return Json(new
                    {
                        success = true,
                        data = groupedCourses
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "An error occurred while retrieving enrolled courses",
                    error = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}