using DOHPayroll.Models;
using DOHPayroll.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOHPayroll.Databases
{
    public sealed class PISDatabase
    {
        #region INITIALIZERS
        public static string CONNECTION_STRING { get; set; }

        private static PISDatabase INSTANCE;

        private PISDatabase() { }

        public static PISDatabase Instance
        {
            get
            {
                if (INSTANCE == null)
                {
                    INSTANCE = new PISDatabase();
                    INSTANCE.Initialize(0);
                }
                return INSTANCE;
            }
        }

        public void Initialize(int index)
        {
            CONNECTION_STRING += "SERVER=" + ConnectionString.Servers[index] + ";";
            CONNECTION_STRING += "DATABASE=pis;";
            CONNECTION_STRING += "UID=" + ConnectionString.UIDs[index] + ";";
            CONNECTION_STRING += "PASSWORD=" + ConnectionString.Password[index] + ";";
            CONNECTION_STRING += "pooling = false; SslMode=none; convert zero datetime=True";
        }
        #endregion

        #region GET JOB ORDER BY DISBURSEMENT TYPE
        public async Task<List<Employee>> GetJobOrdersByDisbursementType(string disbursementType)
        {
            var query = new StringBuilder();
            query.AppendFormat(@"
            SELECT pi.userid, pi.fname, pi.mname, pi.lname, ds.description AS designation, dv.description AS division,
            sc.description AS section, pi.employee_status, pi.job_status, wx.monthly_salary,
            pi.disbursement_type
            FROM pis.personal_information pi
            LEFT JOIN dts.designation ds ON ds.id = pi.designation_id
            LEFT JOIN dts.division dv ON dv.id = pi.division_id
            LEFT JOIN dts.section sc ON sc.id = pi.section_id
            LEFT JOIN(SELECT* FROM pis.work_experience WHERE date_to = 'Present') AS wx ON wx.userid = pi.userid
            WHERE pi.employee_status = 1 
            AND pi.userid != 'hr_admin' 
            AND (pi.region IS NULL OR pi.region = 'region_7') 
            AND pi.job_status = 'Job Order' 
            AND pi.disbursement_type = '{0}';
            ", disbursementType);

            var jobOrders = new List<Employee>();

            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand(query.ToString(), connection))
                {
                    var reader = await command.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        jobOrders.Add(new Employee
                        {
                            ID = reader["userid"].ToString(),
                            Fname = reader["fname"].ToString(),
                            Mname = reader["mname"].ToString(),
                            Lname = reader["lname"].ToString(),
                            Salary = reader["monthly_salary"].ToString(),
                            Designation = reader["designation"].ToString(),
                            Division = reader["division"].ToString(),
                            Section = reader["section"].ToString()
                        });
                    }
                    await reader.CloseAsync();
                }
                await connection.CloseAsync();
            }

            return jobOrders;
        }
        #endregion

        #region GET REGULARS BY DISBURSEMENT TYPE
        public async Task<List<Employee>> GetRegularsByDisbursementType(string disbursementType)
        {
            var query = new StringBuilder();
            query.AppendFormat(@"
                SELECT pi.userid, pi.fname, pi.mname, pi.lname, ds.description AS designation, dv.description AS division, 
                sc.description AS section, pi.employee_status, pi.job_status, wx.monthly_salary, 
                pi.disbursement_type, IFNULL(rt.rata, 0) as cprata 
                FROM pis.personal_information pi 
                LEFT JOIN payroll.employee_cellphone_rata rt ON rt.userid = pi.userid 
                LEFT JOIN dts.designation ds ON ds.id = pi.designation_id 
                LEFT JOIN dts.division dv ON dv.id = pi.division_id 
                LEFT JOIN dts.section sc ON sc.id = pi.section_id 
                LEFT JOIN(SELECT* FROM pis.work_experience WHERE date_to = 'Present') AS wx ON wx.userid = pi.userid 
                WHERE pi.employee_status = 1 AND pi.job_status = 'Permanent' 
                AND pi.userid != 'hr_admin' 
                AND (pi.region IS NULL OR pi.region = 'region_7') 
                AND pi.disbursement_type = '{0}';
            ", disbursementType);

            var jobOrders = new List<Employee>();

            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand(query.ToString(), connection))
                {
                    var reader = await command.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        jobOrders.Add(new Employee
                        {
                            ID = reader["userid"].ToString(),
                            Fname = reader["fname"].ToString(),
                            Mname = reader["mname"].ToString(),
                            Lname = reader["lname"].ToString(),
                            Salary = reader["monthly_salary"].ToString(),
                            Designation = reader["designation"].ToString(),
                            Division = reader["division"].ToString(),
                            Section = reader["section"].ToString(),
                            CPRATA = reader["cprata"].ToString() == "1"
                        });
                    }
                    await reader.CloseAsync();
                }
                await connection.CloseAsync();
            }

            return jobOrders;
        }
        #endregion

        #region GET SALARY
        public async Task<string> GetSalary(string tranch, string grade, string step)
        {
            var query = "SELECT salary_amount as salary FROM pis.salary_grade WHERE salary_tranche = '" + tranch + "' AND salary_grade = '" + grade + "' AND salary_step = '" + step + "'";

            string salary = "0.00";

            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    var dataReader = await command.ExecuteReaderAsync();
                    while (dataReader.Read())
                    {
                        salary = dataReader["salary"].ToString();
                    }
                    await dataReader.CloseAsync();
                }
                await connection.CloseAsync();
            }
            return salary;
        }
        #endregion

        #region UPDATE EMPLOYEE SALARY
        public bool UpdateEmployeeSalary(string salary, string userid, string tranch, string grade, string step)
        {
            var salaryGrade = tranch + " | " + grade + "-" + step;
            string query = "UPDATE pis.work_experience SET monthly_salary = '" + salary + "', salary_grade = '" + salaryGrade + "' WHERE userid = '" + userid + "' AND date_to = 'Present'";

            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                connection.Open();
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }

            return true;
        }
        #endregion

        #region GET SALARY CHARGE BY JOB STATUS
        public async Task<SelectList> GetSalaryChargeAsync(string job_status)
        {
            var query = new StringBuilder();
            query.AppendFormat(@"
            SELECT DISTINCT(salary_charge) FROM pis.personal_information
            WHERE userid != 'hr_admin' 
            AND (region IS NULL OR region = 'region_7')
            AND job_status = '{0}'
            AND employee_status = 1
            AND salary_charge IS NOT NULL
            ORDER BY salary_charge
            ", job_status);

            var salaryCharge = new List<SelectListModel>();

            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                connection.Open();
                using (MySqlCommand command = new MySqlCommand(query.ToString(), connection))
                {
                    var reader = await command.ExecuteReaderAsync();
                    while(await reader.ReadAsync())
                    {
                        salaryCharge.Add(new SelectListModel
                        {
                            value = reader["salary_charge"].ToString(),
                            text = reader["salary_charge"].ToString()
                        });
                    }
                }
                connection.Close();
            }

            var selectList = new SelectList(salaryCharge.Where(x=> !string.IsNullOrEmpty(x.value) && x.value != "4"), "value", "text");

            return selectList;
        }
        #endregion

        #region GET LIST EMPLOYEES
        public async Task<List<Employee>> GetEmployeesAsync(string nameid)
        {
            var query = new StringBuilder();
            query.AppendFormat(@"
                SELECT pi.userid, pi.fname, pi.mname, pi.lname, rt.rata
                FROM pis.personal_information pi 
                LEFT JOIN payroll.employee_cellphone_rata rt ON rt.userid = pi.userid 
                WHERE pi.employee_status = 1 AND pi.job_status = 'Permanent' 
                AND pi.userid != 'hr_admin' 
                AND (pi.region IS NULL OR pi.region = 'region_7') 
                AND pi.disbursement_type = 'ATM'
                AND rt.rata IS NULL
                AND CONCAT(pi.fname,pi.mname,pi.lname) LIKE '%{0}%';
            ", nameid);

            var employees= new List<Employee>();

            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand(query.ToString(), connection))
                {
                    var reader = await command.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        employees.Add(new Employee
                        {
                            ID = reader["userid"].ToString(),
                            Fname = reader["fname"].ToString(),
                            Mname = reader["mname"].ToString(),
                            Lname = reader["lname"].ToString()
                        });
                    }
                    await reader.CloseAsync();
                }
                await connection.CloseAsync();
            }

            return employees;
        }
        #endregion
    }
}
