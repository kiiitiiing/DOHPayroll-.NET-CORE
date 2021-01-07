using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DOHPayroll.Databases;
using DOHPayroll.Resources;
using DOHPayroll.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DOHPayroll.Controllers
{
    public class JsonController : Controller
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        #region GET MINS LATE AND ABSENCES
        [HttpPost]
        public async Task<string> GetMinsAbsences(string userid, string date, string option, string job_status)
        {
            GetStartDay(date, option);
            var (mins, absentList, rendered) = await DTRDatabase.Instance.GetMinsV2Async(userid, StartDate, EndDate, job_status);
            var workingDays = DTRDatabase.Instance.GetWorkingDays(StartDate);
            return mins + " " + absentList + " " + workingDays + " " + rendered;
        }
        #endregion

        #region UPDATE PAYMENTS
        [HttpPost]
        public async Task<bool> UpdateRemittances(string userid, string type, string value, string noPayments, string noPaid)
        {
            return await PayrollDatabase.Instance.UpdateRemittanceAsync(userid, type, value, noPayments, noPaid);
        }
        #endregion

        #region UPDATE SALARY
        [HttpPost]
        public async Task<string> UpdateSalary(string userid, string tranch, string grade, string step)
        {
            var salary = await PISDatabase.Instance.GetSalary(tranch, grade, step);
            if (PISDatabase.Instance.UpdateEmployeeSalary(salary, userid, tranch, grade, step))
                return salary;
            else
                return "";
        }
        #endregion

        #region GET EMPLOYEES
        [HttpGet]
        public async Task<JsonResult> GetEmployeesJson(string name)
        {
            var employees = await PISDatabase.Instance.GetEmployeesAsync(name);
            return Json( new { items = employees.AsDropDown()});
        }

        [HttpPost]
        public async Task<bool> AddEmployee(string userid)
        {
            return await PayrollDatabase.Instance.AddEmployeeAsync(userid);
        }

        [HttpPost]
        public async Task<bool> DeleteEmployee(string userid)
        {
            return await PayrollDatabase.Instance.DeleteCellphoneRataAsync(userid);
        }
        #endregion

        #region HELPERS

        public void GetStartDay(string date, string option)
        {
            var year = int.Parse(date.Split(" ")[1]);
            var month = Months.All.First(x => x.Key == date.Split(" ")[0]).Value;
            if (option == "1")
            {
                StartDate = new DateTime(year, month, 1);
                EndDate = new DateTime(year, month, 15);
            }
            else if(option == "2")
            {
                StartDate = new DateTime(year, month, 16);
                EndDate = new DateTime(year, month, DateTime.DaysInMonth(year, month));
            }
            else
            {
                StartDate = new DateTime(year, month, 1);
                EndDate = new DateTime(year, month, DateTime.DaysInMonth(year, month));
            }
        }

        #endregion

        #region DELETE ROW

        public async Task<bool> DeleteRow(string id, string table)
        {
            var result = await PayrollDatabase.Instance.DeleteRowByIdAsync(id, table);
            return result;
        }

        #endregion
    }
}
