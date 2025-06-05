using System;
using System.Web.Mvc;
using Enrollment_System.Models;
using Npgsql;
using System.Configuration;
using Enrollment_System.Utilities;

namespace Enrollment_System.Controllers
{
    public class AccountController : Controller
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["DbConnection"].ConnectionString;

        [HttpGet]
        public ActionResult SignUp()
        {
            return View("~/Views/Account/SignUp.cshtml");
        }
        
        // GET: /Account/LogIn
        [HttpGet]
        public ActionResult StudentLogIn()
        {
            return View("~/Views/Account/StudentLogIn.cshtml");
        }
        
        // GET: Account/Login/Professor
        [HttpGet]
        public ActionResult FacultyLogIn()
        {
            return View("~/Views/Account/FacultyLogin.cshtml");
        }
        
        [HttpPost]
        [Route("/Account/SignUp")]
        public ActionResult SignUp(Student student) {
            try
            {
                // Validate required fields
                if (student.Id < 0 ||
                    string.IsNullOrEmpty(student.LastName) || 
                    string.IsNullOrEmpty(student.FirstName) ||
                    student.Birthdate == DateTime.MinValue || 
                    string.IsNullOrEmpty(student.HomeAddress) || 
                    string.IsNullOrEmpty(student.Contact) ||
                    string.IsNullOrEmpty(student.Email) || 
                    string.IsNullOrEmpty(student.Password))
                {
                    return Json(new { mess = 0, error = "All required fields must be filled." }, JsonRequestBehavior.AllowGet);
                }

                using (var db = new NpgsqlConnection(_connectionString))
                {
                    db.Open();

                    // Check if student code already exists
                    using (var checkCmd = new NpgsqlCommand("SELECT COUNT(*) FROM STUDENT WHERE STUD_ID = @StudentID", db))
                    {
                        checkCmd.Parameters.AddWithValue("@StudentID", student.Id);
                        int studCodeExists = Convert.ToInt32(checkCmd.ExecuteScalar());

                        if (studCodeExists > 0)
                        {
                            return Json(new { mess = 2, error = "Student code already exists.", field = "Id" }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    // Check if email already exists
                    using (var checkEmailCmd = new NpgsqlCommand("SELECT COUNT(*) FROM STUDENT WHERE STUD_EMAIL = @Email", db))
                    {
                        checkEmailCmd.Parameters.AddWithValue("@Email", student.Email);
                        int emailExists = Convert.ToInt32(checkEmailCmd.ExecuteScalar());

                        if (emailExists > 0)
                        {
                            return Json(new { mess = 3, error = "Email address is already in use.", field = "Stud_Email" }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    // Hash the password
                    var hashedPassword = PasswordUtil.HashPassword(student.Password);

                    // Insert student record
                    using (var cmd = new NpgsqlCommand(@"
                        INSERT INTO STUDENT 
                        (STUD_ID, STUD_LNAME, STUD_FNAME, STUD_MNAME, STUD_DOB, STUD_CONTACT, STUD_EMAIL, STUD_HOME_ADDRESS, PASSWORD_HASH)
                        VALUES (@studentId, @lastName, @firstName, @middleName, @birthDate, @contactNo, @emailAddress, @address, @password)", db))
                    {
                        cmd.Parameters.AddWithValue("@studentId", student.Id);
                        cmd.Parameters.AddWithValue("@lastName", student.LastName);
                        cmd.Parameters.AddWithValue("@firstName", student.FirstName);
                        cmd.Parameters.AddWithValue("@middleName", string.IsNullOrEmpty(student.MiddleName) ? DBNull.Value : (object)student.MiddleName);
                        cmd.Parameters.AddWithValue("@birthDate", student.Birthdate);
                        cmd.Parameters.AddWithValue("@contactNo", student.Contact);
                        cmd.Parameters.AddWithValue("@emailAddress", student.Email);
                        cmd.Parameters.AddWithValue("@address", student.HomeAddress);
                        cmd.Parameters.AddWithValue("@password", hashedPassword);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            return Json(new
                            {
                                mess = 1,
                                message = "Student record created successfully.",
                                redirectUrl = Url.Action("StudentLogIn", "Account", new { message = "Student record created successfully." })
                            }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(new { mess = 0, error = "Failed to create student record." }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { mess = 0, error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        
        [HttpPost]
        [Route("Account/Student/LogIn")]
        public ActionResult StudentLogIn(Student student)
        {
            try
            {
                if (student.Id < 0 || string.IsNullOrEmpty(student.Password))
                {
                    return Json(new { mess = 0, error = "All required fields must be filled." }, JsonRequestBehavior.AllowGet);
                }

                using (var db = new NpgsqlConnection(_connectionString))
                {
                    db.Open();
                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandText = "SELECT STUD_ID, STUD_FNAME, STUD_LNAME, STUD_EMAIL, PASSWORD_HASH FROM STUDENT WHERE STUD_ID = @StudentId";
                        cmd.Parameters.AddWithValue("@StudentId", student.Id);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                return Json(new { success = false, message = "Invalid student code or password." }, JsonRequestBehavior.AllowGet);
                            }

                            reader.Read();

                            var hashedPassword = reader["PASSWORD_HASH"].ToString();

                            if (!PasswordUtil.VerifyPassword(student.Password, hashedPassword))
                            {
                                return Json(new { success = false, message = "Invalid student code or password." }, JsonRequestBehavior.AllowGet);
                            }
                            
                            var studentData = new
                            {
                                Id = reader["STUD_ID"].ToString(),
                                FirstName = reader["STUD_FNAME"].ToString(),
                                LastName = reader["STUD_LNAME"].ToString(),
                                Email = reader["STUD_EMAIL"].ToString()
                            };
                            Session["StudentId"] = student.Id;
                            return Json(new
                            { 
                                success = true,
                                message = "Login successful!",
                                data = studentData,
                                redirectUrl = Url.Action("Dashboard", "Student", new { message = "Student record created successfully." })
                            }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message }, JsonRequestBehavior.DenyGet);
            }
        }
        
        [HttpPost]
        [Route("Account/Faculty/LogIn")]
        public ActionResult FacultyLogIn(FacultyAccount facultyAccount)
        {
            try
            {
                if (facultyAccount.Id < 0 || string.IsNullOrEmpty(facultyAccount.Password))
                {
                    return Json(new { success = false, message = "All required fields must be filled." }, JsonRequestBehavior.AllowGet);
                }

                using (var db = new NpgsqlConnection(_connectionString))
                {
                    db.Open();
                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandText = "SELECT FCL_ID, FCL_NAME, FCL_PASSWORD, FCL_TYPE FROM FACULTY WHERE FCL_ID = @FacultyId";
                        cmd.Parameters.AddWithValue("@FacultyId", facultyAccount.Id);
                        
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                return Json(new { success = false, message = "Invalid faculty ID or password." }, JsonRequestBehavior.AllowGet);
                            }

                            reader.Read();

                            string storedPassword = reader["FCL_PASSWORD"].ToString(); 
                            string inputPassword = facultyAccount.Password; 

                            if (inputPassword != storedPassword)
                            {
                                return Json(new { success = false, message = "Invalid faculty ID or password." }, JsonRequestBehavior.AllowGet);
                            }
                            
                            var facultyType = reader["FCL_TYPE"].ToString();
                            string redirectUrl = "";
                            
                            switch (facultyType.ToLower())
                            {
                                case "admin":
                                    redirectUrl = Url.Action("Dashboard", "Admin");
                                    break;
                                default:
                                    return Json(new { success = false, message = "Unknown faculty type." }, JsonRequestBehavior.AllowGet);
                            }
                            
                            var facultyData = new
                            {
                                Id = reader["FCL_ID"].ToString(),
                                Name = reader["FCL_NAME"].ToString(), // Fixed: Your table has FCL_NAME, not FCL_FNAME/FCL_LNAME
                                Type = facultyType
                            };
                            
                            return Json(new
                            { 
                                success = true,
                                message = "Login successful!",
                                data = facultyData,
                                redirectUrl = redirectUrl
                            }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message }, JsonRequestBehavior.DenyGet);
            }
        }
        
        [HttpGet]
        public ActionResult LogOut()
        {
            return View("~/Views/Home/Index.cshtml");
        }
        
    }
}