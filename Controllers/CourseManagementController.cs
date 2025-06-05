using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using Enrollment_System.Controllers.Service;
using Newtonsoft.Json;
using Npgsql;
using Enrollment_System.Models;

namespace Enrollment_System.Controllers
{
    public class CourseManagementController : Controller
    {
        private readonly string _connectionString;
        private readonly BaseControllerServices _baseController;

        public CourseManagementController(BaseControllerServices baseController)
        {
            _baseController = baseController;
            _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DbConnection"].ConnectionString;
        }
        // GET
        public ActionResult Dashboard()
        {
            return View("~/Views/Admin/Dashboard.cshtml");
        }
        
        public ActionResult Students()
        {
            var students = _baseController.GetStudentsFromDatabase();
            return View("~/Views/Admin/StudentManagement.cshtml", students);
        }
      
        
        public ActionResult Schedules()
        {
            var courses = _baseController.GetCoursesFromDatabase();
            ViewBag.BlockSections = _baseController.GetBlockSectionsFromDatabase();
            ViewBag.Programs = _baseController.GetProgramsFromDatabase();
            ViewBag.YearLevels = _baseController.GetYearLevelFromDatabase();
            ViewBag.Semesters = _baseController.GetSemesterFromDatabase();
            return View("~/Views/Admin/SetSchedules.cshtml", courses);
        }
        
