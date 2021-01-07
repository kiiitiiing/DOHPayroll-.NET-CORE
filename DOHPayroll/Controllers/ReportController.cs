using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DOHPayroll.Databases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DOHPayroll.Controllers
{
    [Authorize(Policy = "Administrator")]
    public class ReportController : Controller
    {
        #region GENERATE REPORTS
        public async Task<IActionResult> Generate()
        {
            ViewBag.JobOrderSalaryCharge = await PISDatabase.Instance.GetSalaryChargeAsync("Job Order");
            ViewBag.RegularSalaryCharge = await PISDatabase.Instance.GetSalaryChargeAsync("Permanent");
            ViewBag.Divisions = await DTSDatabase.Instance.GetDivisionAsync();
            return View();
        }
        #endregion

        #region GET RATA/CELLPHONE EMPLOYEES
        public async Task<IActionResult> RataCellphoneEmployees()
        {
            var employees = await PayrollDatabase.Instance.GetEmployeesWithCPRATA();

            return PartialView(employees);
        }
        #endregion


    }
}
