using DOHPayroll.DTRModels;
using DOHPayroll.Models.PostModels;
using DOHPayroll.ViewModels;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace DOHPayroll.Databases
{
    public sealed class DTRDatabase
    {
        public static string CONNECTION_STRING { get; set; }

        private static DTRDatabase INSTANCE;

        private DTRDatabase() { }

        public static DTRDatabase Instance
        {
            get
            {
                if(INSTANCE == null)
                {
                    INSTANCE = new DTRDatabase();
                    INSTANCE.Initialize(0);
                }
                return INSTANCE;
            }
        }

        public void Initialize(int index)
        {
            CONNECTION_STRING += "SERVER=" + ConnectionString.Servers[index] + ";";
            CONNECTION_STRING += "DATABASE=dohdtr;";
            CONNECTION_STRING += "UID=" + ConnectionString.UIDs[index] + ";";
            CONNECTION_STRING += "PASSWORD=" + ConnectionString.Password[index] + ";";
            CONNECTION_STRING += "pooling = false; SslMode=none; convert zero datetime=True";
        }
        #region GET USER BY USERNAME
        public async Task<CookiesModel> GetUserByUsername(string username)
        {
            string query = "SELECT d.fname, d.mname, d.lname, d.username, d.password, d.userid, wat.position, wat.designation , wat.division, wat.section FROM dohdtr.users d " +
                "LEFT JOIN " +
                "(SELECT p.userid, p.position, p.designation_id, p.division_id, p.section_id, ds.description as designation, dv.description as division, sc.description as section " +
                "FROM pis.personal_information p  " +
                "LEFT JOIN dts.designation ds ON ds.id = p.designation_id  " +
                "LEFT JOIN dts.division dv ON dv.id = p.division_id  " +
                "LEFT JOIN dts.section sc ON sc.id = p.section_id  ) AS wat  " +
                "ON wat.userid = d.userid  " +
                "WHERE username = '"+username+"'";

            List<CookiesModel> users = new List<CookiesModel>();

            using(MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    var reader = await command.ExecuteReaderAsync();

                    while(await reader.ReadAsync())
                    {
                        users.Add(new CookiesModel
                        {
                            Fname = reader["fname"].ToString(),
                            Mname = reader["mname"].ToString(),
                            Lname = reader["lname"].ToString(),
                            Username = reader["username"].ToString(),
                            Password = reader["password"].ToString(),
                            UserId = reader["userid"].ToString(),
                            Position = reader["position"].ToString(),
                            Designation = reader["designation"].ToString(),
                            Division = reader["division"].ToString(),
                            Section = reader["section"].ToString()
                        });
                    }
                    await reader.CloseAsync();
                }
                await connection.CloseAsync();
            }

            return users.FirstOrDefault();
        }
        #endregion
        #region GET MINS LATE AND ABSENSES
        public async Task<(int, string, double)> GetMinsV2Async(string userid, DateTime from, DateTime to, string job_status)
        {
            List<LogsModel> model = new List<LogsModel>();

            string procedure = job_status == "jo" ? "GETLOGS2" : "Gliding_2020";
            string query = "CALL dohdtr." + procedure + "('" + userid + "', '" + from.ToString("yyyy-MM-dd") + "', '" + to.ToString("yyyy-MM-dd") + "')";

            int mins = 0;
            double days_rendered = 0;
            string days_absent = "";

            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        string id = reader["userid"].ToString();
                        string fullname = reader["name"].ToString();
                        string dayname = reader["dayname"].ToString();
                        DateTime datein = DateTime.Parse(reader["datein"].ToString());
                        var time = reader["time"].ToString().Split('|');
                        int late = 0;
                        int.TryParse(reader["late"].ToString(), out late);
                        int undertime = 0;
                        int.TryParse(reader["undertime"].ToString(), out undertime);
                        bool absent = (time[0].Split('_')[0] == "empty" && time[1].Split('_')[0] == "empty" && time[2].Split('_')[0] == "empty" && time[3].Split('_')[0] == "empty");
                        bool halfAm = false;
                        bool halfPm = false;
                        if (!absent)
                        {
                            halfAm = (time[0].Split('_')[0] == "empty" && time[1].Split('_')[0] == "empty");
                            halfPm = (time[2].Split('_')[0] == "empty" && time[3].Split('_')[0] == "empty");
                        }
                        else
                            days_absent += datein.ToString("MM/dd/yyyy") + ",";
                        model.Add(new LogsModel
                        {
                            UserId = userid,
                            FullName = fullname,
                            DateIn = datein,
                            AMIN = time[0],
                            AMOUT = time[1],
                            PMIN = time[2],
                            PMOUT = time[3],
                            Late = late,
                            UnderTime = undertime,
                            Absent = absent,
                            HalfDay = halfAm || halfPm,
                            //HalfDayPM = halfPm,
                            DayName = dayname
                        });
                    }
                    await reader.CloseAsync();
                }
                await connection.CloseAsync();
            }

            var working = model.Where(x => x.DayName != "Sunday").Where(x => x.DayName != "Saturday").Where(x => !x.AMIN.Contains("HOLIDAY"));
            mins = working.Sum(x => x.Late) + working.Sum(x => x.UnderTime) + (working.Where(x => x.HalfDay == true).Count() * 240);
            var halfdays = (working.Where(x => x.HalfDay == true).Count() * 0.5);
            days_rendered = working.Where(x => !x.AMIN.Split('_')[0].Equals("empty") && !x.AMOUT.Split('_')[0].Equals("empty") && !x.PMIN.Split('_')[0].Equals("empty") && !x.PMOUT.Split('_')[0].Equals("empty")).Count() + halfdays;

            return (mins, days_absent.RemoveLastChar(), days_rendered);
        }
        #endregion

        #region GET WORKING DAYS
        public int GetWorkingDays(DateTime date)
        {
            int month = date.Month;//int.Parse(from.Split(delimiter)[1]);
            int year = date.Year;//int.Parse(from.Split(delimiter)[0]);
            int no_days = DateTime.DaysInMonth(year, month);
            int working_days = 0;

            for (int i = 1; i <= no_days; i++)
            {
                string format = month + "/" + i + "/" + year;
                if (!ifWeekend(format) && !IsHoliday(format))
                {
                    working_days++;
                }
            }
            return working_days;
        }
        #endregion

        #region CHECK IF WEEKEND
        public bool ifWeekend(string format)
        {
            var date = DateTime.Parse(format);
            var datez = date.DayOfWeek;
            if ((datez == DayOfWeek.Saturday) || (datez == DayOfWeek.Sunday))
            {
                return true;
            }
            return false;
        }
        #endregion
        #region CHECK IF HOLIDAY
        public bool IsHoliday(string date)
        {
            bool found = false;
            string month = (int.Parse(date.Split('/')[0]) > 9) ? date.Split('/')[0] : "0" + date.Split('/')[0];
            string day = (int.Parse(date.Split('/')[1]) > 9) ? date.Split('/')[1] : "0" + date.Split('/')[1];
            string year = date.Split('/')[2];

            string mDate = year + "-" + month + "-" + day;

            string query = "SELECT id FROM calendar WHERE start <= '" + mDate + "' AND end > '" + mDate + "' AND status = '1'";

            using (MySqlConnection SqlConnection = new MySqlConnection(CONNECTION_STRING))
            {
                SqlConnection.Open();
                using (MySqlCommand cmd = new MySqlCommand(query, SqlConnection))
                {
                    MySqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        found = true;
                    }
                    reader.Close();
                }
                SqlConnection.Close();
            }
            return found;
        }
        #endregion
    }
}
