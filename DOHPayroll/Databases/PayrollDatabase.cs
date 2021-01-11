using DOHPayroll.Models;
using DOHPayroll.PayrollModels;
using DOHPayroll.Resources;
using DOHPayroll.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DOHPayroll.Databases
{
    public sealed class PayrollDatabase
    {
        #region INITIALIZERS
        public static string CONNECTION_STRING { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        private static PayrollDatabase INSTANCE;

        private PayrollDatabase() { }

        public static PayrollDatabase Instance
        {
            get
            {
                if (INSTANCE == null)
                {
                    INSTANCE = new PayrollDatabase();
                    INSTANCE.Initialize(1);
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
        #endregion

        #region DELETE PAYROLL BY ID

        public async Task<bool> DeleteRowByIdAsync(string id, string table)
        {
            string query = "DELETE FROM payroll." + table + " WHERE id = '" + id + "'";

            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
                await connection.CloseAsync();
            }

            return true;
        }

        #endregion

        #region SAVE JOB ORDER PAYROLLS
        public async Task<bool> SaveJoPayrollsAsync(List<JoPayrollModel> model, string date, string range)
        {
            GetStartDay(date, range);
            string query = "REPLACE INTO payroll.payroll VALUES ";
            var ctr = 0;
            foreach(var payroll in model)
            {
                ctr++;
                query += "('" + payroll.PayrollId + "','" + payroll.ID + "', '" + StartDate.ToString("yyyy-MM-dd") + "', '" + EndDate.ToString("yyyy-MM-dd") + "','" +
                    payroll.AbsentDays + "','" + payroll.WorkingDays + "','" + payroll.Salary + "','" + payroll.Adjustment + "','" + payroll.MinutesLate + "','" +
                    payroll.HWMPC.Item + "','" + payroll.PHIC.Item + "','" + payroll.Disallowance + "','" + payroll.GSIS.Item + "','" + payroll.Pagibig.Item + "','" + payroll.PagibigLoan.Item
                    + "','" + (string.IsNullOrEmpty(payroll.Remarks) ? "none" : payroll.Remarks) + "','" + DateTime.Now.ToString("yyyy-MM-dd") + "','" + DateTime.Now.ToString("yyyy-MM-dd") + "'," +
                    "'" + payroll.ProfessionalTax + "','" + payroll.OtherAdjustments + "')" + (ctr == model.Count() ? "" : ", ");
            }

            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    await command.ExecuteNonQueryAsync();
                    foreach(var payroll in model)
                    {
                        if (payroll.Pagibig.Item > 0)
                        {
                            await IncrementRemittance("pagibig_remittance", payroll.ID);
                        }
                        if (payroll.HWMPC.Item > 0)
                        {
                            await IncrementRemittance("coop_remittance", payroll.ID);
                        }
                        if (payroll.PHIC.Item > 0)
                        {
                            await IncrementRemittance("phic_remittance", payroll.ID);
                        }
                        if (payroll.Disallowance > 0)
                        {
                            await IncrementRemittance("disallowance_remittance", payroll.ID);
                        }
                        if (payroll.GSIS.Item > 0)
                        {
                            await IncrementRemittance("gsis_remittance", payroll.ID);
                        }
                        if (payroll.PagibigLoan.Item > 0)
                        {
                            await IncrementRemittance("pagibig_loan", payroll.ID);
                        }
                    }
                }
                await connection.CloseAsync();
            }
            return true;
        }
        #endregion

        #region SAVE REGULAR PAYROLLS
        public async Task<bool> SaveRegularPayrollsAsync(List<RegularPayrollModel> model)
        {
            string query = "REPLACE INTO payroll.regular_payroll VALUES";
            var ctr = 0;
            foreach(var payroll in model)
            {
                ctr++;
                query += "('" + payroll.PayrollId + "','" + payroll.ID + "', '" + payroll.Month + "', '" + payroll.Year + "','" +
                payroll.AbsentDays + "','" + payroll.WorkingDays + "','" + payroll.Salary + "','" + payroll.PERA + "','" + payroll.MinutesLate + "', '" + payroll.ProfessionalTax + "', '" +
                payroll.CFI.Item + "','" + payroll.GSIS_Premium + "','" + payroll.GSIS_Consoloan.Item + "','" + payroll.GSIS_PolicyLoan.Item + "','" + payroll.GSIS_EML.Item + "'," +
                "'" + payroll.GSIS_UOLI.Item + "','" + payroll.GSIS_EDU.Item + "', '" + payroll.GSIS_Help.Item + "', '" + payroll.GSIS_Rel.Item + "','" + payroll.PagibigPremium + "'," +
                "'" + payroll.PagibigLoan.Item + "', '" + payroll.Disallowances.Item + "', '" + payroll.Phic + "', '" + payroll.SIMC.Item + "', '" + payroll.HWMPC.Item + "', '" + payroll.DBP.Item + "'," +
                "'" + payroll.PagibigMP2.Item + "', '" + payroll.PagibigCalamity.Item + "', '" + payroll.GSIS_GFAL.Item + "', '" + payroll.GSIS_COMP.Item + "')" + (ctr == model.Count() ? "" : ", ");
            }
                

            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    await command.ExecuteNonQueryAsync();
                    /*if (model.GS.Item > 0)
                    {
                        await IncrementRemittance("pagibig_remittance", model.ID);
                    }
                    if (model.HWMPC.Item > 0)
                    {
                        await IncrementRemittance("coop_remittance", model.ID);
                    }
                    if (model.PHIC.Item > 0)
                    {
                        await IncrementRemittance("phic_remittance", model.ID);
                    }
                    if (model.Disallowance > 0)
                    {
                        await IncrementRemittance("disallowance_remittance", model.ID);
                    }
                    if (model.GSIS.Item > 0)
                    {
                        await IncrementRemittance("gsis_remittance", model.ID);
                    }
                    if (model.PagibigLoan.Item > 0)
                    {
                        await IncrementRemittance("pagibig_loan", model.ID);
                    }*/
                }
                await connection.CloseAsync();
            }
            return true;
        }
        #endregion

        #region SAVE SUBSISTENCES
        public async Task<bool> SaveSubsistencesAsync(List<SubsistenceModel> model)
        {
            var query = new StringBuilder();
            query.Append("REPLACE INTO payroll.subsistence VALUES");
            var ctr = 0;
            foreach (var sub in model)
            {
                ctr++;
                query.AppendFormat(@"
                ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}'){10}
                ", sub.SubsistenceId, sub.ID, sub.Subsistence, sub.Laundry, sub.LessTotal, sub.HWMPC,
                sub.NoDaysAbsent, sub.Remarks, sub.Month, sub.Year, (ctr == model.Count() ? "" : ", "));
            }


            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand(query.ToString(), connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
                await connection.CloseAsync();
            }
            return true;
        }
        #endregion

        #region UPDATE JOB ORDER PAYROLL
        public async Task<bool> UpdateJoPayrollAsync(JoPayrollModel model)
        {
            GetStartDay(model.Date, model.Range.ToString());
            string query = "REPLACE INTO payroll.payroll VALUES('" + model.PayrollId + "','" + model.ID + "', '" + StartDate.ToString("yyyy-MM-dd") + "', '" + EndDate.ToString("yyyy-MM-dd") + "','" +
                    model.AbsentDays + "','" + model.WorkingDays + "','" + model.Salary + "','" + model.Adjustment + "','" + model.MinutesLate + "','" +
                    model.HWMPC.Item + "','" + model.PHIC.Item + "','" + model.Disallowance + "','" + model.GSIS.Item + "','" + model.Pagibig.Item + "','" + model.PagibigLoan.Item
                    + "',@remarks,NULL,NULL,'" + model.ProfessionalTax + "','" + model.OtherAdjustments + "')";

            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@remarks", string.IsNullOrEmpty(model.Remarks)? "" : model.Remarks);
                    await command.ExecuteNonQueryAsync();
                    if (model.Pagibig.Item > 0)
                    {
                        await IncrementRemittance("pagibig_remittance", model.ID);
                    }
                    if (model.HWMPC.Item > 0)
                    {
                        await IncrementRemittance("coop_remittance", model.ID);
                    }
                    if (model.PHIC.Item > 0)
                    {
                        await IncrementRemittance("phic_remittance", model.ID);
                    }
                    if (model.Disallowance > 0)
                    {
                        await IncrementRemittance("disallowance_remittance", model.ID);
                    }
                    if (model.GSIS.Item > 0)
                    {
                        await IncrementRemittance("gsis_remittance", model.ID);
                    }
                    if (model.PagibigLoan.Item > 0)
                    {
                        await IncrementRemittance("pagibig_loan", model.ID);
                    }
                }
                await connection.CloseAsync();
            }
            return true;
        }
        #endregion

        #region UPDATE REGULAR PAYROLL
        public async Task<bool> UpdateRegularPayrollAsync(RegularPayrollModel model)
        {
            GetStartDay(model.Date);
            string query = "REPLACE INTO payroll.regular_payroll VALUES('" + model.PayrollId + "','" + model.ID + "', '" + model.Month + "', '" + model.Year + "','" +
                    model.AbsentDays + "','" + model.WorkingDays + "','" + model.Salary + "','" + model.PERA + "','" + model.MinutesLate + "', '" + model.ProfessionalTax + "', '" +
                    model.CFI.Item + "','" + model.GSIS_Premium + "','" + model.GSIS_Consoloan.Item + "','" + model.GSIS_PolicyLoan.Item + "','" + model.GSIS_EML.Item + "'," +
                    "'" + model.GSIS_UOLI.Item + "','" + model.GSIS_EDU.Item + "', '" + model.GSIS_Help.Item + "', '" + model.GSIS_Rel.Item + "','" + model.PagibigPremium + "'," +
                    "'" + model.PagibigLoan.Item + "', '" + model.Disallowances.Item + "', '" + model.Phic + "', '" + model.SIMC.Item + "', '" + model.HWMPC.Item + "', '" + model.DBP.Item + "'," +
                    "'" + model.PagibigMP2.Item + "', '" + model.PagibigCalamity.Item + "', '" + model.GSIS_GFAL.Item + "', '" + model.GSIS_COMP.Item + "');";

            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    await command.ExecuteNonQueryAsync();
                    /*if (model.GS.Item > 0)
                    {
                        await IncrementRemittance("pagibig_remittance", model.ID);
                    }
                    if (model.HWMPC.Item > 0)
                    {
                        await IncrementRemittance("coop_remittance", model.ID);
                    }
                    if (model.PHIC.Item > 0)
                    {
                        await IncrementRemittance("phic_remittance", model.ID);
                    }
                    if (model.Disallowance > 0)
                    {
                        await IncrementRemittance("disallowance_remittance", model.ID);
                    }
                    if (model.GSIS.Item > 0)
                    {
                        await IncrementRemittance("gsis_remittance", model.ID);
                    }
                    if (model.PagibigLoan.Item > 0)
                    {
                        await IncrementRemittance("pagibig_loan", model.ID);
                    }*/
                }
                await connection.CloseAsync();
            }
            return true;
        }
        #endregion

        #region INCREMENT REMITTANCE

        public async Task IncrementRemittance(string table, string id)
        {

            string query = "UPDATE payroll." + table + " SET count = (count + 1) WHERE userid = '" + id + "'";
            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
                await connection.CloseAsync();
            }
        }

        #endregion

        #region UPDATE REMITTANCE
        public async Task<bool> UpdateRemittanceAsync(string userid, string type, string value, string noPayments, string noPaid)
        {
            string query = "REPLACE INTO payroll." + type + "_remittance (userid, max, count, amount) VALUES('" + userid + "', '" + noPayments + "', '" + noPaid + "', '" + value + "')";
            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
                await connection.CloseAsync();
            }

            return true;
        }
        #endregion

        #region IF REGULAR PAYROLL EXISTS
        public int PayrollExists(string userid, int month, int year)
        {
            string query = "SELECT id FROM payroll.regular_payroll WHERE userid = '" + userid + "' " +
                "AND month = '" + month + "' AND year = '" + year + "' LIMIT 1";

            int id = 0;
            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                connection.Open();
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        id = int.Parse(reader["id"].ToString());
                    }
                    reader.Close();
                }
                connection.Close();
            }

            return id;

        }
        #endregion

        #region IF PAYROLL EXISTS
        public int PayrollExists(string userid, DateTime startDate, DateTime endDate)
        {
            string query = "SELECT id FROM payroll.payroll WHERE userid = '"+userid+"' " +
                "AND start_date = '"+startDate.ToString("yyyy-MM-dd")+"' AND end_date = '"+endDate.ToString("yyyy-MM-dd")+"' LIMIT 1";

            int id = 0;
            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                connection.Open();
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    var reader = command.ExecuteReader();
                    while(reader.Read())
                    {
                        id = int.Parse(reader["id"].ToString());
                    }
                    reader.Close();
                }
                connection.Close();
            }

            return id;

        }
        #endregion

        #region GET JOB ORDER PAYROLL BY ID V2
        public async Task<JoPayrollModel> GetJoPayrollByID(string userid, string fname, string mname, string lname, string salary, DateTime startDate, DateTime endDate)
        {
            string query = "SELECT p.id, p.userid, p.start_date, p.end_date, p.absent_days, p.working_days, p.adjustment, p.minutes_late, " +
                "p.coop, ifnull(cr.max, 0) as coop_max, ifnull(cr.count, 0) as coop_ctr,  " +
                "p.pagibig, ifnull(pr.max, 0) as pagibig_max, ifnull(pr.count, 0) as pagibig_ctr, " +
                "p.gsis, ifnull(gr.max, 0) as gsis_max, ifnull(gr.count, 0) as gsis_ctr, " +
                "p.phic, ifnull(phr.max, 0) as phic_max, ifnull(phr.count, 0) as phic_ctr, " +
                "p.pagibig_loan, ifnull(plr.max, 0) as pagibig_loan_max, ifnull(plr.count, 0) as pagibig_loan_ctr, " +
                "p.tax, p.disallowance, p.remarks " +
                "FROM payroll.payroll p " +
                "LEFT JOIN payroll.coop_remittance cr ON cr.userid = p.userid " +
                "LEFT JOIN payroll.pagibig_remittance pr ON pr.userid = p.userid " +
                "LEFT JOIN payroll.gsis_remittance gr ON gr.userid = p.userid " +
                "LEFT JOIN payroll.phic_remittance phr ON phr.userid = p.userid " +
                "LEFT JOIN payroll.pagibig_loan_remittance plr ON plr.userid = p.userid " +
                "WHERE p.userid = '" + userid + "' ORDER BY p.start_date DESC LIMIT 1;";

            var (min, absences, rendered) = await DTRDatabase.Instance.GetMinsV2Async(userid, startDate, endDate, "jo");
            var payroll = new JoPayrollModel
            {
                Fname = fname,
                Mname = mname,
                Lname = lname,
                ID = userid,
                Date = startDate.ToString("MMMM yyyy"),
                Range = startDate.Day == 1 ? 1 : 2,
                AbsentDays = absences,
                WorkingDays = DTRDatabase.Instance.GetWorkingDays(startDate),
                Salary = salary,
                MinutesLate = min,
                PayrollId = PayrollExists(userid, startDate, endDate),
                Adjustment = 0,
                HWMPC = new LoanModel
                {
                    Item = 0,
                    NoPayment = 0,
                    NoPaid = 0
                },
                Pagibig = new LoanModel
                {
                    Item = 0,
                    NoPayment = 0,
                    NoPaid = 0
                },
                GSIS = new LoanModel
                {
                    Item = 0,
                    NoPayment = 0,
                    NoPaid = 0
                },
                PHIC = new LoanModel
                {
                    Item = 0,
                    NoPayment = 0,
                    NoPaid = 0
                },
                PagibigLoan = new LoanModel
                {
                    Item = 0,
                    NoPayment = 0,
                    NoPaid = 0
                },
                ProfessionalTax = 0,
                Disallowance = 0,
                Remarks = "",
                EWT = 0
        };

            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    var reader = await command.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        payroll.Adjustment = double.Parse(reader["adjustment"].ToString());
                        payroll.HWMPC = new LoanModel
                        {
                            Item = double.Parse(reader["coop"].ToString()),
                            NoPayment = int.Parse(reader["coop_max"].ToString()),
                            NoPaid = int.Parse(reader["coop_ctr"].ToString())
                        };
                        payroll.Pagibig = new LoanModel
                        {
                            Item = double.Parse(reader["pagibig"].ToString()),
                            NoPayment = int.Parse(reader["pagibig_max"].ToString()),
                            NoPaid = int.Parse(reader["pagibig_ctr"].ToString())
                        };
                        payroll.GSIS = new LoanModel
                        {
                            Item = double.Parse(reader["gsis"].ToString()),
                            NoPayment = int.Parse(reader["gsis_max"].ToString()),
                            NoPaid = int.Parse(reader["gsis_ctr"].ToString())
                        };
                        payroll.PHIC = new LoanModel
                        {
                            Item = double.Parse(reader["phic"].ToString()),
                            NoPayment = int.Parse(reader["phic_max"].ToString()),
                            NoPaid = int.Parse(reader["phic_ctr"].ToString())
                        };
                        payroll.PagibigLoan = new LoanModel
                        {
                            Item = double.Parse(reader["pagibig_loan"].ToString()),
                            NoPayment = int.Parse(reader["pagibig_loan_max"].ToString()),
                            NoPaid = int.Parse(reader["pagibig_loan_ctr"].ToString())
                        };
                        payroll.ProfessionalTax = double.Parse(reader["tax"].ToString());
                        payroll.Disallowance = double.Parse(reader["disallowance"].ToString());
                        payroll.Remarks = "";
                        payroll.EWT = 0;
                    }
                    await reader.CloseAsync();
                }
                await connection.CloseAsync();
            }

            return payroll;
        }
        #endregion

        #region GET JOB ORDER PAYROLL BY ID
        public async Task<JoPayrollModel> GetJoPayrollByID(string userid, string fname, string mname, string lname, string salary, int? payrollId)
        {
            string query = "SELECT p.id, p.userid, p.start_date, p.end_date, p.absent_days, p.working_days, p.adjustment, p.minutes_late, " +
                "p.coop, ifnull(cr.max, 0) as coop_max, ifnull(cr.count, 0) as coop_ctr, we.position_title, " +
                "p.pagibig, ifnull(pr.max, 0) as pagibig_max, ifnull(pr.count, 0) as pagibig_ctr, " +
                "p.gsis, ifnull(gr.max, 0) as gsis_max, ifnull(gr.count, 0) as gsis_ctr, " +
                "p.phic, ifnull(phr.max, 0) as phic_max, ifnull(phr.count, 0) as phic_ctr, " +
                "p.pagibig_loan, ifnull(plr.max, 0) as pagibig_loan_max, ifnull(plr.count, 0) as pagibig_loan_ctr, " +
                "p.tax, p.disallowance, p.remarks " +
                "FROM payroll.payroll p " +
                "LEFT JOIN pis.work_experience we ON we.userid = p.userid " +
                "LEFT JOIN payroll.coop_remittance cr ON cr.userid = p.userid " +
                "LEFT JOIN payroll.pagibig_remittance pr ON pr.userid = p.userid " +
                "LEFT JOIN payroll.gsis_remittance gr ON gr.userid = p.userid " +
                "LEFT JOIN payroll.phic_remittance phr ON phr.userid = p.userid " +
                "LEFT JOIN payroll.pagibig_loan_remittance plr ON plr.userid = p.userid ";

            if (payrollId == null)
                query += "WHERE p.userid = '" + userid + "' ORDER BY p.start_date DESC LIMIT 1;";
            else
                query += "WHERE p.id = '" + payrollId.Value + "';";


            var option = DateTime.Now.Day <= 15;
            GetStartDay(DateTime.Now.Month, DateTime.Now.Year, option);
            var (min, absences, rendered) = await DTRDatabase.Instance.GetMinsV2Async(userid, StartDate, EndDate, "jo");
            var payroll = new JoPayrollModel
            {
                Fname = fname,
                Mname = mname,
                Lname = lname,
                ID = userid,
                Date = DateTime.Now.ToString("MMMM yyyy"),
                Range = option ? 1 : 2,
                AbsentDays = absences,
                WorkingDays = DTRDatabase.Instance.GetWorkingDays(DateTime.Now),
                Salary = salary,
                Adjustment = 0,
                MinutesLate = min,
                PayrollId = 0,
                HWMPC = new LoanModel
                {
                    Item = 0,
                    NoPayment = 0,
                    NoPaid = 0,
                },
                Pagibig = new LoanModel
                {
                    Item = 0,
                    NoPayment = 0,
                    NoPaid = 0,
                },
                GSIS = new LoanModel
                {
                    Item = 0,
                    NoPayment = 0,
                    NoPaid = 0,
                },
                PHIC = new LoanModel
                {
                    Item = 0,
                    NoPayment = 0,
                    NoPaid = 0,
                },
                PagibigLoan = new LoanModel
                {
                    Item = 0,
                    NoPayment = 0,
                    NoPaid = 0,
                },
                ProfessionalTax = 0,
                Disallowance = 0,
                Remarks = "",
                EWT = 0
            };

            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var startDate = payrollId == null? DateTime.Now : DateTime.Parse(reader["start_date"].ToString());
                        payroll.Date = startDate.ToString("MMMM yyyy");
                        payroll.Range = startDate.Day <= 15 ? 1 : 2;
                        payroll.WorkingDays = DTRDatabase.Instance.GetWorkingDays(startDate);
                        payroll.Fname = fname;
                        payroll.Mname = mname;
                        payroll.Lname = lname;
                        payroll.Designation = reader["position_title"].ToString();
                        payroll.PayrollId = payrollId == null ? 0 : int.Parse(reader["id"].ToString());
                        payroll.ID = userid;
                        payroll.AbsentDays = payrollId == null ? absences : reader["absent_days"].ToString();
                        payroll.Salary = salary;
                        payroll.Adjustment = double.Parse(reader["adjustment"].ToString());
                        payroll.MinutesLate = payrollId == null ? min : int.Parse(reader["minutes_late"].ToString());
                        payroll.HWMPC = new LoanModel
                        {
                            Item = double.Parse(reader["coop"].ToString()),
                            NoPayment = int.Parse(reader["coop_max"].ToString()),
                            NoPaid = int.Parse(reader["coop_ctr"].ToString())
                        };
                        payroll.Pagibig = new LoanModel
                        {
                            Item = double.Parse(reader["pagibig"].ToString()),
                            NoPayment = int.Parse(reader["pagibig_max"].ToString()),
                            NoPaid = int.Parse(reader["pagibig_ctr"].ToString())
                        };
                        payroll.GSIS = new LoanModel
                        {
                            Item = double.Parse(reader["gsis"].ToString()),
                            NoPayment = int.Parse(reader["gsis_max"].ToString()),
                            NoPaid = int.Parse(reader["gsis_ctr"].ToString())
                        };
                        payroll.PHIC = new LoanModel
                        {
                            Item = double.Parse(reader["phic"].ToString()),
                            NoPayment = int.Parse(reader["phic_max"].ToString()),
                            NoPaid = int.Parse(reader["phic_ctr"].ToString())
                        };
                        payroll.PagibigLoan = new LoanModel
                        {
                            Item = double.Parse(reader["pagibig_loan"].ToString()),
                            NoPayment = int.Parse(reader["pagibig_loan_max"].ToString()),
                            NoPaid = int.Parse(reader["pagibig_loan_ctr"].ToString())
                        };
                        payroll.ProfessionalTax = double.Parse(reader["tax"].ToString());
                        payroll.Disallowance = double.Parse(reader["disallowance"].ToString());
                        payroll.Remarks = "";
                        payroll.EWT = 0;
                    }
                    await reader.CloseAsync();
                }
                await connection.CloseAsync();
            }

            return payroll;
        }
        #endregion

        #region GET JOB ORDER PAYROLL BY DATE
        public async Task<List<JoPayrollModel>> GetJoPayrollByDate(string date, int range, string disbursement, string salary_charge)
        {
            GetStartDay(date, range.ToString());
            var query = new StringBuilder();
            query.AppendFormat(@"
                SELECT p.id, p.userid, p.start_date, p.end_date, p.absent_days, p.working_days, p.adjustment, p.minutes_late,
                pi.fname, pi.mname, pi.lname, pi.designation, pi.section, pi.division, pi.disbursement_type, pi.tin_no, pi.salary_charge,
                p.coop, ifnull(cr.max, 0) as coop_max, ifnull(cr.count, 0) as coop_ctr,
                we.position_title, we.monthly_salary,
                p.pagibig, ifnull(pr.max, 0) as pagibig_max, ifnull(pr.count, 0) as pagibig_ctr,
                p.gsis, ifnull(gr.max, 0) as gsis_max, ifnull(gr.count, 0) as gsis_ctr,
                p.phic, ifnull(phr.max, 0) as phic_max, ifnull(phr.count, 0) as phic_ctr,
                p.pagibig_loan, ifnull(plr.max, 0) as pagibig_loan_max, ifnull(plr.count, 0) as pagibig_loan_ctr,
                p.tax, p.disallowance, p.remarks
                FROM payroll.payroll p
                LEFT JOIN (
                   SELECT pis.userid, pis.fname, pis.mname, pis.lname, pis.tin_no, pis.disbursement_type,
                   des.description AS designation, pis.salary_charge,
                   divi.description AS division, sec.description AS section FROM pis.personal_information pis
                   LEFT JOIN dts.designation des ON des.id = pis.designation_id
                   LEFT JOIN dts.division divi ON divi.id = pis.division_id
                   LEFT JOIN dts.section sec ON sec.id = pis.section_id
                ) pi ON pi.userid = p.userid
                LEFT JOIN (SELECT * FROM pis.work_experience WHERE date_to = 'Present') we ON we.userid = p.userid
                LEFT JOIN payroll.coop_remittance cr ON cr.userid = p.userid
                LEFT JOIN payroll.pagibig_remittance pr ON pr.userid = p.userid
                LEFT JOIN payroll.gsis_remittance gr ON gr.userid = p.userid
                LEFT JOIN payroll.phic_remittance phr ON phr.userid = p.userid
                LEFT JOIN payroll.pagibig_loan_remittance plr ON plr.userid = p.userid
                WHERE pi.disbursement_type = '{0}'
                AND p.start_date = '{1}'
                AND p.end_date = '{2}'
            ", disbursement, StartDate.ToString("yyyy-MM-dd"), EndDate.ToString("yyyy-MM-dd"));

            if(!string.IsNullOrEmpty(salary_charge) && salary_charge != "ALL")
            {
                query.AppendFormat(@" AND pi.salary_charge = '{0}'", salary_charge);
            }


            var payrolls = new List<JoPayrollModel>();

            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand(query.ToString(), connection))
                {
                    var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        payrolls.Add(new JoPayrollModel
                        {
                            Fname = reader["fname"].ToString(),
                            Mname = reader["mname"].ToString(),
                            Lname = reader["lname"].ToString(),
                            Designation = reader["designation"].ToString(),
                            Date = date,
                            Range = range,
                            Section = reader["section"].ToString(),
                            Division = reader["division"].ToString(),
                            DisbursementType = reader["disbursement_type"].ToString(),
                            Tin = reader["tin_no"].ToString(),
                            PayrollId = int.Parse(reader["id"].ToString()),
                            ID = reader["userid"].ToString(),
                            AbsentDays = reader["absent_days"].ToString(),
                            WorkingDays = int.Parse(reader["working_days"].ToString()),
                            Salary = reader["monthly_salary"].ToString(),
                            SalaryCharge = string.IsNullOrEmpty(reader["salary_charge"].ToString())? "NO SALARY CHARGE" : reader["salary_charge"].ToString(),
                            Adjustment = double.Parse(reader["adjustment"].ToString()),
                            MinutesLate = int.Parse(reader["minutes_late"].ToString()),
                            HWMPC = new LoanModel
                            {
                                Item = double.Parse(reader["coop"].ToString()),
                                NoPayment = int.Parse(reader["coop_max"].ToString()),
                                NoPaid = int.Parse(reader["coop_ctr"].ToString())
                            },
                            Pagibig = new LoanModel
                            {
                                Item = double.Parse(reader["pagibig"].ToString()),
                                NoPayment = int.Parse(reader["pagibig_max"].ToString()),
                                NoPaid = int.Parse(reader["pagibig_ctr"].ToString())
                            },
                            GSIS = new LoanModel
                            {
                                Item = double.Parse(reader["gsis"].ToString()),
                                NoPayment = int.Parse(reader["gsis_max"].ToString()),
                                NoPaid = int.Parse(reader["gsis_ctr"].ToString())
                            },
                            PHIC = new LoanModel
                            {
                                Item = double.Parse(reader["phic"].ToString()),
                                NoPayment = int.Parse(reader["phic_max"].ToString()),
                                NoPaid = int.Parse(reader["phic_ctr"].ToString())
                            },
                            PagibigLoan = new LoanModel
                            {
                                Item = double.Parse(reader["pagibig_loan"].ToString()),
                                NoPayment = int.Parse(reader["pagibig_loan_max"].ToString()),
                                NoPaid = int.Parse(reader["pagibig_loan_ctr"].ToString())
                            },
                            ProfessionalTax = double.Parse(reader["tax"].ToString()),
                            Disallowance = double.Parse(reader["disallowance"].ToString()),
                            Remarks = reader["remarks"].ToString(),
                            EWT = 0
                        });
                    }
                    await reader.CloseAsync();
                }
                await connection.CloseAsync();
            }

            return payrolls;
        }
        #endregion

        #region GET REGULAR PAYROLL BY ID V2
        public async Task<RegularPayrollModel> GetRegularPayrollByID(string userid, string fname, string mname, string lname, string salary, string date)
        {
            string query = "SELECT p.id, p.userid, p.month, p.year, p.absent_days, p.working_days, p.month_salary, p.pera, p.minutes_late, p.tax, " +
                "p.cfi, ifnull(cfir.max, 0) as cfi_max, ifnull(cfir.count, 0) as cfi_ctr, " +
                "p.gsis_premium, " +
                "p.gsis_comp, ifnull(comp.max, 0) as gsis_comp_max, ifnull(comp.count, 0) as gsis_comp_ctr,   " +
                "p.gsis_gfal, ifnull(gfal.max, 0) as gsis_gfal_max, ifnull(gfal.count, 0) as gsis_gfal_ctr,   " +
                "p.gsis_consoloan, ifnull(gcr.max, 0) as gsis_consoloan_max, ifnull(gcr.count, 0) as gsis_consoloan_ctr,   " +
                "p.gsis_policy_loan, ifnull(gpr.max, 0) as gsis_policy_loan_max, ifnull(gpr.count, 0) as gsis_policy_loan_ctr,   " +
                "p.gsis_eml, ifnull(ger.max, 0) as gsis_eml_max, ifnull(ger.count, 0) as gsis_eml_ctr,   " +
                "p.gsis_uoli, ifnull(gur.max, 0) as gsis_uoli_max, ifnull(gur.count, 0) as gsis_uoli_ctr,   " +
                "p.gsis_edu, ifnull(gdr.max, 0) as gsis_edu_max, ifnull(gdr.count, 0) as gsis_edu_ctr,   " +
                "p.gsis_help, ifnull(ghr.max, 0) as gsis_help_max, ifnull(ghr.count, 0) as gsis_help_ctr,   " +
                "p.gsis_rel, ifnull(rr.max, 0) as gsis_rel_max, ifnull(rr.count, 0) as gsis_rel_ctr,   " +
                "p.pagibig_premium, we.position_title,  " +
                "p.pagibig_loan, ifnull(plr.max, 0) as pagibig_loan_max, ifnull(plr.count, 0) as pagibig_loan_ctr,  " +
                "p.pagibig_mp2, ifnull(pmr.max, 0) as pagibig_mp2_max, ifnull(pmr.count, 0) as pagibig_mp2_ctr,   " +
                "p.pagibig_calamity, ifnull(pcr.max, 0) as pagibig_calamity_max, ifnull(pcr.count, 0) as pagibig_calamity_ctr,   " +
                "p.philhealth,  " +
                "p.simc, ifnull(sr.max, 0) as simc_max, ifnull(sr.count, 0) as simc_ctr,   " +
                "p.hwmpc, ifnull(cr.max, 0) as hwmpc_max, ifnull(cr.count, 0) as hwmpc_ctr,   " +
                "p.dbp, ifnull(dr.max, 0) as dbp_max, ifnull(dr.count, 0) as dbp_ctr, " +
                "p.disallowances, ifnull(dsr.max, 0) as disallowances_max, ifnull(dsr.count, 0) as disallowances_ctr " +
                "FROM payroll.regular_payroll p " +
                "LEFT JOIN pis.work_experience we ON we.userid = p.userid " +
                "LEFT JOIN payroll.cfi_remittance cfir ON cfir.userid = p.userid " +
                "LEFT JOIN payroll.gsis_comp_remittance comp ON comp.userid = p.userid " +
                "LEFT JOIN payroll.gsis_gfal_remittance gfal ON gfal.userid = p.userid " +
                "LEFT JOIN payroll.gsis_consoloan_remittance gcr ON gcr.userid = p.userid " +
                "LEFT JOIN payroll.gsis_policyloan_remittance gpr ON gpr.userid = p.userid " +
                "LEFT JOIN payroll.gsis_eml_remittance ger ON ger.userid = p.userid " +
                "LEFT JOIN payroll.gsis_uoli_remittance gur ON gur.userid = p.userid " +
                "LEFT JOIN payroll.gsis_edu_remittance gdr ON gdr.userid = p.userid " +
                "LEFT JOIN payroll.gsis_help_remittance ghr ON ghr.userid = p.userid " +
                "LEFT JOIN payroll.rel_remittance rr ON rr.userid = p.userid " +
                "LEFT JOIN payroll.pagibig_loan_remittance plr ON plr.userid = p.userid " +
                "LEFT JOIN payroll.pagibig_mp2_remittance pmr ON pmr.userid = p.userid " +
                "LEFT JOIN payroll.pagibig_calamity_remittance pcr ON pcr.userid = p.userid " +
                "LEFT JOIN payroll.simc_remittance sr ON sr.userid = p.userid " +
                "LEFT JOIN payroll.coop_remittance cr ON cr.userid = p.userid " +
                "LEFT JOIN payroll.dbp_remittance dr ON dr.userid = p.userid " +
                "LEFT JOIN payroll.disallowance_remittance dsr ON dsr.userid = p.userid " +
                "WHERE p.userid = '" + userid + "' ORDER BY p.year DESC, p.month DESC LIMIT 1;";

            GetStartDay(date);
            var (min, absences, rendered) = await DTRDatabase.Instance.GetMinsV2Async(userid, StartDate, EndDate, "regular");
            var payroll = new RegularPayrollModel
            {
                Fname = fname,
                Mname = mname,
                Lname = lname,
                PayrollId = PayrollExists(userid, StartDate.Month, StartDate.Year),
                ID = userid,
                Date = date,
                AbsentDays = absences,
                WorkingDays = DTRDatabase.Instance.GetWorkingDays(StartDate),
                Salary = salary,
                PERA = 0,
                MinutesLate = min,
                ProfessionalTax = 0,
                GSIS_COMP = new LoanModel
                {
                    Item = 0,
                    NoPayment = 0,
                    NoPaid = 0
                },
                GSIS_GFAL = new LoanModel
                {
                    Item = 0,
                    NoPayment = 0,
                    NoPaid = 0
                },
                CFI = new LoanModel
                {
                    Item = 0,
                    NoPayment = 0,
                    NoPaid = 0
                },
                GSIS_Consoloan = new LoanModel
                {
                    Item = 0,
                    NoPayment = 0,
                    NoPaid = 0
                },
                GSIS_PolicyLoan = new LoanModel
                {
                    Item = 0,
                    NoPayment = 0,
                    NoPaid = 0
                },
                GSIS_EML = new LoanModel
                {
                    Item = 0,
                    NoPayment = 0,
                    NoPaid = 0
                },
                GSIS_UOLI = new LoanModel
                {
                    Item = 0,
                    NoPayment = 0,
                    NoPaid = 0
                },
                GSIS_EDU = new LoanModel
                {
                    Item = 0,
                    NoPayment = 0,
                    NoPaid = 0
                },
                GSIS_Help = new LoanModel
                {
                    Item = 0,
                    NoPayment = 0,
                    NoPaid = 0
                },
                GSIS_Rel = new LoanModel
                {
                    Item = 0,
                    NoPayment = 0,
                    NoPaid = 0
                },
                PagibigPremium = 0,
                PagibigLoan = new LoanModel
                {
                    Item = 0,
                    NoPayment = 0,
                    NoPaid = 0
                },
                PagibigMP2 = new LoanModel
                {
                    Item = 0,
                    NoPayment = 0,
                    NoPaid = 0
                },
                PagibigCalamity = new LoanModel
                {
                    Item = 0,
                    NoPayment = 0,
                    NoPaid = 0
                },
                SIMC = new LoanModel
                {
                    Item = 0,
                    NoPayment = 0,
                    NoPaid = 0
                },
                HWMPC = new LoanModel
                {
                    Item = 0,
                    NoPayment = 0,
                    NoPaid = 0
                },
                DBP = new LoanModel
                {
                    Item = 0,
                    NoPayment = 0,
                    NoPaid = 0
                },
                Disallowances = new LoanModel
                {
                    Item = 0,
                    NoPayment = 0,
                    NoPaid = 0
                }
            };

            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        payroll.Designation = reader["position_title"].ToString();
                        payroll.ID = userid;
                        payroll.PERA = double.Parse(reader["pera"].ToString());
                        payroll.ProfessionalTax = double.Parse(reader["tax"].ToString());
                        payroll.CFI = new LoanModel
                        {
                            Item = double.Parse(reader["cfi"].ToString()),
                            NoPayment = int.Parse(reader["cfi_max"].ToString()),
                            NoPaid = int.Parse(reader["cfi_ctr"].ToString())
                        };
                        payroll.GSIS_Consoloan = new LoanModel
                        {
                            Item = double.Parse(reader["gsis_consoloan"].ToString()),
                            NoPayment = int.Parse(reader["gsis_consoloan_max"].ToString()),
                            NoPaid = int.Parse(reader["gsis_consoloan_ctr"].ToString())
                        };
                        payroll.GSIS_PolicyLoan = new LoanModel
                        {
                            Item = double.Parse(reader["gsis_policy_loan"].ToString()),
                            NoPayment = int.Parse(reader["gsis_policy_loan_max"].ToString()),
                            NoPaid = int.Parse(reader["gsis_policy_loan_ctr"].ToString())
                        };
                        payroll.GSIS_EML = new LoanModel
                        {
                            Item = double.Parse(reader["gsis_eml"].ToString()),
                            NoPayment = int.Parse(reader["gsis_eml_max"].ToString()),
                            NoPaid = int.Parse(reader["gsis_eml_ctr"].ToString())
                        };
                        payroll.GSIS_UOLI = new LoanModel
                        {
                            Item = double.Parse(reader["gsis_uoli"].ToString()),
                            NoPayment = int.Parse(reader["gsis_uoli_max"].ToString()),
                            NoPaid = int.Parse(reader["gsis_uoli_ctr"].ToString())
                        };
                        payroll.GSIS_EDU = new LoanModel
                        {
                            Item = double.Parse(reader["gsis_edu"].ToString()),
                            NoPayment = int.Parse(reader["gsis_edu_max"].ToString()),
                            NoPaid = int.Parse(reader["gsis_edu_ctr"].ToString())
                        };
                        payroll.GSIS_Help = new LoanModel
                        {
                            Item = double.Parse(reader["gsis_help"].ToString()),
                            NoPayment = int.Parse(reader["gsis_help_max"].ToString()),
                            NoPaid = int.Parse(reader["gsis_help_ctr"].ToString())
                        };
                        payroll.GSIS_Rel = new LoanModel
                        {
                            Item = double.Parse(reader["gsis_rel"].ToString()),
                            NoPayment = int.Parse(reader["gsis_rel_max"].ToString()),
                            NoPaid = int.Parse(reader["gsis_rel_ctr"].ToString())
                        };
                        payroll.PagibigPremium = double.Parse(reader["pagibig_premium"].ToString());
                        payroll.PagibigLoan = new LoanModel
                        {
                            Item = double.Parse(reader["pagibig_loan"].ToString()),
                            NoPayment = int.Parse(reader["pagibig_loan_max"].ToString()),
                            NoPaid = int.Parse(reader["pagibig_loan_ctr"].ToString())
                        };
                        payroll.PagibigMP2 = new LoanModel
                        {
                            Item = double.Parse(reader["pagibig_mp2"].ToString()),
                            NoPayment = int.Parse(reader["pagibig_mp2_max"].ToString()),
                            NoPaid = int.Parse(reader["pagibig_mp2_ctr"].ToString())
                        };
                        payroll.PagibigCalamity = new LoanModel
                        {
                            Item = double.Parse(reader["pagibig_calamity"].ToString()),
                            NoPayment = int.Parse(reader["pagibig_calamity_max"].ToString()),
                            NoPaid = int.Parse(reader["pagibig_calamity_ctr"].ToString())
                        };
                        payroll.SIMC = new LoanModel
                        {
                            Item = double.Parse(reader["simc"].ToString()),
                            NoPayment = int.Parse(reader["simc_max"].ToString()),
                            NoPaid = int.Parse(reader["simc_ctr"].ToString())
                        };
                        payroll.HWMPC = new LoanModel
                        {
                            Item = double.Parse(reader["hwmpc"].ToString()),
                            NoPayment = int.Parse(reader["hwmpc_max"].ToString()),
                            NoPaid = int.Parse(reader["hwmpc_ctr"].ToString())
                        };
                        payroll.DBP = new LoanModel
                        {
                            Item = double.Parse(reader["dbp"].ToString()),
                            NoPayment = int.Parse(reader["dbp_max"].ToString()),
                            NoPaid = int.Parse(reader["dbp_ctr"].ToString())
                        };
                        payroll.Disallowances = new LoanModel
                        {
                            Item = double.Parse(reader["disallowances"].ToString()),
                            NoPayment = int.Parse(reader["disallowances_max"].ToString()),
                            NoPaid = int.Parse(reader["disallowances_ctr"].ToString())
                        };
                    }
                    await reader.CloseAsync();
                }
                await connection.CloseAsync();
            }

            return payroll;
        }
        #endregion

        #region GET REGULAR PAYROLL BY ID
        public async Task<RegularPayrollModel> GetRegularPayrollByID(string userid, string fname, string mname, string lname, string salary, int? payrollId)
        {
            var query = new StringBuilder();
            query.AppendFormat(@"
                SELECT p.id, p.userid, p.month, p.year, p.absent_days, p.working_days, p.month_salary, p.pera, p.minutes_late, p.tax,
                p.cfi, ifnull(cfir.max, 0) as cfi_max, ifnull(cfir.count, 0) as cfi_ctr,
                p.gsis_premium,
                p.gsis_gfal, ifnull(gfal.max, 0) as gsis_gfal_max, ifnull(gfal.count, 0) as gsis_gfal_ctr,  
                p.gsis_comp, ifnull(comp.max, 0) as gsis_comp_max, ifnull(comp.count, 0) as gsis_comp_ctr,  
                p.gsis_consoloan, ifnull(gcr.max, 0) as gsis_consoloan_max, ifnull(gcr.count, 0) as gsis_consoloan_ctr,  
                p.gsis_policy_loan, ifnull(gpr.max, 0) as gsis_policy_loan_max, ifnull(gpr.count, 0) as gsis_policy_loan_ctr,  
                p.gsis_eml, ifnull(ger.max, 0) as gsis_eml_max, ifnull(ger.count, 0) as gsis_eml_ctr,  
                p.gsis_uoli, ifnull(gur.max, 0) as gsis_uoli_max, ifnull(gur.count, 0) as gsis_uoli_ctr,  
                p.gsis_edu, ifnull(gdr.max, 0) as gsis_edu_max, ifnull(gdr.count, 0) as gsis_edu_ctr,  
                p.gsis_help, ifnull(ghr.max, 0) as gsis_help_max, ifnull(ghr.count, 0) as gsis_help_ctr,  
                p.gsis_rel, ifnull(rr.max, 0) as gsis_rel_max, ifnull(rr.count, 0) as gsis_rel_ctr,  
                p.pagibig_premium, we.position_title, 
                p.pagibig_loan, ifnull(plr.max, 0) as pagibig_loan_max, ifnull(plr.count, 0) as pagibig_loan_ctr, 
                p.pagibig_mp2, ifnull(pmr.max, 0) as pagibig_mp2_max, ifnull(pmr.count, 0) as pagibig_mp2_ctr,  
                p.pagibig_calamity, ifnull(pcr.max, 0) as pagibig_calamity_max, ifnull(pcr.count, 0) as pagibig_calamity_ctr,  
                p.philhealth, 
                p.simc, ifnull(sr.max, 0) as simc_max, ifnull(sr.count, 0) as simc_ctr,  
                p.hwmpc, ifnull(cr.max, 0) as hwmpc_max, ifnull(cr.count, 0) as hwmpc_ctr,  
                p.dbp, ifnull(dr.max, 0) as dbp_max, ifnull(dr.count, 0) as dbp_ctr,
                p.disallowances, ifnull(dsr.max, 0) as disallowances_max, ifnull(dsr.count, 0) as disallowances_ctr
                FROM payroll.regular_payroll p
                LEFT JOIN pis.work_experience we ON we.userid = p.userid
                LEFT JOIN payroll.cfi_remittance cfir ON cfir.userid = p.userid
                LEFT JOIN payroll.gsis_gfal_remittance gfal ON gfal.userid = p.userid
                LEFT JOIN payroll.gsis_comp_remittance comp ON comp.userid = p.userid
                LEFT JOIN payroll.gsis_consoloan_remittance gcr ON gcr.userid = p.userid
                LEFT JOIN payroll.gsis_policyloan_remittance gpr ON gpr.userid = p.userid
                LEFT JOIN payroll.gsis_eml_remittance ger ON ger.userid = p.userid
                LEFT JOIN payroll.gsis_uoli_remittance gur ON gur.userid = p.userid
                LEFT JOIN payroll.gsis_edu_remittance gdr ON gdr.userid = p.userid
                LEFT JOIN payroll.gsis_help_remittance ghr ON ghr.userid = p.userid
                LEFT JOIN payroll.rel_remittance rr ON rr.userid = p.userid
                LEFT JOIN payroll.pagibig_loan_remittance plr ON plr.userid = p.userid
                LEFT JOIN payroll.pagibig_mp2_remittance pmr ON pmr.userid = p.userid
                LEFT JOIN payroll.pagibig_calamity_remittance pcr ON pcr.userid = p.userid
                LEFT JOIN payroll.simc_remittance sr ON sr.userid = p.userid
                LEFT JOIN payroll.coop_remittance cr ON cr.userid = p.userid
                LEFT JOIN payroll.dbp_remittance dr ON dr.userid = p.userid
                LEFT JOIN payroll.disallowance_remittance dsr ON dsr.userid = p.userid
                ");

            if (payrollId == null)
                query.AppendFormat(@"WHERE p.userid = '{0}' ORDER BY p.year DESC, p.month DESC LIMIT 1", userid);
            else
                query.AppendFormat(@"WHERE p.id = '{0}' ORDER BY p.year DESC, p.month DESC LIMIT 1", payrollId);

            GetStartDay(DateTime.Now.Month, DateTime.Now.Year);
            //var (min, absences, rendered) = await DTRDatabase.Instance.GetMinsV2Async(userid, StartDate, EndDate, "regular");
            var payroll = new RegularPayrollModel
            {
                Fname = fname,
                Mname = mname,
                Lname = lname,
                PayrollId = 0,
                ID = userid,
                Date = DateTime.Now.ToString("MMMM yyyy"),
                AbsentDays = "",//absences,
                WorkingDays = DTRDatabase.Instance.GetWorkingDays(DateTime.Now),
                Salary = salary,
                PERA = 0,
                MinutesLate = 0,//min,
                ProfessionalTax = 0,
                CFI = new LoanModel
                {
                    Item = 0,
                    NoPayment = 0,
                    NoPaid = 0
                },
                GSIS_GFAL = new LoanModel
                {
                    Item = 0,
                    NoPayment = 0,
                    NoPaid = 0
                },
                GSIS_COMP = new LoanModel
                {
                    Item = 0,
                    NoPayment = 0,
                    NoPaid = 0
                },
                GSIS_Consoloan = new LoanModel
                {
                    Item = 0,
                    NoPayment = 0,
                    NoPaid = 0
                },
                GSIS_PolicyLoan = new LoanModel
                {
                    Item = 0,
                    NoPayment = 0,
                    NoPaid = 0
                },
                GSIS_EML = new LoanModel
                {
                    Item = 0,
                    NoPayment = 0,
                    NoPaid = 0
                },
                GSIS_UOLI = new LoanModel
                {
                    Item = 0,
                    NoPayment = 0,
                    NoPaid = 0
                },
                GSIS_EDU = new LoanModel
                {
                    Item = 0,
                    NoPayment = 0,
                    NoPaid = 0
                },
                GSIS_Help = new LoanModel
                {
                    Item = 0,
                    NoPayment = 0,
                    NoPaid = 0
                },
                GSIS_Rel = new LoanModel
                {
                    Item = 0,
                    NoPayment = 0,
                    NoPaid = 0
                },
                PagibigPremium = 0,
                PagibigLoan = new LoanModel
                {
                    Item = 0,
                    NoPayment = 0,
                    NoPaid = 0
                },
                PagibigMP2 = new LoanModel
                {
                    Item = 0,
                    NoPayment = 0,
                    NoPaid = 0
                },
                PagibigCalamity = new LoanModel
                {
                    Item = 0,
                    NoPayment = 0,
                    NoPaid = 0
                },
                SIMC = new LoanModel
                {
                    Item = 0,
                    NoPayment = 0,
                    NoPaid = 0
                },
                HWMPC = new LoanModel
                {
                    Item = 0,
                    NoPayment = 0,
                    NoPaid = 0
                },
                DBP = new LoanModel
                {
                    Item = 0,
                    NoPayment = 0,
                    NoPaid = 0
                },
                Disallowances = new LoanModel
                {
                    Item = 0,
                    NoPayment = 0,
                    NoPaid = 0
                }
            };

            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand(query.ToString(), connection))
                {
                    var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        payroll.Designation = reader["position_title"].ToString();
                        payroll.PayrollId = int.Parse(reader["id"].ToString());
                        payroll.ID = userid;
                        payroll.PERA = double.Parse(reader["pera"].ToString());
                        payroll.ProfessionalTax = double.Parse(reader["tax"].ToString());
                        payroll.GSIS_GFAL = new LoanModel
                        {
                            Item = double.Parse(reader["gsis_gfal"].ToString()),
                            NoPayment = int.Parse(reader["gsis_gfal_max"].ToString()),
                            NoPaid = int.Parse(reader["gsis_gfal_ctr"].ToString())
                        };
                        payroll.GSIS_COMP = new LoanModel
                        {
                            Item = double.Parse(reader["gsis_comp"].ToString()),
                            NoPayment = int.Parse(reader["gsis_comp_max"].ToString()),
                            NoPaid = int.Parse(reader["gsis_comp_ctr"].ToString())
                        };
                        payroll.CFI = new LoanModel
                        {
                            Item = double.Parse(reader["cfi"].ToString()),
                            NoPayment = int.Parse(reader["cfi_max"].ToString()),
                            NoPaid = int.Parse(reader["cfi_ctr"].ToString())
                        };
                        payroll.GSIS_Consoloan = new LoanModel
                        {
                            Item = double.Parse(reader["gsis_consoloan"].ToString()),
                            NoPayment = int.Parse(reader["gsis_consoloan_max"].ToString()),
                            NoPaid = int.Parse(reader["gsis_consoloan_ctr"].ToString())
                        };
                        payroll.GSIS_PolicyLoan = new LoanModel
                        {
                            Item = double.Parse(reader["gsis_policy_loan"].ToString()),
                            NoPayment = int.Parse(reader["gsis_policy_loan_max"].ToString()),
                            NoPaid = int.Parse(reader["gsis_policy_loan_ctr"].ToString())
                        };
                        payroll.GSIS_EML = new LoanModel
                        {
                            Item = double.Parse(reader["gsis_eml"].ToString()),
                            NoPayment = int.Parse(reader["gsis_eml_max"].ToString()),
                            NoPaid = int.Parse(reader["gsis_eml_ctr"].ToString())
                        };
                        payroll.GSIS_UOLI = new LoanModel
                        {
                            Item = double.Parse(reader["gsis_uoli"].ToString()),
                            NoPayment = int.Parse(reader["gsis_uoli_max"].ToString()),
                            NoPaid = int.Parse(reader["gsis_uoli_ctr"].ToString())
                        };
                        payroll.GSIS_EDU = new LoanModel
                        {
                            Item = double.Parse(reader["gsis_edu"].ToString()),
                            NoPayment = int.Parse(reader["gsis_edu_max"].ToString()),
                            NoPaid = int.Parse(reader["gsis_edu_ctr"].ToString())
                        };
                        payroll.GSIS_Help = new LoanModel
                        {
                            Item = double.Parse(reader["gsis_help"].ToString()),
                            NoPayment = int.Parse(reader["gsis_help_max"].ToString()),
                            NoPaid = int.Parse(reader["gsis_help_ctr"].ToString())
                        };
                        payroll.GSIS_Rel = new LoanModel
                        {
                            Item = double.Parse(reader["gsis_rel"].ToString()),
                            NoPayment = int.Parse(reader["gsis_rel_max"].ToString()),
                            NoPaid = int.Parse(reader["gsis_rel_ctr"].ToString())
                        };
                        payroll.PagibigPremium = double.Parse(reader["pagibig_premium"].ToString());
                        payroll.PagibigLoan = new LoanModel
                        {
                            Item = double.Parse(reader["pagibig_loan"].ToString()),
                            NoPayment = int.Parse(reader["pagibig_loan_max"].ToString()),
                            NoPaid = int.Parse(reader["pagibig_loan_ctr"].ToString())
                        };
                        payroll.PagibigMP2 = new LoanModel
                        {
                            Item = double.Parse(reader["pagibig_mp2"].ToString()),
                            NoPayment = int.Parse(reader["pagibig_mp2_max"].ToString()),
                            NoPaid = int.Parse(reader["pagibig_mp2_ctr"].ToString())
                        };
                        payroll.PagibigCalamity = new LoanModel
                        {
                            Item = double.Parse(reader["pagibig_calamity"].ToString()),
                            NoPayment = int.Parse(reader["pagibig_calamity_max"].ToString()),
                            NoPaid = int.Parse(reader["pagibig_calamity_ctr"].ToString())
                        };
                        payroll.SIMC = new LoanModel
                        {
                            Item = double.Parse(reader["simc"].ToString()),
                            NoPayment = int.Parse(reader["simc_max"].ToString()),
                            NoPaid = int.Parse(reader["simc_ctr"].ToString())
                        };
                        payroll.HWMPC = new LoanModel
                        {
                            Item = double.Parse(reader["hwmpc"].ToString()),
                            NoPayment = int.Parse(reader["hwmpc_max"].ToString()),
                            NoPaid = int.Parse(reader["hwmpc_ctr"].ToString())
                        };
                        payroll.DBP = new LoanModel
                        {
                            Item = double.Parse(reader["dbp"].ToString()),
                            NoPayment = int.Parse(reader["dbp_max"].ToString()),
                            NoPaid = int.Parse(reader["dbp_ctr"].ToString())
                        };
                        payroll.Disallowances = new LoanModel
                        {
                            Item = double.Parse(reader["disallowances"].ToString()),
                            NoPayment = int.Parse(reader["disallowances_max"].ToString()),
                            NoPaid = int.Parse(reader["disallowances_ctr"].ToString())
                        };
                    }
                    await reader.CloseAsync();
                }
                await connection.CloseAsync();
            }

            return payroll;
        }
        #endregion

        #region GET JOB ORDER PAYROLL LIST BY ID
        public async Task<List<JoPayrollModel>> GetJoPayrollListByID(string userid)
        {
            string query = "SELECT p.id, p.userid, p.start_date, p.end_date, p.month_salary, we.position_title, " +
                "pi.fname, pi.mname, pi.lname " +
                "FROM payroll.payroll p " +
                "LEFT JOIN pis.personal_information pi ON pi.userid = p.userid " +
                "LEFT JOIN (SELECT wes.userid, wes.position_title FROM pis.work_experience wes WHERE date_to = 'Present') we ON we.userid = p.userid " +
                "WHERE p.userid = '" + userid + "'";

            var payrolls = new List<JoPayrollModel>();

            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        payrolls.Add(new JoPayrollModel
                        {
                            Fname = reader["fname"].ToString(),
                            Mname = reader["mname"].ToString(),
                            Lname = reader["lname"].ToString(),
                            Designation = reader["position_title"].ToString(),
                            Salary = reader["month_salary"].ToString(),
                            PayrollId = int.Parse(reader["id"].ToString()),
                            ID = userid,
                            Date = DateTime.Parse(reader["start_date"].ToString()).ToString("MMMM yyyy"),
                            Range = DateTime.Parse(reader["start_date"].ToString()).Day == 1 ? 1 : 2
                        });
                    }
                    await reader.CloseAsync();
                }
                await connection.CloseAsync();
            }

            return payrolls;
        }
        #endregion

        #region GET REGULAR PAYROLL LIST BY ID
        public async Task<List<RegularPayrollModel>> GetRegularPayrollListByID(string userid)
        {
            var query = new StringBuilder();

            query.AppendFormat(@"
                SELECT p.id, p.userid, p.month, p.year,
                pi.fname, pi.mname, pi.lname, we.monthly_salary
                FROM payroll.regular_payroll p
                LEFT JOIN pis.personal_information pi ON pi.userid = p.userid
                LEFT JOIN (
                    SELECT w.userid, w.monthly_salary, w.salary_grade, w.date_to
                    FROM pis.work_experience w WHERE w.date_to = 'Present') 
                we ON we.userid = p.userid 
                WHERE p.userid = '{0}';
            ",userid);

            var payrolls = new List<RegularPayrollModel>();

            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand(query.ToString(), connection))
                {
                    var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var month = int.Parse(reader["month"].ToString());
                        var year = int.Parse(reader["year"].ToString());
                        var date = new DateTime(year, month, 1);
                        payrolls.Add(new RegularPayrollModel
                        {
                            Fname = reader["fname"].ToString(),
                            Mname = reader["mname"].ToString(),
                            Lname = reader["lname"].ToString(),
                            PayrollId = int.Parse(reader["id"].ToString()),
                            ID = userid,
                            Date = date.ToString("MMMM yyyy"),
                            Salary = reader["monthly_salary"].ToString()
                        });
                    }
                    await reader.CloseAsync();
                }
                await connection.CloseAsync();
            }

            return payrolls;
        }
        #endregion

        #region GET REGULAR PAYROLL LIST BY DATE
        public async Task<List<RegularPayrollModel>> GetRegularPayrollListByDate(int month, int year, string salary_charge)
        {
            var query = new StringBuilder();

            query.AppendFormat(@"
                SELECT p.id, pi.userid, p.philhealth, pi.fname, pi.mname, pi.lname, pi.division, we.position_title, we.salary_grade,
                pi.salary_charge, p.month, p.year, p.absent_days, p.working_days, p.month_salary, p.pera, p.minutes_late, p.tax, 
                p.cfi, ifnull(cfir.max, 0) as cfi_max, ifnull(cfir.count, 0) as cfi_ctr, 
                p.gsis_premium, 
                p.gsis_gfal, ifnull(gfal.max, 0) as gsis_gfal_max, ifnull(gfal.count, 0) as gsis_gfal_ctr,  
                p.gsis_comp, ifnull(comp.max, 0) as gsis_comp_max, ifnull(comp.count, 0) as gsis_comp_ctr,  
                p.gsis_consoloan, ifnull(gcr.max, 0) as gsis_consoloan_max, ifnull(gcr.count, 0) as gsis_consoloan_ctr,   
                p.gsis_policy_loan, ifnull(gpr.max, 0) as gsis_policy_loan_max, ifnull(gpr.count, 0) as gsis_policy_loan_ctr,   
                p.gsis_eml, ifnull(ger.max, 0) as gsis_eml_max, ifnull(ger.count, 0) as gsis_eml_ctr,   
                p.gsis_uoli, ifnull(gur.max, 0) as gsis_uoli_max, ifnull(gur.count, 0) as gsis_uoli_ctr,   
                p.gsis_edu, ifnull(gdr.max, 0) as gsis_edu_max, ifnull(gdr.count, 0) as gsis_edu_ctr,   
                p.gsis_help, ifnull(ghr.max, 0) as gsis_help_max, ifnull(ghr.count, 0) as gsis_help_ctr,   
                p.gsis_rel, ifnull(rr.max, 0) as gsis_rel_max, ifnull(rr.count, 0) as gsis_rel_ctr,   
                p.pagibig_premium, 
                p.pagibig_loan, ifnull(plr.max, 0) as pagibig_loan_max, ifnull(plr.count, 0) as pagibig_loan_ctr,  
                p.pagibig_mp2, ifnull(pmr.max, 0) as pagibig_mp2_max, ifnull(pmr.count, 0) as pagibig_mp2_ctr,   
                p.pagibig_calamity, ifnull(pcr.max, 0) as pagibig_calamity_max, ifnull(pcr.count, 0) as pagibig_calamity_ctr,  
                p.simc, ifnull(sr.max, 0) as simc_max, ifnull(sr.count, 0) as simc_ctr,   
                p.hwmpc, ifnull(cr.max, 0) as hwmpc_max, ifnull(cr.count, 0) as hwmpc_ctr,   
                p.dbp, ifnull(dr.max, 0) as dbp_max, ifnull(dr.count, 0) as dbp_ctr, 
                p.disallowances, ifnull(dsr.max, 0) as disallowances_max, ifnull(dsr.count, 0) as disallowances_ctr 
                FROM payroll.regular_payroll p 
                LEFT JOIN (SELECT * FROM pis.work_experience WHERE date_to = 'Present') we ON we.userid = p.userid 
                LEFT JOIN (
	                SELECT per.userid, per.fname, per.mname, per.lname, per.division_id, per.salary_charge, 
                    divi.description AS division
                    FROM pis.personal_information per
                    LEFT JOIN dts.division divi ON divi.id = per.division_id
                ) pi ON pi.userid = p.userid 
                LEFT JOIN payroll.cfi_remittance cfir ON cfir.userid = p.userid 
                LEFT JOIN payroll.gsis_gfal_remittance gfal ON gfal.userid = p.userid
                LEFT JOIN payroll.gsis_comp_remittance comp ON comp.userid = p.userid
                LEFT JOIN payroll.gsis_consoloan_remittance gcr ON gcr.userid = p.userid 
                LEFT JOIN payroll.gsis_policyloan_remittance gpr ON gpr.userid = p.userid 
                LEFT JOIN payroll.gsis_eml_remittance ger ON ger.userid = p.userid 
                LEFT JOIN payroll.gsis_uoli_remittance gur ON gur.userid = p.userid 
                LEFT JOIN payroll.gsis_edu_remittance gdr ON gdr.userid = p.userid 
                LEFT JOIN payroll.gsis_help_remittance ghr ON ghr.userid = p.userid 
                LEFT JOIN payroll.rel_remittance rr ON rr.userid = p.userid 
                LEFT JOIN payroll.pagibig_loan_remittance plr ON plr.userid = p.userid 
                LEFT JOIN payroll.pagibig_mp2_remittance pmr ON pmr.userid = p.userid 
                LEFT JOIN payroll.pagibig_calamity_remittance pcr ON pcr.userid = p.userid 
                LEFT JOIN payroll.simc_remittance sr ON sr.userid = p.userid 
                LEFT JOIN payroll.coop_remittance cr ON cr.userid = p.userid 
                LEFT JOIN payroll.dbp_remittance dr ON dr.userid = p.userid 
                LEFT JOIN payroll.disallowance_remittance dsr ON dsr.userid = p.userid 
                WHERE p.month = {0} 
                AND p.year = {1}
            ", month, year);

            if(!string.IsNullOrEmpty(salary_charge) && salary_charge != "ALL")
            {
                query.AppendFormat(@" AND pi.salary_charge = '{0}'", salary_charge);
            }

            var payrolls = new List<RegularPayrollModel>();

            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand(query.ToString(), connection))
                {
                    var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var date = new DateTime(year, month, 1);
                        payrolls.Add(new RegularPayrollModel
                        {
                            PayrollId = int.Parse(reader["id"].ToString()),
                            ID = reader["userid"].ToString(),
                            Fname = reader["fname"].ToString(),
                            Mname = reader["mname"].ToString(),
                            Lname = reader["lname"].ToString(),
                            Division = reader["division"].ToString(),
                            SalaryCharge = string.IsNullOrEmpty(reader["salary_charge"].ToString())? "No Salary Charge" :
                                reader["salary_charge"].ToString().SalCharge(),
                            SalaryGrade = reader["salary_grade"].ToString(),
                            Designation = reader["position_title"].ToString(),
                            Date = date.ToString("MMMM yyyy"),
                            AbsentDays = reader["absent_days"].ToString(),
                            WorkingDays = int.Parse(reader["working_days"].ToString()),
                            Salary = reader["month_salary"].ToString(),
                            PERA = double.Parse(reader["pera"].ToString()),
                            MinutesLate = int.Parse(reader["minutes_late"].ToString()),
                            ProfessionalTax = double.Parse(reader["tax"].ToString()),
                            CFI = new LoanModel
                            {
                                Item = double.Parse(reader["cfi"].ToString()),
                                NoPayment = int.Parse(reader["cfi_max"].ToString()),
                                NoPaid = int.Parse(reader["cfi_ctr"].ToString())
                            },
                            GSIS_GFAL = new LoanModel
                            {
                                Item = double.Parse(reader["gsis_gfal"].ToString()),
                                NoPayment = int.Parse(reader["gsis_gfal_max"].ToString()),
                                NoPaid = int.Parse(reader["gsis_gfal_ctr"].ToString())
                            },
                            GSIS_COMP = new LoanModel
                            {
                                Item = double.Parse(reader["gsis_comp"].ToString()),
                                NoPayment = int.Parse(reader["gsis_comp_max"].ToString()),
                                NoPaid = int.Parse(reader["gsis_comp_ctr"].ToString())
                            },
                            GSIS_Consoloan = new LoanModel
                            {
                                Item = double.Parse(reader["gsis_consoloan"].ToString()),
                                NoPayment = int.Parse(reader["gsis_consoloan_max"].ToString()),
                                NoPaid = int.Parse(reader["gsis_consoloan_ctr"].ToString())
                            },
                            GSIS_PolicyLoan = new LoanModel
                            {
                                Item = double.Parse(reader["gsis_policy_loan"].ToString()),
                                NoPayment = int.Parse(reader["gsis_policy_loan_max"].ToString()),
                                NoPaid = int.Parse(reader["gsis_policy_loan_ctr"].ToString())
                            },
                            GSIS_EML = new LoanModel
                            {
                                Item = double.Parse(reader["gsis_eml"].ToString()),
                                NoPayment = int.Parse(reader["gsis_eml_max"].ToString()),
                                NoPaid = int.Parse(reader["gsis_eml_ctr"].ToString())
                            },
                            GSIS_UOLI = new LoanModel
                            {
                                Item = double.Parse(reader["gsis_uoli"].ToString()),
                                NoPayment = int.Parse(reader["gsis_uoli_max"].ToString()),
                                NoPaid = int.Parse(reader["gsis_uoli_ctr"].ToString())
                            },
                            GSIS_EDU = new LoanModel
                            {
                                Item = double.Parse(reader["gsis_edu"].ToString()),
                                NoPayment = int.Parse(reader["gsis_edu_max"].ToString()),
                                NoPaid = int.Parse(reader["gsis_edu_ctr"].ToString())
                            },
                            GSIS_Help = new LoanModel
                            {
                                Item = double.Parse(reader["gsis_help"].ToString()),
                                NoPayment = int.Parse(reader["gsis_help_max"].ToString()),
                                NoPaid = int.Parse(reader["gsis_help_ctr"].ToString())
                            },
                            GSIS_Rel = new LoanModel
                            {
                                Item = double.Parse(reader["gsis_rel"].ToString()),
                                NoPayment = int.Parse(reader["gsis_rel" + "_max"].ToString()),
                                NoPaid = int.Parse(reader["gsis_rel" + "_ctr"].ToString())
                            },
                            PagibigPremium = double.Parse(reader["pagibig_premium"].ToString()),
                            PagibigLoan = new LoanModel
                            {
                                Item = double.Parse(reader["pagibig_loan"].ToString()),
                                NoPayment = int.Parse(reader["pagibig_loan" + "_max"].ToString()),
                                NoPaid = int.Parse(reader["pagibig_loan" + "_ctr"].ToString())
                            },
                            Disallowances = new LoanModel
                            {
                                Item = double.Parse(reader["disallowances"].ToString()),
                                NoPayment = int.Parse(reader["disallowances" + "_max"].ToString()),
                                NoPaid = int.Parse(reader["disallowances" + "_ctr"].ToString())
                            },
                            SIMC = new LoanModel
                            {
                                Item = double.Parse(reader["simc"].ToString()),
                                NoPayment = int.Parse(reader["simc" + "_max"].ToString()),
                                NoPaid = int.Parse(reader["simc" + "_ctr"].ToString())
                            },
                            HWMPC = new LoanModel
                            {
                                Item = double.Parse(reader["hwmpc"].ToString()),
                                NoPayment = int.Parse(reader["hwmpc" + "_max"].ToString()),
                                NoPaid = int.Parse(reader["hwmpc" + "_ctr"].ToString())
                            },
                            DBP = new LoanModel
                            {
                                Item = double.Parse(reader["dbp"].ToString()),
                                NoPayment = int.Parse(reader["dbp" + "_max"].ToString()),
                                NoPaid = int.Parse(reader["dbp" + "_ctr"].ToString())
                            },
                            PagibigMP2 = new LoanModel
                            {
                                Item = double.Parse(reader["pagibig_mp2"].ToString()),
                                NoPayment = int.Parse(reader["pagibig_mp2" + "_max"].ToString()),
                                NoPaid = int.Parse(reader["pagibig_mp2" + "_ctr"].ToString())
                            },
                            PagibigCalamity = new LoanModel
                            {
                                Item = double.Parse(reader["pagibig_calamity"].ToString()),
                                NoPayment = int.Parse(reader["pagibig_calamity" + "_max"].ToString()),
                                NoPaid = int.Parse(reader["pagibig_calamity" + "_ctr"].ToString())
                            }
                        });
                    }
                    await reader.CloseAsync();
                }
                await connection.CloseAsync();
            }

            return payrolls;
        }
        #endregion

        #region GET ALL REGULAR PAYSLIP
        public async Task<List<RegularPayslipModel>> GetRegularPayslipsAsync(DateTime date, int division)
        {
            var query = new StringBuilder();

            query.AppendFormat(@"
                SELECT pi.userid, pi.fname, pi.mname, pi.lname, pi.division_id,
                pi.employee_status, pi.job_status, wx.monthly_salary,
                p.id, pi.userid, p.month, p.year, p.absent_days, p.working_days, p.pera, p.minutes_late, p.tax, 
                p.cfi, p.gsis_premium, p.gsis_gfal, p.gsis_comp, p.gsis_consoloan, p.gsis_policy_loan, p.gsis_eml, p.gsis_uoli,
                p.gsis_edu, p.gsis_help, p.gsis_rel, p.pagibig_premium, p.pagibig_loan, p.pagibig_mp2, p.pagibig_calamity, p.simc,
                p.hwmpc, p.dbp, p.disallowances,
                IFNULL(hp.hazard_pay, 0.00) AS hazard_pay, IFNULL(hp.hwmpc_loan, 0.00) AS hwmpc_loan, IFNULL(hp.days_leave, 0) AS days_leave, 
                IFNULL(hp.mortuary, 0.00) AS mortuary, IFNULL(hp.days_oo, 0) AS days_oo, IFNULL(hp.globe_bill, 0.00) as globe_bill, 
                IFNULL(l.salary_1, 0.00) AS salary_1, IFNULL(l.salary_2, 0.00) AS salary_2,IFNULL(l.salary_3, 0.00) AS salary_3,
                IFNULL(l.salary_4, 0.00) AS salary_4,IFNULL(l.salary_5, 0.00) AS salary_5, IFNULL(l.disallowance, 0.00) AS disallowance,
                IFNULL(c.amount,0.00) AS amount, IFNULL(c.month_billing_amt, 0.00) AS month_billing_amt, c.month_bill,
                IFNULL(r.ra, 0.00) AS ra, IFNULL(r.ta, 0.00) AS ta, IFNULL(r.deduction, 0.00) AS deduction,
                IFNULL(s.subsistence_allowance, 0.00) AS subsistence_allowance, IFNULL(s.laundry_allowance, 0.00) AS laundry_allowance,
                IFNULL(s.no_days, 0) AS no_days, IFNULL(s.hwmpc, 0.00) AS hwmpc, IFNULL(hp.sun_bill, 0.00) as sun_bill
                FROM pis.personal_information pi 
                LEFT JOIN(SELECT* FROM pis.work_experience WHERE date_to = 'Present') AS wx ON wx.userid = pi.userid 
                LEFT JOIN payroll.regular_payroll p ON pi.userid = p.userid AND p.month = {0} AND p.year = {1}
                LEFT JOIN payroll.hazard_pay hp ON p.userid = hp.userid AND hp.month = {0} AND hp.year = {1}
                LEFT JOIN payroll.longevity l ON p.userid = l.userid AND l.month = {0} AND l.year = {1}
                LEFT JOIN payroll.communicable_allowance c ON p.userid = c.userid AND c.month = {0} AND c.year = {1}
                LEFT JOIN payroll.rata r ON p.userid = r.userid AND r.month = {0} AND r.year = {1}
                LEFT JOIN payroll.subsistence s ON p.userid = s.userid AND s.month = {0} AND s.year = {1}
                WHERE pi.employee_status = 1 AND pi.job_status = 'Permanent' 
                AND pi.userid != 'hr_admin' 
                AND (pi.region IS NULL OR pi.region = 'region_7') 
                AND pi.disbursement_type = 'ATM'
                AND pi.division_id = {2}
            ", date.Month, date.Year, division);

            var payslips = new List<RegularPayslipModel>();

            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand(query.ToString(), connection))
                {
                    var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        #region FIRST HALF
                        var payroll = new RegularPayrollModel
                        {
                            ID = reader["userid"].ToString(),
                            Fname = reader["fname"].ToString(),
                            Mname = reader["mname"].ToString(),
                            Lname = reader["lname"].ToString(),
                            Salary = reader["monthly_salary"].ToString(),
                            Date = date.ToString("MMMM yyyy"),
                            AbsentDays = reader["absent_days"].ToString(),
                            WorkingDays = reader["working_days"].ConvertInt(),
                            MinutesLate = reader["minutes_late"].ConvertInt(),
                            PERA = reader["pera"].ConvertDouble(),
                            ProfessionalTax = reader["tax"].ConvertDouble(),
                            GSIS_GFAL = new LoanModel
                            {
                                Item = reader["gsis_gfal"].ConvertDouble()
                            },
                            GSIS_COMP = new LoanModel
                            {
                                Item = reader["gsis_comp"].ConvertDouble()
                            },
                            CFI = new LoanModel
                            {
                                Item = reader["cfi"].ConvertDouble()
                            },
                            GSIS_Consoloan = new LoanModel
                            {
                                Item = reader["gsis_consoloan"].ConvertDouble()
                            },
                            GSIS_PolicyLoan = new LoanModel
                            {
                                Item = reader["gsis_policy_loan"].ConvertDouble()
                            },
                            GSIS_EML = new LoanModel
                            {
                                Item = reader["gsis_eml"].ConvertDouble()
                            },
                            GSIS_UOLI = new LoanModel
                            {
                                Item = reader["gsis_uoli"].ConvertDouble()
                            },
                            GSIS_EDU = new LoanModel
                            {
                                Item = reader["gsis_edu"].ConvertDouble()
                            },
                            GSIS_Help = new LoanModel
                            {
                                Item = reader["gsis_help"].ConvertDouble()
                            },
                            GSIS_Rel = new LoanModel
                            {
                                Item = reader["gsis_rel"].ConvertDouble()
                            },
                            PagibigPremium = reader["pagibig_premium"].ConvertDouble(),
                            PagibigLoan = new LoanModel
                            {
                                Item = reader["pagibig_loan"].ConvertDouble()
                            },
                            PagibigMP2 = new LoanModel
                            {
                                Item = reader["pagibig_mp2"].ConvertDouble()
                            },
                            PagibigCalamity = new LoanModel
                            {
                                Item = reader["pagibig_calamity"].ConvertDouble()
                            },
                            SIMC = new LoanModel
                            {
                                Item = reader["simc"].ConvertDouble()
                            },
                            HWMPC = new LoanModel
                            {
                                Item = reader["hwmpc"].ConvertDouble()
                            },
                            DBP = new LoanModel
                            {
                                Item = reader["dbp"].ConvertDouble()
                            },
                            Disallowances = new LoanModel
                            {
                                Item = reader["disallowances"].ConvertDouble()
                            }
                        };
                        var subsistence = new SubsistenceModel
                        {
                            ID = reader["userid"].ToString(),
                            Fname = reader["fname"].ToString(),
                            Mname = reader["mname"].ToString(),
                            Lname = reader["lname"].ToString(),
                            Salary = reader["monthly_salary"].ToString(),
                            Date = date.ToString("MMMM yyyy"),
                            Subsistence = reader["subsistence_allowance"].ConvertDouble(),
                            Laundry = reader["laundry_allowance"].ConvertDouble(),
                            HWMPC = reader["hwmpc"].ConvertDouble(),
                            NoDaysAbsent = reader["no_days"].ConvertDouble()
                        };
                        var longevity = new LongevityModel
                        {
                            ID = reader["userid"].ToString(),
                            Fname = reader["fname"].ToString(),
                            Mname = reader["mname"].ToString(),
                            Lname = reader["lname"].ToString(),
                            Salary = reader["monthly_salary"].ToString(),
                            Date = date.ToString("MMMM yyyy"),
                            Disallowance = reader["disallowance"].ConvertDouble(),
                            Salaries = new List<SalaryModel>
                            {
                                new SalaryModel
                                {
                                    Index = 1,
                                    Salary = double.Parse(reader["salary_1"].ToString())
                                },
                                new SalaryModel
                                {
                                    Index = 2,
                                    Salary = double.Parse(reader["salary_2"].ToString())
                                },
                                new SalaryModel
                                {
                                    Index = 3,
                                    Salary = double.Parse(reader["salary_3"].ToString())
                                },
                                new SalaryModel
                                {
                                    Index = 4,
                                    Salary = double.Parse(reader["salary_4"].ToString())
                                },
                                new SalaryModel
                                {
                                    Index = 5,
                                    Salary = double.Parse(reader["salary_5"].ToString())
                                }
                            }
                        };
                        #endregion
                        #region SECOND HALF
                        var hazard = new HazardModel
                        {
                            ID = reader["userid"].ToString(),
                            Fname = reader["fname"].ToString(),
                            Mname = reader["mname"].ToString(),
                            Lname = reader["lname"].ToString(),
                            Salary = reader["monthly_salary"].ToString(),
                            Date = date.ToString("MMMM yyyy"),
                            DaysWithLeave = int.Parse(reader["days_leave"].ToString()),
                            DaysWithOO = int.Parse(reader["days_oo"].ToString()),
                            HazardPay = double.Parse(reader["hazard_pay"].ToString()),
                            HWMPCLoan = double.Parse(reader["hwmpc_loan"].ToString()),
                            Mortuary = double.Parse(reader["mortuary"].ToString()),
                            GlobeBill = double.Parse(reader["globe_bill"].ToString()),
                            SunBill = double.Parse(reader["sun_bill"].ToString())
                        };
                        var cellphone = new CellphoneModel
                        {
                            ID = reader["userid"].ToString(),
                            Fname = reader["fname"].ToString(),
                            Mname = reader["mname"].ToString(),
                            Lname = reader["lname"].ToString(),
                            Salary = reader["monthly_salary"].ToString(),
                            Date = date.ToString("MMMM yyyy"),
                            Amount = double.Parse(reader["amount"].ToString()),
                            MonthBillAmount = double.Parse(reader["month_billing_amt"].ToString()),
                            MonthBill = reader["month_bill"].ToString()
                        };
                        var rata = new RATAModel
                        {
                            ID = reader["userid"].ToString(),
                            Fname = reader["fname"].ToString(),
                            Mname = reader["mname"].ToString(),
                            Lname = reader["lname"].ToString(),
                            Salary = reader["monthly_salary"].ToString(),
                            Date = date.ToString("MMMM yyyy"),
                            RA = double.Parse(reader["ra"].ToString()),
                            TA = double.Parse(reader["ta"].ToString()),
                            Deductions = double.Parse(reader["deduction"].ToString())
                        };
                        #endregion
                        payslips.Add(new RegularPayslipModel
                        {
                            FirstHalf = new FirstHalf
                            {
                                Longevity = longevity,
                                RegularPayroll = payroll,
                                Subsistence = subsistence
                            },
                            SecondHalf = new SecondHalf
                            {
                                Hazard = hazard,
                                CommAll = cellphone,
                                Rata = rata
                            }
                        });
                    }
                }
                await connection.CloseAsync();
            }
            return payslips;
        }
        #endregion

        #region GET HAZARD BY ID 

        public async Task<HazardModel> GetHazardById(string userid, string fname, string mname, string lname)
        {
            string query = "SELECT hp.id, hp.userid, hp.hazard_pay, hp.hwmpc_loan, hp.month, hp.year, hp.days_leave, hp.mortuary, " +
                "hp.days_oo, IFNULL(hp.globe_bill, 0.00) as globe_bill, IFNULL(hp.sun_bill, 0.00) as sun_bill " +
                "FROM payroll.hazard_pay hp " +
                "WHERE hp.userid = '" + userid + "' ORDER BY hp.year DESC, hp.month DESC LIMIT 1;";

            var hazard = new HazardModel
            {
                HazardID = 0,
                ID = userid,
                Fname = fname,
                Mname = mname,
                Lname = lname,
                Date = DateTime.Now.ToString("MMMM yyyy"),
                DaysWithLeave = 0,
                DaysWithOO = 0,
                HazardPay = 0,
                HWMPCLoan = 0,
                Mortuary = 0,
                GlobeBill = 0,
                SunBill = 0
            };

            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    var reader = await command.ExecuteReaderAsync();
                    var hazards = new List<HazardModel>();
                    while (await reader.ReadAsync())
                    {
                        hazards.Add(new HazardModel
                        {
                            HazardID = int.Parse(reader["id"].ToString()),
                            ID = userid,
                            Fname = fname,
                            Mname = mname,
                            Lname = lname,
                            Date = DateTime.Now.ToString("MMMM yyyy"),
                            DaysWithLeave = int.Parse(reader["days_leave"].ToString()),
                            DaysWithOO = int.Parse(reader["days_oo"].ToString()),
                            HazardPay = double.Parse(reader["hazard_pay"].ToString()),
                            HWMPCLoan = double.Parse(reader["hwmpc_loan"].ToString()),
                            Mortuary = double.Parse(reader["mortuary"].ToString()),
                            GlobeBill = double.Parse(reader["globe_bill"].ToString()),
                            SunBill = double.Parse(reader["sun_bill"].ToString())
                        });
                    }
                    if(hazards.Count() > 0)
                        hazard = hazards.FirstOrDefault();
                    await reader.CloseAsync();
                }
                await connection.CloseAsync();
            }

            return hazard;
        }

        #endregion

        #region GET HAZARD BY DATE

        public async Task<HazardModel> GetHazardByDateAsync(string userid, DateTime date)
        {
            var query = new StringBuilder();
            query.AppendFormat(@"
                SELECT hp.id, hp.userid, hp.hazard_pay, hp.hwmpc_loan, hp.month, hp.year, hp.days_leave, hp.mortuary,
                hp.days_oo, IFNULL(hp.globe_bill, 0.00) as globe_bill, IFNULL(hp.sun_bill, 0.00) as sun_bill
                FROM payroll.hazard_pay hp
                WHERE hp.userid = '{0}' 
                AND hp.month = {1}
                AND hp.year = {2}
                ORDER BY hp.year DESC, hp.month DESC LIMIT 1;
            ", userid, date.Month, date.Year);

            var hazard = new HazardModel
            {
                HazardID = 0,
                Date = date.ToString("MMMM yyyy"),
                DaysWithLeave = 0,
                DaysWithOO = 0,
                HazardPay = 0,
                HWMPCLoan = 0,
                Mortuary = 0,
                GlobeBill = 0,
                SunBill = 0
            };

            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand(query.ToString(), connection))
                {
                    var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        hazard.HazardID = int.Parse(reader["id"].ToString());
                        hazard.DaysWithLeave = int.Parse(reader["days_leave"].ToString());
                        hazard.DaysWithOO = int.Parse(reader["days_oo"].ToString());
                        hazard.HazardPay = double.Parse(reader["hazard_pay"].ToString());
                        hazard.HWMPCLoan = double.Parse(reader["hwmpc_loan"].ToString());
                        hazard.Mortuary = double.Parse(reader["mortuary"].ToString());
                        hazard.GlobeBill = double.Parse(reader["globe_bill"].ToString());
                        hazard.SunBill = double.Parse(reader["sun_bill"].ToString());
                    }
                    await reader.CloseAsync();
                }
                await connection.CloseAsync();
            }

            return hazard;
        }

        #endregion

        #region GET HAZARD LIST BY ID

        public async Task<List<HazardModel>> GetHazardListById(string userid)
        {
            string query = "SELECT hp.id, hp.userid, hp.hazard_pay, hp.hwmpc_loan, hp.month, hp.year, hp.days_leave,  " +
                "hp.days_oo, IFNULL(hp.globe_bill, 0.00) as globe_bill, IFNULL(hp.sun_bill, 0.00) as sun_bill " +
                "FROM payroll.hazard_pay hp " +
                "WHERE hp.userid = '" + userid + "';";

            var hazards = new List<HazardModel>();

            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var month = int.Parse(reader["month"].ToString());
                        var year = int.Parse(reader["year"].ToString());
                        var date = new DateTime(year, month, 1);
                        hazards.Add(new HazardModel
                        {
                            HazardID = int.Parse(reader["id"].ToString()),
                            ID = userid,
                            Date = date.ToString("MMMM yyyy")
                        });
                    }
                    await reader.CloseAsync();
                }
                await connection.CloseAsync();
            }

            return hazards;
        }
        #endregion

        #region UPDATE HAZARD
        public async Task<bool> UpdateHazardAsync(HazardModel model)
        {
            string query = "REPLACE INTO payroll.hazard_pay VALUES('0','" + model.ID+ "', '" + model.HazardPay + "', '" + model.HWMPCLoan + "', " +
                "'" + model.Mortuary + "', '" + model.DigitelBilling + "', '" + model.Month + "', '" + model.Year+ "', " +
                "'" + model.DaysWithLeave + "', '" + model.DaysWithOO + "', '" + model.GlobeBill + "', '" + model.SunBill + "')";
            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
                await connection.CloseAsync();
            }

            return true;
        }
        #endregion

        #region GET RATA/CELLPHONE EMPLOYEES

        public async Task<List<EmployeeCellphoneRata>> GetEmployeesWithCPRATA()
        {
            string query = "SELECT ecr.userid, ecr.cellphone, ecr.rata, pi.fname, pi.mname, pi.lname " +
                "FROM payroll.employee_cellphone_rata ecr " +
                "LEFT JOIN pis.personal_information pi ON pi.userid = ecr.userid";

            var employees = new List<EmployeeCellphoneRata>();

            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        employees.Add(new EmployeeCellphoneRata
                        {
                            ID = reader["userid"].ToString(),
                            Cellphone = reader["cellphone"].ToString() == "1",
                            RATA = reader["rata"].ToString() == "1",
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

        #region GET EMPLOYEE RATA
        public async Task<RATAModel> GetRATAByIdAsync(string userid, string fname, string mname, string lname)
        {
            string query = "SELECT * FROM payroll.rata WHERE userid = '" + userid + "' ORDER BY year DESC, month DESC LIMIT 1;";

            var rata = new RATAModel
            {
                ID = userid,
                Fname = fname,
                Mname = mname,
                Lname = lname,
                Date = DateTime.Now.ToString("MMMM yyyy")
            };
            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        rata.RA = double.Parse(reader["ra"].ToString());
                        rata.TA = double.Parse(reader["ta"].ToString());
                        rata.Deductions = double.Parse(reader["deduction"].ToString());
                        rata.Remarks = reader["remarks"].ToString();
                    }
                    await reader.CloseAsync();
                }
                await connection.CloseAsync();
            }
            return rata;
        }
        #endregion

        #region UPDATE RATA
        public async Task<bool> UpdateRATAAsync(RATAModel model)
        {
            string query = "REPLACE INTO payroll.rata VALUES('0','" + model.ID + "', '" + model.RA + "', '" + model.TA + "', " +
                "'" + model.Deductions + "', '" + model.Remarks + "', '" + model.Month + "', '" + model.Year + "')";
            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
                await connection.CloseAsync();
            }

            return true;
        }
        #endregion

        #region GET EMPLOYEE RATA LIST
        public async Task<List<RATAModel>> GetRATAListByIdAsync(string userid)
        {
            string query = "SELECT * FROM payroll.rata WHERE userid = '" + userid + "'";

            var ratas = new List<RATAModel>();
            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var month = int.Parse(reader["month"].ToString());
                        var year = int.Parse(reader["year"].ToString());
                        var date = new DateTime(year, month, 1);
                        ratas.Add(new RATAModel
                        {
                            RATAID = int.Parse(reader["id"].ToString()),
                            ID = userid,
                            Date = date.ToString("MMMM yyyy")
                        });
                    }
                    await reader.CloseAsync();
                }
                await connection.CloseAsync();
            }
            return ratas;
        }
        #endregion

        #region GET EMPLOYEE CELLPHONE
        public async Task<CellphoneModel> GetCellphoneByIdAsync(string userid, string fname, string mname, string lname)
        {
            string query = "SELECT * FROM payroll.communicable_allowance WHERE userid = '" + userid + "' ORDER BY year DESC, month DESC LIMIT 1;";

            var cellphone = new CellphoneModel
            {
                ID = userid,
                Fname = fname,
                Mname = mname,
                Lname = lname,
                Date = DateTime.Now.ToString("MMMM yyyy"),
                MonthBill = DateTime.Now.ToString("MMMM")
            };
            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        cellphone.Amount = double.Parse(reader["amount"].ToString());
                        cellphone.MonthBillAmount = double.Parse(reader["month_billing_amt"].ToString());
                        cellphone.Remarks = reader["remarks"].ToString();
                        cellphone.MonthBill = reader["month_bill"].ToString();
                    }
                    await reader.CloseAsync();
                }
                await connection.CloseAsync();
            }
            return cellphone;
        }
        #endregion

        #region GET EMPLOYEE CELLPHONE LIST
        public async Task<List<CellphoneModel>> GetCellphoneListByIdAsync(string userid)
        {
            string query = "SELECT * FROM payroll.communicable_allowance WHERE userid = '" + userid + "'";

            var cellphones = new List<CellphoneModel>();
            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var month = int.Parse(reader["month"].ToString());
                        var year = int.Parse(reader["year"].ToString());
                        var date = new DateTime(year, month, 1);
                        cellphones.Add(new CellphoneModel
                        {
                            CellphoneId = int.Parse(reader["id"].ToString()),
                            ID = userid,
                            Date = date.ToString("MMMM yyyy")
                        });
                    }
                    await reader.CloseAsync();
                }
                await connection.CloseAsync();
            }
            return cellphones;
        }
        #endregion

        #region UPDATE CELLPHONE
        public async Task<bool> UpdateCellphoneAsync(CellphoneModel model)
        {
            string query = "REPLACE INTO payroll.communicable_allowance VALUES('0','" + model.ID + "', '" + model.Amount + "', '" + model.MonthBillAmount + "', " +
                "'" + model.Remarks + "', '" + model.Month + "', '" + model.Year + "', '" + model.MonthBill + "')";
            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
                await connection.CloseAsync();
            }

            return true;
        }
        #endregion

        #region IF SUBSISTENCE EXISTS
        public int SubsistenceExists(string userid, int month, int year)
        {
            string query = "SELECT id FROM payroll.subsistence WHERE userid = '" + userid + "' " +
                   "AND month = '" + month + "' AND year = '" + year + "' LIMIT 1";

            int id = 0;
            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                connection.Open();
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        id = int.Parse(reader["id"].ToString());
                    }
                    reader.Close();
                }
                connection.Close();
            }

            return id;
        }
        #endregion

        #region GET EMPLOYEE SUBISTENCE
        public async Task<SubsistenceModel> GetSubsistenceByIdAsync(string userid, string fname, string mname, string lname, int month, int year)
        {
            string query = "SELECT * FROM payroll.subsistence WHERE userid = '" + userid + "' ORDER BY year DESC, month DESC LIMIT 1";
            var date = new DateTime(year, month, 1);
            var subsistence = new SubsistenceModel
            {
                SubsistenceId = 0,
                ID = userid,
                Fname = fname,
                Mname = mname,
                Lname = lname,
                Date = date.ToString("MMMM yyyy"),
                Subsistence = GetDeduction("subsistence_allowance"),
                Laundry = GetDeduction("laundry_allowance"),
                HWMPC = 0,
                NoDaysAbsent = 0
            };
            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        subsistence.Subsistence = double.Parse(reader["subsistence_allowance"].ToString());
                        subsistence.Laundry = double.Parse(reader["laundry_allowance"].ToString());
                        subsistence.HWMPC = double.Parse(reader["hwmpc"].ToString());
                    }
                    await reader.CloseAsync();
                }
                await connection.CloseAsync();
            }
            return subsistence;
        }
        #endregion

        #region GET EMPLOYEE SUBISTENCE LIST
        public async Task<List<SubsistenceModel>> GetSubsistenceListByIdAsync(string userid)
        {
            string query = "SELECT * FROM payroll.subsistence WHERE userid = '" + userid + "'";

            var subsistences = new List<SubsistenceModel>();
            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var month = int.Parse(reader["month"].ToString());
                        var year = int.Parse(reader["year"].ToString());
                        var date = new DateTime(year, month, 1);
                        subsistences.Add(new SubsistenceModel
                        {
                            SubsistenceId = int.Parse(reader["id"].ToString()),
                            ID = userid,
                            Date = date.ToString("MMMM yyyy")
                        });
                    }
                    await reader.CloseAsync();
                }
                await connection.CloseAsync();
            }
            return subsistences;
        }
        #endregion

        #region GET EMPLOYEE LONGEVITY
        public async Task<LongevityModel> GetLongevityByIdAsync(string userid, string fname, string mname, string lname, int month, int year)
        {
            var query = new StringBuilder();
            query.AppendFormat(@"
            SELECT pi.userid, pi.fname, pi.mname, pi.lname, sc.description AS section,
             wx.monthly_salary, pi.disbursement_type,
             IFNULL(lg.salary_1,0.00) AS salary_1, 
             IFNULL(lg.salary_2,0.00) AS salary_2, 
             IFNULL(lg.salary_3,0.00) AS salary_3, 
             IFNULL(lg.salary_4,0.00) AS salary_4, 
             IFNULL(lg.salary_5,0.00) AS salary_5, 
             lg.disallowance, lg.month, lg.year
            FROM pis.personal_information pi 
            LEFT JOIN (
	            SELECT * FROM payroll.longevity
                ORDER BY year DESC, month DESC
	            ) lg ON lg.userid = pi.userid
            LEFT JOIN dts.section sc ON sc.id = pi.section_id 
            LEFT JOIN(SELECT* FROM pis.work_experience WHERE date_to = 'Present') AS wx ON wx.userid = pi.userid 
            WHERE pi.employee_status = 1 AND pi.job_status = 'Permanent' 
            AND pi.userid != 'hr_admin' 
            AND (pi.region IS NULL OR pi.region = 'region_7') 
            AND pi.disbursement_type = 'ATM'
            AND pi.userid = '{0}'
            ORDER BY lg.year DESC, lg.month DESC LIMIT 1
            ", userid);
            var date = new DateTime(year, month, 1);
            var longevity = new LongevityModel
            {
                LongevityId = 0,
                ID = userid,
                Fname = fname,
                Mname = mname,
                Lname = lname,
                Date = date.ToString("MMMM yyyy"),
                Disallowance = 0,
                Salaries = NewSalaries(5)
            };
            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand(query.ToString(), connection))
                {
                    var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        for(int x=0; x<5; x++)
                        {
                            longevity.Salaries[x].Salary = double.Parse(reader["salary_" + (x + 1)].ToString());
                        }
                        longevity.Disallowance = double.Parse(reader["disallowance"].ToString());
                    }
                    await reader.CloseAsync();
                }
                await connection.CloseAsync();
            }
            return longevity;
        }

        public async Task<LongevityModel> GetLongevityByDateAsync(string userid, DateTime date)
        {
            var query = new StringBuilder();
            query.AppendFormat(@"
                SELECT * FROM payroll.longevity
                WHERE userid = '{0}'
                AND month = {1}
                AND year = {2}
            ", userid, date.Month, date.Year);
            var longevity = new LongevityModel
            {
                LongevityId = 0,
                ID = userid,
                Date = date.ToString("MMMM yyyy"),
                Disallowance = 0,
                Salaries = NewSalaries(5)
            };
            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand(query.ToString(), connection))
                {
                    var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        for (int x = 0; x < 5; x++)
                        {
                            longevity.Salaries[x].Salary = double.Parse(reader["salary_" + (x + 1)].ToString());
                        }
                        longevity.Disallowance = double.Parse(reader["disallowance"].ToString());
                    }
                    await reader.CloseAsync();
                }
                await connection.CloseAsync();
            }
            return longevity;
        }
        #endregion

        #region GET EMPLOYEE LONGEVITY LIST
        public async Task<List<LongevityModel>> GetLongevityListByIdAsync(string userid)
        {
            string query = "SELECT * FROM payroll.longevity WHERE userid = '" + userid + "'";

            var longevities = new List<LongevityModel>();
            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var month = int.Parse(reader["month"].ToString());
                        var year = int.Parse(reader["year"].ToString());
                        var date = new DateTime(year, month, 1);
                        longevities.Add(new LongevityModel
                        {
                            LongevityId = int.Parse(reader["id"].ToString()),
                            ID = userid,
                            Date = date.ToString("MMMM yyyy")
                        });
                    }
                    await reader.CloseAsync();
                }
                await connection.CloseAsync();
            }
            return longevities;
        }
        #endregion

        #region UPDATE SUBSISTENCE
        public async Task<bool> UpdateSubsistenceAsync(SubsistenceModel model)
        {
            string query = "REPLACE INTO payroll.subsistence VALUES('0','" + model.ID + "', '" + model.Subsistence + "', '" + model.Laundry + "', " +
                "'" + model.LessTotal + "', '" + model.HWMPC + "', '" + model.NoDaysAbsent + "', '" + model.Remarks + "', " +
                "'" + model.Month + "', '" + model.Year + "')";
            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
                await connection.CloseAsync();
            }

            return true;
        }
        #endregion

        #region UPDATE LONGEVITY
        public async Task<bool> UpdateLongevityAsync(LongevityModel model)
        {
            var query = new StringBuilder();
            query.AppendFormat(@"
            REPLACE INTO payroll.longevity VALUES
            ('0','{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}')
            ", model.ID, model.Salaries[0].Salary, model.Salaries[1].Salary,
            model.Salaries[2].Salary, model.Salaries[3].Salary, model.Salaries[4].Salary,
            model.Disallowance, model.Month, model.Year);
            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand(query.ToString(), connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
                await connection.CloseAsync();
            }

            return true;
        }
        #endregion

        #region GET REMITTANCES
        //Regular
        public async Task<List<PagibigModel>> GetPagibigByDateAsync(DateTime date)
        {
            var query = new StringBuilder();
            query.AppendFormat(@"
                SELECT pi.userid, pi.fname, pi.mname, pi.lname, pi.name_extension, pi.disbursement_type,
                pi.pag_ibigno,
                IFNULL(rp.pagibig_premium, 0.00) AS pagibig_premium
                FROM pis.personal_information pi 
                LEFT JOIN (
	                SELECT * 
	                FROM payroll.regular_payroll 
                    WHERE month = {0} 
                    AND year = {1})
                rp ON rp.userid = pi.userid
                WHERE pi.employee_status = 1 AND pi.job_status = 'Permanent' 
                AND pi.userid != 'hr_admin' 
                AND (pi.region IS NULL OR pi.region = 'region_7') 
                AND pi.disbursement_type = 'ATM';
                ", date.Month, date.Year);

            var pagibigList = new List<PagibigModel>();

            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand(query.ToString(), connection))
                {
                    var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        pagibigList.Add(new PagibigModel
                        {
                            Lname = reader["lname"].ToString(),
                            Fname = reader["fname"].ToString(),
                            Mname = reader["mname"].ToString(),
                            NameExtension = reader["name_extension"].ToString(),
                            PagibigID = reader["pag_ibigno"].ToString(),
                            PagibigAccNo = "",
                            MembershipPro = "F1",
                            PerCov = date.ToString("yyyy") + date.ToString("MM"),
                            EEShare = double.Parse(reader["pagibig_premium"].ToString()),
                            ERShare = 100,
                            Remarks = "",
                            DisbursementType = reader["disbursement_type"].ToString()
                        });
                    }
                }
                await connection.CloseAsync();
            }
            return pagibigList.OrderBy(x=>x.Lname).ToList();
        }
        //job order
        public async Task<List<PagibigModel>> GetPagibigByDateAsync(DateTime startDate, DateTime endDate)
        {
            var query = new StringBuilder();
            query.AppendFormat(@"
                SELECT pi.userid, pi.fname, pi.mname, pi.lname, pi.name_extension, pi.disbursement_type,
                pi.pag_ibigno,
                IFNULL(rp.pagibig, 0.00) AS pagibig
                FROM pis.personal_information pi 
                LEFT JOIN (
	                SELECT * 
	                FROM payroll.payroll 
                    WHERE start_date = '{0}'
                    AND end_date = '{1}')
                rp ON rp.userid = pi.userid
                WHERE pi.employee_status = 1 AND pi.job_status = 'Permanent' 
                AND pi.userid != 'hr_admin' 
                AND (pi.region IS NULL OR pi.region = 'region_7') 
                AND pi.disbursement_type = 'ATM';
                ", startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));

            var pagibigList = new List<PagibigModel>();

            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand(query.ToString(), connection))
                {
                    var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        pagibigList.Add(new PagibigModel
                        {
                            Lname = reader["lname"].ToString(),
                            Fname = reader["fname"].ToString(),
                            Mname = reader["mname"].ToString(),
                            NameExtension = reader["name_extension"].ToString(),
                            PagibigID = reader["pag_ibigno"].ToString(),
                            PagibigAccNo = "",
                            MembershipPro = "",
                            PerCov = startDate.ToString("yyyy")+ startDate.ToString("MM"),
                            EEShare = double.Parse(reader["pagibig"].ToString()),
                            ERShare = 0,
                            Remarks = "",
                            DisbursementType = reader["disbursement_type"].ToString()
                        });
                    }
                }
                await connection.CloseAsync();
            }
            return pagibigList.OrderBy(x=>x.Lname).ToList();
        }
        //JOB ORDER
        public async Task<List<RemittanceModel>> GetRemittanceByDateAsync(DateTime startDate, DateTime endDate, string remittance)
        {
            var column = remittance == "phic" ? "phic" : "coop";
            var query = new StringBuilder();
            query.AppendFormat(@"
                SELECT pi.userid, pi.fname, pi.mname, pi.lname, pi.phicno,
                pi.disbursement_type, IFNULL(p.{2}, '0.00') AS {2}
                FROM pis.personal_information pi 
                LEFT JOIN (
	                SELECT * FROM payroll.payroll WHERE start_date >= '{0}' AND end_date <= '{1}')
                p ON p.userid = pi.userid
                WHERE pi.employee_status = 1 AND pi.job_status = 'Job Order' 
                AND pi.userid != 'hr_admin' 
                AND (pi.region IS NULL OR pi.region = 'region_7') 
                AND pi.disbursement_type = 'ATM';
                ", startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"), column);

            var phicList = new List<RemittanceModel>();

            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand(query.ToString(), connection))
                {
                    var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        phicList.Add(new RemittanceModel
                        {
                            PhicNo = reader["phicno"].ToString(),
                            Lname = reader["lname"].ToString(),
                            Fname = reader["fname"].ToString(),
                            Mname = reader["mname"].ToString(),
                            Amount = double.Parse(reader[column].ToString()),
                            DisbursementType = reader["disbursement_type"].ToString()
                        });
                    }
                }
                await connection.CloseAsync();
            }
            return phicList;
        }
        //REGULAR
        public async Task<List<RemittanceModel>> GetRemittanceByDateAsync(DateTime date, string remittance)
        {
            var column = remittance == "phic" ? "philhealth" : "hwmpc";
            var query = new StringBuilder();
            query.AppendFormat(@"
                SELECT rp.userid, pi.phicno, pi.lname, pi.fname, pi.mname, rp.month, rp.year, rp.{2}, pi.disbursement_type
                FROM payroll.regular_payroll rp
                LEFT JOIN pis.personal_information pi ON pi.userid = rp.userid
                WHERE rp.month = {0} AND rp.year = {1} AND rp.{2} != 0
                ", date.Month, date.Year, column);

            var phicList = new List<RemittanceModel>();

            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand(query.ToString(), connection))
                {
                    var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        phicList.Add(new RemittanceModel
                        {
                            PhicNo = reader["phicno"].ToString(),
                            Lname = reader["lname"].ToString(),
                            Fname = reader["fname"].ToString(),
                            Mname = reader["mname"].ToString(),
                            Amount = double.Parse(reader[column].ToString()),
                            DisbursementType = reader["disbursement_type"].ToString()
                        });
                    }
                }
                await connection.CloseAsync();
            }
            return phicList;
        }
        #endregion

        #region GET OTHER PAYROLLS
        //HAZARD
        public async Task<List<HazardModel>> GetHazardByDateAsync(int month, int year)
        {
            var query = new StringBuilder();
            query.AppendFormat(@"
                SELECT hp.userid, hp.hazard_pay, hp.hwmpc_loan, hp.mortuary, hp.globe_bill, hp.sun_bill,
                hp.days_leave, hp.days_oo, pi.section, pi.monthly_salary,
                pi.fname, pi.mname, pi.lname, pi.designation, pi.salary_grade
                FROM payroll.hazard_pay hp
                LEFT JOIN (
	                SELECT info.userid, info.fname, info.mname, info.lname, des.description AS designation,
	                we.date_to, we.salary_grade, sec.description AS section, we.monthly_salary
	                FROM pis.personal_information info
	                LEFT JOIN (SELECT * FROM pis.work_experience WHERE date_to = 'Present') we on we.userid = info.userid
                    LEFT JOIN dts.designation des ON des.id = info.designation_id
	                LEFT JOIN dts.section sec ON sec.id = info.section_id
	                WHERE we.date_to = 'Present'
                ) pi ON pi.userid = hp.userid
                WHERE hp.month = {0} AND hp.year = {1};
            ", month, year);

            var hazardList = new List<HazardModel>();

            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand(query.ToString(), connection))
                {
                    var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        hazardList.Add(new HazardModel
                        {
                            ID = reader["userid"].ToString(),
                            Lname = reader["lname"].ToString(),
                            Fname = reader["fname"].ToString(),
                            Mname = reader["mname"].ToString(),
                            Salary = reader["monthly_salary"].ToString(),
                            Section = reader["section"].ToString(),
                            Designation = reader["designation"].ToString(),
                            HazardPay = double.Parse(reader["hazard_pay"].ToString()),
                            HWMPCLoan = double.Parse(reader["hwmpc_loan"].ToString()),
                            GlobeBill = double.Parse(reader["globe_bill"].ToString()),
                            SunBill = double.Parse(reader["sun_bill"].ToString()),
                            DaysWithOO = int.Parse(reader["days_oo"].ToString()),
                            DaysWithLeave = int.Parse(reader["days_leave"].ToString()),
                        });
                    }
                }
                await connection.CloseAsync();
            }
            return hazardList;
        }
        //RATA
        public async Task<List<RATAModel>> GetRataPayrollAsync(int month, int year)
        {
            var query = new StringBuilder();
            query.AppendFormat(@"
                SELECT ecr.userid, ra.ra, ra.ta, ra.deduction, ra.month, ra.year, ra.remarks,
                pi.fname, pi.mname, pi.lname, pi.designation,
                wo.salary_grade
                FROM payroll.employee_cellphone_rata ecr
                LEFT JOIN (
	                SELECT p.userid, p.fname, p.mname, p.lname, des.description AS designation
	                FROM pis.personal_information p
	                LEFT JOIN dts.designation des ON des.id = p.designation_id
	                )
                pi ON pi.userid = ecr.userid
                LEFT JOIN payroll.rata ra ON ra.userid = ecr.userid
                LEFT JOIN (SELECT * FROM pis.work_experience we WHERE we.date_to = 'Present') wo ON wo.userid = ecr.userid
                WHERE ra.year = {0} AND ra.month = {1}
            ", year, month);

            var ratalist = new List<RATAModel>();

            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand(query.ToString(), connection))
                {
                    var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        ratalist.Add(new RATAModel
                        {
                            ID = reader["userid"].ToString(),
                            Lname = reader["lname"].ToString(),
                            Fname = reader["fname"].ToString(),
                            Mname = reader["mname"].ToString(),
                            Designation = reader["designation"].ToString(),
                            RA = double.Parse(reader["ra"].ToString()),
                            TA = double.Parse(reader["ta"].ToString()),
                            Deductions = double.Parse(reader["deduction"].ToString()),
                            Remarks = reader["remarks"].ToString(),
                            SalaryGrade = reader["salary_grade"].ToString()
                        });
                    }
                }
                await connection.CloseAsync();
            }
            return ratalist.OrderByDescending(x=>x.Tranch).ThenByDescending(x=>x.Grade).ThenByDescending(x=>x.Step).ToList();
        }
        //CELLPHONE
        public async Task<List<CellphoneModel>> GetCellphonePayrollAsync(int month, int year)
        {
            var query = new StringBuilder();
            query.AppendFormat(@"
                SELECT ecr.userid, ca.amount, ca.month_billing_amt, ca.month_bill, ca.month, ca.year, ca.remarks,
                pi.fname, pi.mname, pi.lname, pi.designation,
                wo.salary_grade
                FROM payroll.employee_cellphone_rata ecr
                LEFT JOIN (
	                SELECT p.userid, p.fname, p.mname, p.lname, des.description AS designation
	                FROM pis.personal_information p
	                LEFT JOIN dts.designation des ON des.id = p.designation_id
	                )
                pi ON pi.userid = ecr.userid
                LEFT JOIN payroll.communicable_allowance ca ON ca.userid = ecr.userid
                LEFT JOIN (SELECT * FROM pis.work_experience we WHERE we.date_to = 'Present') wo ON wo.userid = ecr.userid
                WHERE ca.year = {0} AND ca.month = {1}
            ", year, month);

            var cellphoneList = new List<CellphoneModel>();

            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand(query.ToString(), connection))
                {
                    var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        cellphoneList.Add(new CellphoneModel
                        {
                            ID = reader["userid"].ToString(),
                            Lname = reader["lname"].ToString(),
                            Fname = reader["fname"].ToString(),
                            Mname = reader["mname"].ToString(),
                            Designation = reader["designation"].ToString(),
                            Amount = double.Parse(reader["amount"].ToString()),
                            MonthBill = reader["month_bill"].ToString(),
                            MonthBillAmount = double.Parse(reader["month_billing_amt"].ToString()),
                            Remarks = reader["remarks"].ToString(),
                            SalaryGrade = reader["salary_grade"].ToString()
                        });
                    }
                }
                await connection.CloseAsync();
            }
            return cellphoneList.OrderByDescending(x => x.Tranch).ThenByDescending(x => x.Grade).ThenByDescending(x => x.Step).ToList();
        }
        //SUBSISTENCE
        public async Task<List<SubsistenceModel>> GetSubsistencePayrollAsync(int month, int year)
        {
            var query = new StringBuilder();
            query.AppendFormat(@"
                SELECT sub.userid, sub.subsistence_allowance, sub.laundry_allowance,
                sub.hwmpc, sub.no_days, sub.remarks,
                info.fname, info.mname, info.lname, info.designation, info.section, info.salary_charge
                FROM payroll.subsistence sub
                LEFT JOIN (
	                SELECT pi.userid, pi.fname, pi.mname, pi.lname, pi.salary_charge,
                    des.description AS designation,
                    sec.description AS section
                    FROM pis.personal_information pi
                    LEFT JOIN dts.designation des ON des.id = pi.designation_id
                    LEFT JOIN dts.section sec ON sec.id = pi.section_id)
                info ON info.userid = sub.userid
                WHERE sub.year = {0} AND sub.month = {1}
            ", year, month);

            var subistenceList = new List<SubsistenceModel>();

            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand(query.ToString(), connection))
                {
                    var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        subistenceList.Add(new SubsistenceModel
                        {
                            ID = reader["userid"].ToString(),
                            Lname = reader["lname"].ToString(),
                            Fname = reader["fname"].ToString(),
                            Mname = reader["mname"].ToString(),
                            Designation = reader["designation"].ToString(),
                            Section = string.IsNullOrEmpty(reader["section"].ToString())? "No Section" : reader["section"].ToString(),
                            SalaryCharge = string.IsNullOrEmpty(reader["salary_charge"].ToString())? "No Salary Charge" : reader["salary_charge"].ToString().SalCharge(),
                            Subsistence = double.Parse(reader["subsistence_allowance"].ToString()),
                            Laundry = double.Parse(reader["laundry_allowance"].ToString()),
                            HWMPC = double.Parse(reader["hwmpc"].ToString()),
                            NoDaysAbsent = double.Parse(reader["no_days"].ToString()),
                            Remarks = reader["remarks"].ToString()
                        });
                    }
                }
                await connection.CloseAsync();
            }
            return subistenceList;
        }
        //LONGEVITY
        public async Task<List<LongevityModel>> GetLongevityPayrollAsync(int month, int year)
        {
            var query = new StringBuilder();
            query.AppendFormat(@"
                SELECT lg.salary_1, lg.salary_2, lg.salary_3, lg.salary_4, lg.salary_5, lg.disallowance,
                info.userid, info.fname, info.mname, info.lname, info.designation,
                info.date_from, IFNULL(info.salary_charge,'No Salary Charge') as salary_charge
                FROM payroll.longevity lg
                LEFT JOIN (
	                SELECT pi.userid, pi.fname, pi.mname, pi.lname, ds.description AS designation,
	                we.date_from, pi.salary_charge
	                FROM pis.personal_information pi 
	                LEFT JOIN dts.designation ds ON ds.id = pi.designation_id 
	                LEFT JOIN pis.work_experience we ON we.userid = pi.userid 
	                WHERE pi.employee_status = 1 AND pi.job_status = 'Permanent' 
	                AND pi.userid != 'hr_admin' 
	                AND (pi.region IS NULL OR pi.region = 'region_7') 
	                AND pi.disbursement_type = 'ATM'
	                GROUP BY userid
                ) info ON info.userid = lg.userid
                WHERE lg.month = {0} AND lg.year = {1}
            ", month, year);

            var longevities = new List<LongevityModel>();

            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand(query.ToString(), connection))
                {
                    var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var salaries = NewSalaries(5);
                        for(int x = 0; x < 5; x++)
                        {
                            Debug.WriteLine("salary_" + (x+1));
                            salaries[x].Salary = double.Parse(reader["salary_" + (x+1)].ToString());
                            Debug.WriteLine(salaries[x].Salary + "");
                        }
                        longevities.Add(new LongevityModel
                        {
                            ID = reader["userid"].ToString(),
                            Lname = reader["lname"].ToString(),
                            Fname = reader["fname"].ToString(),
                            Mname = reader["mname"].ToString(),
                            Designation = reader["designation"].ToString(),
                            SalaryCharge = reader["salary_charge"].ToString().SalCharge(),
                            EntranceToDuty = reader["date_from"].ToString(),
                            Disallowance = double.Parse(reader["disallowance"].ToString()),
                            Salaries = salaries
                        });
                    }
                }
                await connection.CloseAsync();
            }
            return longevities;
        }
        #endregion

        #region ADD CELLPHONE RATA
        public async Task<bool> AddCellphoneRataAsync(string userid)
        {
            var query = new StringBuilder();

            query.AppendFormat(@"
                REPLACE INTO payroll.employee_cellphone_rata
                ( '0', '{0}', '1', '1');
                ", userid);

            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand(query.ToString(), connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
                await connection.CloseAsync();
            }

            return true;
        }
        #endregion

        #region DELETE CELLPHONE RATA
        public async Task<bool> DeleteCellphoneRataAsync(string userid)
        {
            var query = new StringBuilder();

            query.AppendFormat(@"
                DELETE FROM payroll.employee_cellphone_rata WHERE userid = '{0}'
                ", userid);

            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand(query.ToString(), connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
                await connection.CloseAsync();
            }

            return true;
        }
        #endregion

        #region ADD EMPLOYEE CPRATA
        public async Task<bool> AddEmployeeAsync(string userid)
        {
            var query = new StringBuilder();

            query.AppendFormat(@"
            REPLACE INTO payroll.employee_cellphone_rata VALUES
            ('0', '{0}', 1, 1)
            ", userid);

            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand(query.ToString(), connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
                await connection.CloseAsync();
            }

            return true;
        }
        #endregion

        #region GET REGULAY PAYSLIP
        public async Task<RegularPayslipModel> GetPayslipAsync(string userid, DateTime date)
        {
            var query = new StringBuilder();
            query.AppendFormat(@"
                SELECT p.id, pi.userid, p.philhealth, pi.fname, pi.mname, pi.lname, pi.division, we.position_title, we.salary_grade,
                pi.salary_charge, p.month, p.year, p.absent_days, p.working_days, p.month_salary, p.pera, p.minutes_late, p.tax, 
                p.cfi, p.gsis_premium, p.gsis_gfal, p.gsis_comp, p.gsis_consoloan, p.gsis_policy_loan, p.gsis_eml, p.gsis_uoli,
                p.gsis_edu, p.gsis_help, p.gsis_rel, p.pagibig_premium, p.pagibig_loan, p.pagibig_mp2, p.pagibig_calamity, p.simc,
                p.hwmpc, p.dbp, p.disallowances,
                IFNULL(hp.hazard_pay, 0.00) AS hazard_pay, IFNULL(hp.hwmpc_loan, 0.00) AS hwmpc_loan, IFNULL(hp.days_leave, 0) AS days_leave, 
                IFNULL(hp.mortuary, 0.00) AS mortuary, IFNULL(hp.days_oo, 0) AS days_oo, IFNULL(hp.globe_bill, 0.00) as globe_bill, IFNULL(hp.sun_bill, 0.00) as sun_bill,
                IFNULL(l.salary_1, 0.00) AS salary_1, IFNULL(l.salary_2, 0.00) AS salary_2,IFNULL(l.salary_3, 0.00) AS salary_3,
                IFNULL(l.salary_4, 0.00) AS salary_4,IFNULL(l.salary_5, 0.00) AS salary_5, IFNULL(l.disallowance, 0.00) AS disallowance,
                IFNULL(c.amount,0.00) AS amount, IFNULL(c.month_billing_amt, 0.00) AS month_billing_amt, c.month_bill,
                IFNULL(r.ra, 0.00) AS ra, IFNULL(r.ta, 0.00) AS ta, IFNULL(r.deduction, 0.00) AS deduction,
                IFNULL(s.subsistence_allowance, 0.00) AS subsistence_allowance, IFNULL(s.laundry_allowance, 0.00) AS laundry_allowance,
                IFNULL(s.no_days, 0) AS no_days, IFNULL(s.hwmpc, 0.00) AS hwmpc
                FROM payroll.regular_payroll p 
                LEFT JOIN (SELECT * FROM pis.work_experience WHERE date_to = 'Present') we ON we.userid = p.userid 
                LEFT JOIN (
	                SELECT per.userid, per.fname, per.mname, per.lname, per.division_id, per.salary_charge, 
	                divi.description AS division
	                FROM pis.personal_information per
	                LEFT JOIN dts.division divi ON divi.id = per.division_id
                ) pi ON pi.userid = p.userid 
                LEFT JOIN payroll.hazard_pay hp ON p.userid = hp.userid AND hp.month = {0} AND hp.year = {1}
                LEFT JOIN payroll.longevity l ON p.userid = l.userid AND l.month = {0} AND l.year = {1}
                LEFT JOIN payroll.communicable_allowance c ON p.userid = c.userid AND c.month = {0} AND c.year = {1}
                LEFT JOIN payroll.rata r ON p.userid = r.userid AND r.month = {0} AND r.year = {1}
                LEFT JOIN payroll.subsistence s ON p.userid = s.userid AND s.month = {0} AND s.year = {1}
                WHERE p.month = {0} AND p.year = {1} AND p.userid = '{2}';
            ", date.Month, date.Year, userid);

            var payslip = new RegularPayslipModel();

            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand(query.ToString(), connection))
                {
                    var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        #region FIRST HALF
                        var payroll = new RegularPayrollModel
                        {
                            ID = userid,
                            Fname = reader["fname"].ToString(),
                            Mname = reader["mname"].ToString(),
                            Lname = reader["lname"].ToString(),
                            Designation = reader["position_title"].ToString(),
                            Salary = reader["month_salary"].ToString(),
                            Date = date.ToString("MMMM yyyy"),
                            AbsentDays = reader["absent_days"].ToString(),
                            WorkingDays = int.Parse(reader["working_days"].ToString()),
                            MinutesLate = int.Parse(reader["minutes_late"].ToString()),
                            PERA = double.Parse(reader["pera"].ToString()),
                            ProfessionalTax = double.Parse(reader["tax"].ToString()),
                            GSIS_GFAL = new LoanModel
                            {
                                Item = double.Parse(reader["gsis_gfal"].ToString())
                            },
                            GSIS_COMP = new LoanModel
                            {
                                Item = double.Parse(reader["gsis_comp"].ToString())
                            },
                            CFI = new LoanModel
                            {
                                Item = double.Parse(reader["cfi"].ToString())
                            },
                            GSIS_Consoloan = new LoanModel
                            {
                                Item = double.Parse(reader["gsis_consoloan"].ToString())
                            },
                            GSIS_PolicyLoan = new LoanModel
                            {
                                Item = double.Parse(reader["gsis_policy_loan"].ToString())
                            },
                            GSIS_EML = new LoanModel
                            {
                                Item = double.Parse(reader["gsis_eml"].ToString())
                            },
                            GSIS_UOLI = new LoanModel
                            {
                                Item = double.Parse(reader["gsis_uoli"].ToString())
                            },
                            GSIS_EDU = new LoanModel
                            {
                                Item = double.Parse(reader["gsis_edu"].ToString())
                            },
                            GSIS_Help = new LoanModel
                            {
                                Item = double.Parse(reader["gsis_help"].ToString())
                            },
                            GSIS_Rel = new LoanModel
                            {
                                Item = double.Parse(reader["gsis_rel"].ToString())
                            },
                            PagibigPremium = double.Parse(reader["pagibig_premium"].ToString()),
                            PagibigLoan = new LoanModel
                            {
                                Item = double.Parse(reader["pagibig_loan"].ToString())
                            },
                            PagibigMP2 = new LoanModel
                            {
                                Item = double.Parse(reader["pagibig_mp2"].ToString())
                            },
                            PagibigCalamity = new LoanModel
                            {
                                Item = double.Parse(reader["pagibig_calamity"].ToString())
                            },
                            SIMC = new LoanModel
                            {
                                Item = double.Parse(reader["simc"].ToString())
                            },
                            HWMPC = new LoanModel
                            {
                                Item = double.Parse(reader["hwmpc"].ToString())
                            },
                            DBP = new LoanModel
                            {
                                Item = double.Parse(reader["dbp"].ToString())
                            },
                            Disallowances = new LoanModel
                            {
                                Item = double.Parse(reader["disallowances"].ToString())
                            }
                        };
                        var subsistence = new SubsistenceModel
                        {
                            ID = userid,
                            Fname = reader["fname"].ToString(),
                            Mname = reader["mname"].ToString(),
                            Lname = reader["lname"].ToString(),
                            Designation = reader["position_title"].ToString(),
                            Salary = reader["month_salary"].ToString(),
                            Date = date.ToString("MMMM yyyy"),
                            Subsistence = double.Parse(reader["subsistence_allowance"].ToString()),
                            Laundry = double.Parse(reader["laundry_allowance"].ToString()),
                            HWMPC = double.Parse(reader["hwmpc"].ToString()),
                            NoDaysAbsent = double.Parse(reader["no_days"].ToString())
                        };
                        var longevity = new LongevityModel
                        {
                            ID = userid,
                            Fname = reader["fname"].ToString(),
                            Mname = reader["mname"].ToString(),
                            Lname = reader["lname"].ToString(),
                            Designation = reader["position_title"].ToString(),
                            Salary = reader["month_salary"].ToString(),
                            Date = date.ToString("MMMM yyyy"),
                            Disallowance = double.Parse(reader["disallowance"].ToString()),
                            Salaries = new List<SalaryModel>
                            {
                                new SalaryModel
                                {
                                    Index = 1,
                                    Salary = double.Parse(reader["salary_1"].ToString())
                                },
                                new SalaryModel
                                {
                                    Index = 2,
                                    Salary = double.Parse(reader["salary_2"].ToString())
                                },
                                new SalaryModel
                                {
                                    Index = 3,
                                    Salary = double.Parse(reader["salary_3"].ToString())
                                },
                                new SalaryModel
                                {
                                    Index = 4,
                                    Salary = double.Parse(reader["salary_4"].ToString())
                                },
                                new SalaryModel
                                {
                                    Index = 5,
                                    Salary = double.Parse(reader["salary_5"].ToString())
                                }
                            }
                        };
                        #endregion
                        #region SECOND HALF
                        var hazard = new HazardModel
                        {
                            ID = userid,
                            Fname = reader["fname"].ToString(),
                            Mname = reader["mname"].ToString(),
                            Lname = reader["lname"].ToString(),
                            Designation = reader["position_title"].ToString(),
                            Salary = reader["month_salary"].ToString(),
                            Date = date.ToString("MMMM yyyy"),
                            DaysWithLeave = int.Parse(reader["days_leave"].ToString()),
                            DaysWithOO = int.Parse(reader["days_oo"].ToString()),
                            HazardPay = double.Parse(reader["hazard_pay"].ToString()),
                            HWMPCLoan = double.Parse(reader["hwmpc_loan"].ToString()),
                            Mortuary = double.Parse(reader["mortuary"].ToString()),
                            GlobeBill = double.Parse(reader["globe_bill"].ToString()),
                            SunBill = double.Parse(reader["sun_bill"].ToString())
                        };
                        var cellphone = new CellphoneModel
                        {
                            ID = userid,
                            Fname = reader["fname"].ToString(),
                            Mname = reader["mname"].ToString(),
                            Lname = reader["lname"].ToString(),
                            Designation = reader["position_title"].ToString(),
                            Salary = reader["month_salary"].ToString(),
                            Date = date.ToString("MMMM yyyy"),
                            Amount = double.Parse(reader["amount"].ToString()),
                            MonthBillAmount = double.Parse(reader["month_billing_amt"].ToString()),
                            MonthBill = reader["month_bill"].ToString()
                        };
                        var rata = new RATAModel
                        {
                            ID = userid,
                            Fname = reader["fname"].ToString(),
                            Mname = reader["mname"].ToString(),
                            Lname = reader["lname"].ToString(),
                            Designation = reader["position_title"].ToString(),
                            Salary = reader["month_salary"].ToString(),
                            Date = date.ToString("MMMM yyyy"),
                            RA = double.Parse(reader["ra"].ToString()),
                            TA = double.Parse(reader["ta"].ToString()),
                            Deductions = double.Parse(reader["deduction"].ToString())
                        };
                        #endregion
                        payslip = new RegularPayslipModel
                        {
                            FirstHalf = new FirstHalf
                            {
                                Longevity = longevity,
                                RegularPayroll = payroll,
                                Subsistence = subsistence
                            },
                            SecondHalf = new SecondHalf
                            {
                                Hazard = hazard,
                                CommAll = cellphone,
                                Rata = rata
                            }
                        };
                    }
                }
                await connection.CloseAsync();
            }
            return payslip;
        }
        #endregion

        #region GET MANDATORY DEDUCTIONS
        public double GetDeduction(string name)
        {
            var query = "SELECT * FROM payroll.mandatory_deductions WHERE name = '" + name + "'";
            double deduction = 0.00;
            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                connection.Open();
                using (MySqlCommand command = new MySqlCommand(query.ToString(), connection))
                {
                    var reader = command.ExecuteReader();
                    while(reader.Read())
                    {
                        deduction = double.Parse(reader["amount"].ToString());
                    }
                }
                connection.Close();
            }

            return deduction;
        }
        #endregion

        #region HELPERS
        public List<SalaryModel> NewSalaries(int ctr)
        {
            var salaries = new List<SalaryModel>();
            for(int x = 1; x <= ctr; x++)
            {
                salaries.Add(new SalaryModel
                {
                    Index = x,
                    Salary = 0
                });
            }
            return salaries;
        }
        public void GetStartDay(int month, int year)
        {
            StartDate = new DateTime(year, month, 1);
            EndDate = new DateTime(year, month, DateTime.DaysInMonth(year, month));
        }

        public void GetStartDay(int month, int year, bool option)
        {
            if(option)
            {
                StartDate = new DateTime(year, month, 1);
                EndDate = new DateTime(year, month, 15);
            }
            else
            {
                StartDate = new DateTime(year, month, 16);
                EndDate = new DateTime(year, month, DateTime.DaysInMonth(year, month));
            }
        }


        public void GetStartDay(string date, string option)
        {
            var year = int.Parse(date.Split(" ")[1]);
            var month = Months.All.First(x => x.Key == date.Split(" ")[0]).Value;
            if (option == "1")
            {
                StartDate = new DateTime(year, month, 1);
                EndDate = new DateTime(year, month, 15);
            }
            else
            {
                StartDate = new DateTime(year, month, 16);
                EndDate = new DateTime(year, month, DateTime.DaysInMonth(year, month));
            }
        }

        public void GetStartDay(string date)
        {
            var year = int.Parse(date.Split(" ")[1]);
            var month = Months.All.First(x => x.Key == date.Split(" ")[0]).Value;
            StartDate = new DateTime(year, month, 1);
            EndDate = new DateTime(year, month, DateTime.DaysInMonth(year, month));
        }
        #endregion
    }
}
