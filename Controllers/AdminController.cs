using System.Web.Mvc;
using System.Collections.Generic;
using Enrollment_System.Models;
using Npgsql;
namespace Enrollment_System.Controllers
{
    public class AdminController : Controller
    {
        private readonly string _connectionString;

        public AdminController()
        {
            _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DbConnection"].ConnectionString;
        }

        public ActionResult Dashboard()
        {
            List<int> statList = GetDashboardStat();
            ViewBag.statList = statList;
            return View("~/Views/Admin/Dashboard.cshtml");
        }

        public List<int> GetDashboardStat()
        {
            var statList = new List<int>();
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                statList.Add(GetStat(conn, "SELECT COUNT(*) FROM student"));
                statList.Add(GetStat(conn, "SELECT COUNT(*) FROM Faculty WHERE FCL_TYPE = 'admin'"));
                statList.Add(GetStat(conn, "SELECT COUNT(*) FROM Course"));
            }
            return statList;
        }

        public int GetStat(NpgsqlConnection conn, string query)
        {
            int stat = 0;
            using (var cmd = new NpgsqlCommand(query, conn))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        stat = reader.GetInt32(0);
                    }
                }   
            }
            return stat;
        }
    }
}