        [HttpPost]
        [Route("/Head/Schedules/SaveSchedule")]
        public ActionResult SaveSchedule(ScheduleViewModel model)
        {
            try
            {
                // Validate required fields
                if (model == null || model.Schedule == null || model.TimeSlot == null)
                {
                    return Json(new { 
                        mess = 0, 
                        error = "Invalid data format",
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
                            // Validate individual fields
                            if (string.IsNullOrEmpty(model.Schedule.CourseCode))
                            {
                                return Json(new { 
                                    mess = 0, 
                                    error = "Course code is required",
                                    field = "CourseCode"
                                });
                            }

                            if (string.IsNullOrEmpty(model.Schedule.BlockSectionCode))
                            {
                                return Json(new { 
                                    mess = 0, 
                                    error = "Section code is required",
                                    field = "BlockSectionCode"
                                });
                            }

                            if (model.Schedule.ProfessorId <= 0)
                            {
                                return Json(new { 
                                    mess = 0, 
                                    error = "Professor is required",
                                    field = "ProfessorId"
                                });
                            }

                            if (model.Schedule.RoomId <= 0)
                            {
                                return Json(new { 
                                    mess = 0, 
                                    error = "Room is required",
                                    field = "RoomId"
                                });
                            }

                            if (!model.TimeSlot.Any())
                            {
                                return Json(new { 
                                    mess = 0, 
                                    error = "At least one timeslot is required",
                                    field = "TimeSlot"
                                });
                            }

                            // Check for duplicate course in the same block section
                            var duplicateCourseCheckCmd = new NpgsqlCommand(
                                @"SELECT COUNT(*) FROM SCHEDULE 
                                  WHERE CRS_CODE = @courseCode 
                                  AND BSEC_CODE = @blockSectionCode", 
                                db, transaction);

                            duplicateCourseCheckCmd.Parameters.AddWithValue("@courseCode", model.Schedule.CourseCode);
                            duplicateCourseCheckCmd.Parameters.AddWithValue("@blockSectionCode", model.Schedule.BlockSectionCode);

                            var duplicateCount = Convert.ToInt32(duplicateCourseCheckCmd.ExecuteScalar());
                            if (duplicateCount > 0)
                            {
                                return Json(new { 
                                    mess = 0, 
                                    error = $"Course {model.Schedule.CourseCode} is already scheduled for this section",
                                    field = "CourseCode"
                                });
                            }

                            // Check for schedule conflicts (professor, room, and section)
                            foreach (var timeslot in model.TimeSlot)
                            {
                                // First check for section time conflicts (regardless of TBA status)
                                var sectionConflictCmd = new NpgsqlCommand(
                                    @"SELECT 
                                        c.CRS_CODE,
                                        c.CRS_TITLE,
                                        ts.TSL_DAY,
                                        TO_CHAR(ts.TSL_START_TIME, 'HH24:MI') AS START_TIME,
                                        TO_CHAR(ts.TSL_END_TIME, 'HH24:MI') AS END_TIME
                                      FROM SCHEDULE s
                                      JOIN TIME_SLOT ts ON s.SCHD_ID = ts.SCHD_ID
                                      JOIN COURSE c ON s.CRS_CODE = c.CRS_CODE
                                      WHERE s.BSEC_CODE = @blockSectionCode
                                      AND ts.TSL_DAY = @day
                                      AND (
                                          (ts.TSL_START_TIME < @endTime AND ts.TSL_END_TIME > @startTime)
                                      )", 
                                    db, transaction);

                                sectionConflictCmd.Parameters.AddWithValue("@blockSectionCode", model.Schedule.BlockSectionCode);
                                sectionConflictCmd.Parameters.AddWithValue("@day", timeslot.Day);
                                sectionConflictCmd.Parameters.AddWithValue("@startTime", timeslot.StartTime);
                                sectionConflictCmd.Parameters.AddWithValue("@endTime", timeslot.EndTime);

                                using (var reader = sectionConflictCmd.ExecuteReader())
                                {
                                    if (reader.HasRows)
                                    {
                                        reader.Read();
                                        return Json(new { 
                                            mess = 0, 
                                            error = $"Section already has a scheduled course ({reader["CRS_CODE"]} - {reader["CRS_TITLE"]}) " +
                                                   $"on {reader["TSL_DAY"]} from {reader["START_TIME"]} to {reader["END_TIME"]}",
                                            field = "TimeSlot"
                                        });
                                    }
                                }

                                // Then check for professor/room conflicts (with TBA handling)
                                var otherConflictCmd = new NpgsqlCommand(
                                    @"SELECT 
                                        c.CRS_CODE,
                                        c.CRS_TITLE,
                                        p.PROF_NAME,
                                        r.ROOM_CODE,
                                        ts.TSL_DAY,
                                        TO_CHAR(ts.TSL_START_TIME, 'HH24:MI') AS START_TIME,
                                        TO_CHAR(ts.TSL_END_TIME, 'HH24:MI') AS END_TIME
                                      FROM SCHEDULE s
                                      JOIN TIME_SLOT ts ON s.SCHD_ID = ts.SCHD_ID
                                      JOIN COURSE c ON s.CRS_CODE = c.CRS_CODE
                                      JOIN PROFESSOR p ON s.PROF_ID = p.PROF_ID
                                      JOIN ROOM r ON s.ROOM_ID = r.ROOM_ID
                                      WHERE ts.TSL_DAY = @day
                                      AND (
                                          (ts.TSL_START_TIME < @endTime AND ts.TSL_END_TIME > @startTime)
                                      )
                                      AND (
                                          (s.PROF_ID != 1 AND s.ROOM_ID != 1)
                                          AND (
                                              (@profId != 1 AND s.PROF_ID = @profId) OR
                                              (@roomId != 1 AND s.ROOM_ID = @roomId)
                                          )
                                      )", 
                                    db, transaction);

                                otherConflictCmd.Parameters.AddWithValue("@profId", model.Schedule.ProfessorId);
                                otherConflictCmd.Parameters.AddWithValue("@roomId", model.Schedule.RoomId);
                                otherConflictCmd.Parameters.AddWithValue("@day", timeslot.Day);
                                otherConflictCmd.Parameters.AddWithValue("@startTime", timeslot.StartTime);
                                otherConflictCmd.Parameters.AddWithValue("@endTime", timeslot.EndTime);

                                var conflicts = new List<string>();
                                using (var reader = otherConflictCmd.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        string conflictType = reader["PROF_ID"].ToString() == model.Schedule.ProfessorId.ToString() 
                                            ? "Professor" 
                                            : "Room";

                                        conflicts.Add(
                                            $"{conflictType} conflict: {reader["CRS_CODE"]} - {reader["CRS_TITLE"]} " +
                                            $"({reader["PROF_NAME"]}, {reader["ROOM_CODE"]}) " +
                                            $"on {reader["TSL_DAY"]} {reader["START_TIME"]}-{reader["END_TIME"]}"
                                        );
                                    }
                                }

                                if (conflicts.Any())
                                {
                                    return Json(new { 
                                        mess = 0, 
                                        error = $"Schedule conflict detected:\n{string.Join("\n", conflicts)}",
                                        field = "TimeSlot"
                                    });
                                }
                            }

                            // Insert schedule
                            var scheduleInsertCmd = new NpgsqlCommand(
                                @"INSERT INTO SCHEDULE 
                                  (CRS_CODE, BSEC_CODE, PROF_ID, ROOM_ID)
                                  VALUES (@CourseCode, @BlockSectionCode, @ProfessorId, @RoomId)
                                  RETURNING SCHD_ID", 
                                db, transaction);

                            scheduleInsertCmd.Parameters.AddWithValue("@CourseCode", model.Schedule.CourseCode);
                            scheduleInsertCmd.Parameters.AddWithValue("@BlockSectionCode", model.Schedule.BlockSectionCode);
                            scheduleInsertCmd.Parameters.AddWithValue("@ProfessorId", model.Schedule.ProfessorId);
                            scheduleInsertCmd.Parameters.AddWithValue("@RoomId", model.Schedule.RoomId);

                            var scheduleId = Convert.ToInt32(scheduleInsertCmd.ExecuteScalar());

                            // Insert timeslots
                            foreach (var timeslot in model.TimeSlot)
                            {
                                var timeslotInsertCmd = new NpgsqlCommand(
                                    @"INSERT INTO TIME_SLOT
                                      (SCHD_ID, TSL_DAY, TSL_START_TIME, TSL_END_TIME)
                                      VALUES (@ScheduleId, @Day, @StartTime, @EndTime)", 
                                    db, transaction);

                                timeslotInsertCmd.Parameters.AddWithValue("@ScheduleId", scheduleId);
                                timeslotInsertCmd.Parameters.AddWithValue("@Day", timeslot.Day);
                                timeslotInsertCmd.Parameters.AddWithValue("@StartTime", timeslot.StartTime);
                                timeslotInsertCmd.Parameters.AddWithValue("@EndTime", timeslot.EndTime);

                                timeslotInsertCmd.ExecuteNonQuery();
                            }

                            transaction.Commit();
                            return Json(new {
                                mess = 1,
                                message = "Schedule saved successfully",
                                redirectUrl = Url.Action("Schedules", "CourseManagement")
                            });
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            return Json(new { 
                                mess = 0, 
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
                    mess = 0, 
                    error = ex.Message,
                    field = "general"
                });
            }
        }
                
        [HttpGet]
        [Route("/Head/Schedules/GetSchedule")]
        public ActionResult GetSchedule(string sectionId)
        {
            try
            {
                // Validate required field
                if (string.IsNullOrWhiteSpace(sectionId))
                {
                    return Json(new { 
                        success = false,
                        message = "Validation failed",
                        error = "Section ID is required",
                        field = "sectionId"
                    }, JsonRequestBehavior.AllowGet);
                }

                using (var db = new NpgsqlConnection(_connectionString))
                {
                    db.Open();
                    
                    // Get schedule data with structure matching GetAvailableCourse
                    var scheduleQuery = @"
                        SELECT 
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
                            bs.BSEC_NAME AS SectionName,
                            EXISTS (
                                SELECT 1 FROM SCHEDULE s2
                                JOIN TIME_SLOT ts2 ON s2.SCHD_ID = ts2.SCHD_ID
                                WHERE (s2.PROF_ID = s.PROF_ID OR s2.ROOM_ID = s.ROOM_ID)
                                AND s2.SCHD_ID != s.SCHD_ID
                                AND ts2.TSL_DAY = ts.TSL_DAY
                                AND (
                                    (ts2.TSL_START_TIME < ts.TSL_END_TIME AND ts2.TSL_END_TIME > ts.TSL_START_TIME)
                                )
                            ) AS HasConflict
                        FROM SCHEDULE s
                        JOIN TIME_SLOT ts ON s.SCHD_ID = ts.SCHD_ID
                        JOIN COURSE c ON s.CRS_CODE = c.CRS_CODE
                        LEFT JOIN COURSE_CATEGORY ct ON c.CTG_CODE = ct.CTG_CODE
                        JOIN PROFESSOR p ON s.PROF_ID = p.PROF_ID
                        JOIN ROOM r ON s.ROOM_ID = r.ROOM_ID
                        JOIN BLOCK_SECTION bs ON s.BSEC_CODE = bs.BSEC_CODE
                        WHERE s.BSEC_CODE = @SectionId
                        ORDER BY c.CRS_CODE, ts.TSL_DAY, ts.TSL_START_TIME";

                    var scheduleCmd = new NpgsqlCommand(scheduleQuery, db);
                    scheduleCmd.Parameters.AddWithValue("@SectionId", sectionId);

                    var courses = new List<dynamic>();
                    using (var reader = scheduleCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            courses.Add(new
                            {
                                Code = reader["Code"].ToString(),
                                Title = reader["Title"].ToString(),
                                Units = Convert.ToInt32(reader["Units"]),
                                LecHours = Convert.ToInt32(reader["LecHours"]),
                                LabHours = Convert.ToInt32(reader["LabHours"]),
                                CategoryName = reader["CategoryName"].ToString(),
                                CategoryCode = reader["CategoryCode"].ToString(),
                                Prerequisites = reader["Prerequisites"] as string[] ?? Array.Empty<string>(),
                                ScheduleId = Convert.ToInt32(reader["ScheduleId"]),
                                Day = reader["Day"].ToString(),
                                StartTime = (TimeSpan)reader["StartTime"],
                                EndTime = (TimeSpan)reader["EndTime"],
                                RoomNumber = reader["RoomNumber"].ToString(),
                                InstructorName = reader["InstructorName"].ToString(),
                                SectionName = reader["SectionName"].ToString(),
                                HasConflict = Convert.ToBoolean(reader["HasConflict"])
                            });
                        }
                    }

                    // Group by course and format times to match GetAvailableCourse structure
                    var groupedCourses = courses
                        .GroupBy(c => c.Code)
                        .Select(g => new
                        {
                            Category = g.First().CategoryName,
                            Code = g.Key,
                            LabHours = g.First().LabHours,
                            LecHours = g.First().LecHours,
                            Prerequisites = g.First().Prerequisites,
                            Title = g.First().Title,
                            Units = g.First().Units,
                            ScheduleDetails = g.GroupBy(s => s.ScheduleId)
                                            .Select(sg => new
                                            {
                                                InstructorName = sg.First().InstructorName,
                                                RoomNumber = sg.First().RoomNumber,
                                                ScheduleId = sg.Key,
                                                SectionName = sg.First().SectionName,
                                                TimeSlots = sg.Select(s => new
                                                {
                                                    Day = s.Day,
                                                    EndTime = new { 
                                                        Hours = s.EndTime.Hours,
                                                        Minutes = s.EndTime.Minutes
                                                    },
                                                    StartTime = new {
                                                        Hours = s.StartTime.Hours,
                                                        Minutes = s.StartTime.Minutes
                                                    },
                                                    HasConflict = s.HasConflict
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

                    if (groupedCourses.Count == 0)
                    {
                        return Json(new { 
                            success = true,
                            message = "No schedule found for this section",
                            data = new List<object>()
                        }, JsonRequestBehavior.AllowGet);
                    }

                    return Json(new {
                        success = true,
                        message = "Schedule loaded successfully",
                        data = groupedCourses
                    }, JsonRequestBehavior.AllowGet);
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
        
        [HttpGet]
        [Route("/Head/Curriculum/GetCurriculumCourses")]
        public ActionResult GetCurriculumCourses(string programCode, int? yearLevel, int? semester)
        {
            try
            {
                // Validate required field
                if (string.IsNullOrWhiteSpace(programCode))
                {
                    return Json(new { 
                        success = false,
                        message = "Validation failed",
                        error = "Program code is required",
                        field = "programCode"
                    }, JsonRequestBehavior.AllowGet);
                }

                using (var db = new NpgsqlConnection(_connectionString))
                {
                    db.Open();
                    
                    // Get only the course data needed for dropdown options
                    var curriculumQuery = @"
                        SELECT 
                            c.CRS_CODE AS Code,
                            c.CRS_TITLE AS Title,
                            c.CRS_UNITS AS Units,
                            c.CRS_LEC AS LecHours,
                            c.CRS_LAB AS LabHours
                        FROM CURRICULUM_COURSE cc
                        JOIN COURSE c ON cc.CRS_CODE = c.CRS_CODE
                        JOIN CURRICULUM cur ON cc.CUR_CODE = cur.CUR_CODE
                        WHERE cur.PROG_CODE = @ProgramCode
                        AND (@YearLevel IS NULL OR cc.CUR_YEAR_LEVEL = @YearLevel)
                        AND (@Semester IS NULL OR cc.CUR_SEMESTER = @Semester)
                        ORDER BY c.CRS_CODE";

                    var curriculumCmd = new NpgsqlCommand(curriculumQuery, db);
                    curriculumCmd.Parameters.AddWithValue("@ProgramCode", programCode);
                    curriculumCmd.Parameters.AddWithValue("@YearLevel", yearLevel ?? (object)DBNull.Value);
                    curriculumCmd.Parameters.AddWithValue("@Semester", semester ?? (object)DBNull.Value);

                    var courses = new List<dynamic>();
                    using (var reader = curriculumCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            courses.Add(new
                            {
                                Code = reader["Code"].ToString(),
                                Title = reader["Title"].ToString(),
                                Units = Convert.ToDecimal(reader["Units"]),
                                LecHours = Convert.ToInt32(reader["LecHours"]),
                                LabHours = Convert.ToInt32(reader["LabHours"])
                            });
                        }
                    }

                    if (courses.Count == 0)
                    {
                        return Json(new { 
                            success = true,
                            message = "No courses found in this curriculum",
                            data = new List<object>()
                        }, JsonRequestBehavior.AllowGet);
                    }

                    return Json(new {
                        success = true,
                        message = "Curriculum courses loaded successfully",
                        data = courses
                    }, JsonRequestBehavior.AllowGet);
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
        
        public ActionResult StudentManagement()
        {
            return View("~/Views/Admin/StudentManagement.cshtml");
        }
        public ActionResult ClassManagement()
        {
            return View("~/Views/Admin/ClassManagement.cshtml");
        }
        
        
        
    }
}