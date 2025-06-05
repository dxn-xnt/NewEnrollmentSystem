using System;
using System.Collections.Generic;
using Npgsql;
using Enrollment_System.Models;

namespace Enrollment_System.Controllers.Service
{
    public class BaseControllerServices
    {
        private readonly string _connectionString;
        
        public BaseControllerServices(string connectionString)
        {
            _connectionString = connectionString;
        }
        
        public List<CourseCategory> GetCourseCategoriesFromDatabase()
        {
            var categories = new List<CourseCategory>();

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT \"ctg_code\", \"ctg_name\" FROM \"course_category\"", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            categories.Add(new CourseCategory
                            {
                                Code = reader.GetString(0),
                                Name = reader.GetString(1)
                            });
                        }
                    }
                }
            }

            return categories;
        }
        
        public List<Course> GetCoursesFromDatabase()
        {
            var courses = new List<Course>();

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT \"crs_code\", \"crs_title\", \"crs_units\", \"crs_lec\", \"crs_lab\", \"ctg_code\" FROM \"course\"", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            courses.Add(new Course
                            {
                                Code = reader.GetString(0),
                                Title = reader.GetString(1),
                                Units = reader.GetInt32(2),
                                LecHours = reader.GetInt32(3),
                                LabHours = reader.GetInt32(4),
                                CategoryCode = reader.GetString(5)
                            });
                        }
                    }
                }
            }

            return courses;
        }
        
        public List<Curriculum> GetCurriculumFromDatabase()
        {
            var curricula = new List<Curriculum>();

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT \"cur_code\", \"prog_code\", \"ay_code\" FROM \"curriculum\"", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            curricula.Add(new Curriculum
                            {
                                Code = reader.GetString(0),
                                ProgramCode = reader.GetString(1),
                                AcademicYear = reader.GetString(2),
                            });
                        }
                    }
                }
            }

            return curricula;
        }
        
        public List<CurriculumCourse> GetCurriculumCoursesFromDatabase()
        {
            var curriculumCourses = new List<CurriculumCourse>();

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT \"cur_code\", \"crs_code\", \"cur_year_level\", \"cur_semester\" FROM \"curriculum_course\"", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            curriculumCourses.Add(new CurriculumCourse
                            {
                                CurriculumCode = reader.GetString(0),
                                CourseCode = reader.GetString(1),
                                YearLevel = reader.GetInt16(2),
                                Semester = reader.GetInt16(3)
                            });
                        }
                    }
                }
            }

            return curriculumCourses;
        }
        
        public List<Prerequisite> GetPrerequisitesFromDatabase()
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
        
        public List<Program> GetProgramsFromDatabase()
        {
            var programs = new List<Program>();

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT \"prog_code\", \"prog_title\" FROM \"program\"", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            programs.Add(new Program
                            {
                                Code = reader.GetString(0),
                                Title = reader.GetString(1)
                            });
                        }
                    }
                }
            }

            return programs;
        }

        public List<AcademicYear> GetAcademicYearsFromDatabase()
        {
            var academicYears = new List<AcademicYear>();

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT \"ay_code\", \"ay_start_year\", \"ay_end_year\" FROM \"academic_year\"", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            academicYears.Add(new AcademicYear
                            {
                                Code = reader.GetString(0),
                                StartYear = reader.GetInt16(1),
                                EndYear = reader.GetInt16(2),
                            });
                        }
                    }
                }
            }

            return academicYears;
        }
        
        public List<YearLevel> GetYearLevelFromDatabase()
        {
            var yearLevel = new List<YearLevel>();

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT \"yrl_id\", \"yrl_title\" FROM \"year_level\" ", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            yearLevel.Add(new YearLevel
                            {
                                Id = reader.GetInt16(0),
                                Title = reader.GetString(1),
                            });
                        }
                    }
                }
            }

            return yearLevel;
        }

        public List<Semester> GetSemesterFromDatabase()
        {
            var semesters = new List<Semester>();

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT \"sem_id\", \"sem_name\" FROM \"semester\"", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            semesters.Add(new Semester
                            {
                                Id = reader.GetInt16(0),
                                Name = reader.GetString(1)
                            });
                        }
                    }
                }
            }

            return semesters;
        }
        
        public List<BlockSection> GetBlockSectionsFromDatabase()
        {
            var blockSections = new List<BlockSection>();

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(@"
                SELECT 
                    BSEC_CODE,
                    BSEC_NAME,
                    BSEC_STATUS,
                    PROG_CODE,
                    AY_CODE,
                    SEM_ID,
                    YRL_ID
                FROM BLOCK_SECTION", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            blockSections.Add(new BlockSection
                            {
                                Code = reader["BSEC_CODE"]?.ToString(),
                                Name = reader["BSEC_NAME"]?.ToString(),
                                Status = reader["BSEC_STATUS"]?.ToString(),
                                ProgramCode = reader["PROG_CODE"]?.ToString(),
                                AcademicYear = reader["AY_CODE"]?.ToString(),
                                Semester = reader["SEM_ID"] != DBNull.Value ? Convert.ToInt32(reader["SEM_ID"]) : 0,
                                YearLevel = reader["YRL_ID"] != DBNull.Value ? Convert.ToInt32(reader["YRL_ID"]) : 0
                            });
                        }
                    }
                }
            }

            return blockSections;
        }
        
        //Get All Students
        public List<Student> GetStudentsFromDatabase()
        {
            var students = new List<Student>();

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(
                    @"SELECT 
                        stud_id, stud_fname, stud_lname, stud_mname, 
                        stud_email, stud_dob, stud_contact, stud_city_address, 
                        stud_home_address, stud_district, stud_is_first_gen, 
                        stud_yr_level, stud_major, stud_status, stud_sem, 
                        bsec_code, prog_code
                      FROM student", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            students.Add(new Student
                            {
                                Id = reader.GetInt32(0),
                                FirstName = reader.GetString(1),
                                LastName = reader.GetString(2),
                                MiddleName = reader.IsDBNull(3) ? null : reader.GetString(3),
                                Email = reader.GetString(4),
                                Birthdate = reader.GetDateTime(5),
                                Contact = reader.GetString(6),
                                CityAddress = reader.IsDBNull(7) ? null : reader.GetString(7),
                                HomeAddress = reader.IsDBNull(8) ? null : reader.GetString(8),
                                District = reader.IsDBNull(9) ? null : reader.GetString(9),
                                IsFirstGen = reader.GetBoolean(10),
                                YearLevel = reader.IsDBNull(11) ? 0 : reader.GetInt32(11),
                                Major = reader.IsDBNull(12) ? null : reader.GetString(12),
                                Status = reader.IsDBNull(13) ? null : reader.GetString(13),
                                Semester = reader.IsDBNull(14) ? 0 : reader.GetInt32(14),
                                BlockSection = reader.IsDBNull(15) ? null : reader.GetString(15),
                                ProgramCode = reader.IsDBNull(16) ? null : reader.GetString(16)
                            });
                        }
                    }
                }
            }

            return students;
        }
        
        //Get Student by ID
        public Student GetStudentFromDatabase(int studentId)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(
                    @"SELECT 
                        stud_id, stud_fname, stud_lname, stud_mname, 
                        stud_email, stud_dob, stud_contact, stud_city_address, 
                        stud_home_address, stud_district, stud_is_first_gen, 
                        stud_yr_level, stud_major, stud_status, stud_sem, 
                        bsec_code, prog_code
                      FROM student
                      WHERE stud_id = @StudentId", conn))
                {
                    cmd.Parameters.AddWithValue("@StudentId", studentId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Student
                            {
                                Id = reader.GetInt32(0),
                                FirstName = reader.GetString(1),
                                LastName = reader.GetString(2),
                                MiddleName = reader.IsDBNull(3) ? null : reader.GetString(3),
                                Email = reader.GetString(4),
                                Birthdate = reader.GetDateTime(5),
                                Contact = reader.GetString(6),
                                CityAddress = reader.IsDBNull(7) ? null : reader.GetString(7),
                                HomeAddress = reader.IsDBNull(8) ? null : reader.GetString(8),
                                District = reader.IsDBNull(9) ? null : reader.GetString(9),
                                IsFirstGen = reader.GetBoolean(10),
                                YearLevel = reader.IsDBNull(11) ? 0 : reader.GetInt32(11),
                                Major = reader.IsDBNull(12) ? null : reader.GetString(12),
                                Status = reader.IsDBNull(13) ? null : reader.GetString(13),
                                Semester = reader.IsDBNull(14) ? 0 : reader.GetInt32(14),
                                BlockSection = reader.IsDBNull(15) ? null : reader.GetString(15),
                                ProgramCode = reader.IsDBNull(16) ? null : reader.GetString(16)
                            };
                        }
                    }
                }
            }
            return null; // Return null if no student found
        }
        
        //Get All Enrollments
        public List<Enrollment> GetEnrollmentsFromDatabase()
        {
            var enrollments = new List<Enrollment>();

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
                        p.PROG_CODE
                    FROM ENROLLMENT e
                    JOIN STUDENT s ON e.STUD_ID = s.STUD_ID
                    JOIN PROGRAM p ON s.PROG_CODE = p.PROG_CODE
                    WHERE e.ENROL_STATUS = 'Pending'", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            enrollments.Add(new Enrollment
                            {
                                Id = Convert.ToInt32(reader["ENROL_ID"]),
                                Status = reader["ENROL_STATUS"]?.ToString(),
                                Date = Convert.ToDateTime(reader["ENROL_DATE"]).ToString("yyyy-MM-dd"),
                                StudentId = Convert.ToInt32(reader["STUD_ID"]),
                                AcademicYear = reader["AY_CODE"]?.ToString(),
                                Semester = reader["ENROL_SEM"] != DBNull.Value ? Convert.ToInt32(reader["ENROL_SEM"]) : 0,
                                YearLevel = reader["ENROL_YR_LEVEL"] != DBNull.Value ? Convert.ToInt32(reader["ENROL_YR_LEVEL"]) : 0,
                                StudentName = $"{reader["STUD_FNAME"]} {reader["STUD_LNAME"]}",
                                Program = reader["PROG_CODE"]?.ToString()
                            });
                        }
                    }
                }
            }

            return enrollments;
        }
        
        
    }
}