using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DOHPayroll.Databases;
using DOHPayroll.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Newtonsoft.Json.Linq;

namespace DOHPayroll.Controllers
{
    public class PartialViewController : Controller
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        #region JOB ORDER LIST
        public async Task<IActionResult> JobOrderList(string disbursementType, string search, int? page)
        {
            var jobOrders = await PISDatabase.Instance.GetJobOrdersByDisbursementType(disbursementType);
            if (!string.IsNullOrEmpty(search))
            {
                jobOrders = jobOrders.Where(x => x.Fullname.Contains(search, StringComparison.OrdinalIgnoreCase) || x.ID == search).ToList();
            }
            int size = 10;
            var action = this.ControllerContext.RouteData.Values["action"].ToString();
            ViewBag.Type = disbursementType;
            ViewBag.CurrentSearch = search;
            return PartialView(PaginatedList<Employee>.CreateAsync(jobOrders.OrderBy(x => x.Fullname).Where(x => !string.IsNullOrEmpty(x.Lname)).ToList(), action, page ?? 1, size));
        }
        #endregion

        #region JOB ORDER PAYROLL

        [HttpGet]
        public async Task<IActionResult> EditJoPayroll(string userid, string fname, string mname, string lname, string salary, int? payrollId)
        {
            var payroll = await PayrollDatabase.Instance.GetJoPayrollByID(userid, fname, mname, lname, salary, payrollId);
            return PartialView(payroll);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditJoPayroll(JoPayrollModel model)
        {
            if(ModelState.IsValid)
            {
                if(await PayrollDatabase.Instance.UpdateJoPayrollAsync(model))
                {
                    return PartialView(model);
                }

            }

            return PartialView(model);
        }

        #endregion

        #region REGULAR PAYROLL
        [HttpGet]
        public async Task<IActionResult> EditRegularPayroll(string userid, string fname, string mname, string lname, string salary)
        {
            int? payrollId = null;
            var payroll = await PayrollDatabase.Instance.GetRegularPayrollByID(userid, fname, mname, lname, salary, payrollId);
            return PartialView(payroll);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRegularPayroll(RegularPayrollModel model)
        {
            if (ModelState.IsValid)
            {
                if (await PayrollDatabase.Instance.UpdateRegularPayrollAsync(model))
                {
                    return PartialView(model);
                }

            }

            return PartialView(model);
        }
        #endregion

        #region JOB ORDER PAYROLL LIST

        public async Task<IActionResult> JobOrderPayrollList(string userid, int? page)
        {
            var payrollList = await PayrollDatabase.Instance.GetJoPayrollListByID(userid);

            int size = 9;
            var action = this.ControllerContext.RouteData.Values["action"].ToString();
            ViewBag.UserID = userid;
            return PartialView(PaginatedList<JoPayrollModel>.CreateAsync(payrollList.OrderByDescending(x=>x.StartDate).ToList(), action, page ?? 1, size));
        }

        #endregion

        #region REGULAR PAYROLL LIST

        public async Task<IActionResult> RegularPayrollList(string userid, int? page)
        {
            var payrollList = await PayrollDatabase.Instance.GetRegularPayrollListByID(userid);

            int size = 9;
            var action = this.ControllerContext.RouteData.Values["action"].ToString();
            ViewBag.UserID = userid;
            return PartialView(PaginatedList<RegularPayrollModel>.CreateAsync(payrollList.OrderByDescending(x => x.Year).ThenByDescending(x => x.Month).ToList(), action, page ?? 1, size));
        }

        #endregion

        #region REGULAR LIST
        public async Task<IActionResult> RegularList(string disbursementType, string search, string cprata, int? page)
        {
            var regulars = await PISDatabase.Instance.GetRegularsByDisbursementType(disbursementType);
            if (!string.IsNullOrEmpty(search))
            {
                regulars = regulars.Where(x => x.Fullname.Contains(search.ToUpper()) || x.ID == search).ToList();
            }
            if(cprata == "true")
            {
                regulars = regulars.Where(x => x.CPRATA == true).ToList();
            }
            int size = 10;
            var action = this.ControllerContext.RouteData.Values["action"].ToString();
            ViewBag.Type = disbursementType;
            ViewBag.CurrentSearch = search;
            return PartialView(PaginatedList<Employee>.CreateAsync(regulars.OrderBy(x => x.Fullname).Where(x => !string.IsNullOrEmpty(x.Lname)).ToList(), action, page ?? 1, size));
        }
        #endregion

        #region HAZARD
        [HttpGet]
        public async Task<IActionResult> GenerateHazard(string userid, string fname, string mname, string lname)
        {
            var hazard = await PayrollDatabase.Instance.GetHazardById(userid, fname, mname, lname);

            return PartialView(hazard);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateHazard(HazardModel model)
        {
            var errors = ModelState.Values.SelectMany(x => x.Errors);
            if(ModelState.IsValid)
            {
                if(await PayrollDatabase.Instance.UpdateHazardAsync(model))
                {
                    return PartialView(model);
                }
            }

            ViewBag.Errors = errors;

            return PartialView(model);
        }

        #endregion

        #region HAZARD LIST

        public async Task<IActionResult> GetHazardList(string userid, int? page)
        {
            var hazard = await PayrollDatabase.Instance.GetHazardListById(userid);
            int size = 9;
            var action = this.ControllerContext.RouteData.Values["action"].ToString();
            ViewBag.UserID = userid;
            return PartialView(PaginatedList<HazardModel>.CreateAsync(hazard.OrderByDescending(x => x.Year).ThenByDescending(x => x.Month).ToList(), action, page ?? 1, size));
        }

        #endregion

        #region RATA

        public async Task<IActionResult> GenerateRATA(string userid, string fname, string mname, string lname)
        {
            var rata = await PayrollDatabase.Instance.GetRATAByIdAsync(userid, fname, mname, lname);
            return PartialView(rata);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateRATA(RATAModel model)
        {
            var errors = ModelState.Values.SelectMany(x => x.Errors);
            if (ModelState.IsValid)
            {
                if (await PayrollDatabase.Instance.UpdateRATAAsync(model))
                {
                    return PartialView(model);
                }
            }

            ViewBag.Errors = errors;

            return PartialView(model);
        }

        #endregion

        #region RATA LIST

        public async Task<IActionResult> GetRATAList(string userid, int? page)
        {
            var rata = await PayrollDatabase.Instance.GetRATAListByIdAsync(userid);
            int size = 9;
            var action = this.ControllerContext.RouteData.Values["action"].ToString();
            ViewBag.UserID = userid;
            return PartialView(PaginatedList<RATAModel>.CreateAsync(rata.OrderByDescending(x => x.Year).ThenByDescending(x => x.Month).ToList(), action, page ?? 1, size));
        }

        #endregion

        #region CELLPHONE

        [HttpGet]
        public async Task<IActionResult> GenerateCellphone(string userid, string fname, string mname, string lname)
        {
            var cellphone = await PayrollDatabase.Instance.GetCellphoneByIdAsync(userid, fname, mname, lname);
            return PartialView(cellphone);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateCellphone(CellphoneModel model)
        {
            var errors = ModelState.Values.SelectMany(x => x.Errors);
            if (ModelState.IsValid)
            {
                if (await PayrollDatabase.Instance.UpdateCellphoneAsync(model))
                {
                    return PartialView(model);
                }
            }

            ViewBag.Errors = errors;

            return PartialView(model);
        }

        #endregion

        #region CELLPHONE LIST

        public async Task<IActionResult> GetCellphoneList(string userid, int? page)
        {
            var cellphone = await PayrollDatabase.Instance.GetCellphoneListByIdAsync(userid);
            int size = 9;
            var action = this.ControllerContext.RouteData.Values["action"].ToString();
            ViewBag.UserID = userid;
            return PartialView(PaginatedList<CellphoneModel>.CreateAsync(cellphone.OrderByDescending(x => x.Year).ThenByDescending(x => x.Month).ToList(), action, page ?? 1, size));
        }

        #endregion

        #region SUBISTENCE

        [HttpGet]
        public async Task<IActionResult> GenerateSubsistence(string userid, string fname, string mname, string lname)
        {
            StartDate = DateTime.Now;
            var subsistence = await PayrollDatabase.Instance.GetSubsistenceByIdAsync(userid, fname, mname, lname, StartDate.Month, StartDate.Year);
            return PartialView(subsistence);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateSubsistence(SubsistenceModel model)
        {
            var errors = ModelState.Values.SelectMany(x => x.Errors);
            if (ModelState.IsValid)
            {
                if (await PayrollDatabase.Instance.UpdateSubsistenceAsync(model))
                {
                    return PartialView(model);
                }
            }

            ViewBag.Errors = errors;

            return PartialView(model);
        }

        #endregion

        #region SUBISTENCE LIST

        public async Task<IActionResult> GetSubsistenceList(string userid, int? page)
        {
            var cellphone = await PayrollDatabase.Instance.GetSubsistenceListByIdAsync(userid);
            int size = 9;
            var action = this.ControllerContext.RouteData.Values["action"].ToString();
            ViewBag.UserID = userid;
            return PartialView(PaginatedList<SubsistenceModel   >.CreateAsync(cellphone.OrderByDescending(x => x.Year).ThenByDescending(x => x.Month).ToList(), action, page ?? 1, size));
        }

        #endregion

        #region LONGEVITY
        [HttpGet]
        public async Task<IActionResult> GenerateLongevity(string userid, string fname, string mname, string lname)
        {
            StartDate = DateTime.Now;
            var longevity = await PayrollDatabase.Instance.GetLongevityByIdAsync(userid, fname, mname,lname, StartDate.Month, StartDate.Year);

            return PartialView(longevity);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateLongevity(LongevityModel model)
        {
            var errors = ModelState.Values.SelectMany(x => x.Errors);
            if (ModelState.IsValid)
            {
                if (await PayrollDatabase.Instance.UpdateLongevityAsync(model))
                {
                    return PartialView(model);
                }
            }

            ViewBag.Errors = errors;

            return PartialView(model);
        }
        #endregion

        #region LONGEVITY LIST
        public async Task<IActionResult> GetLongevityList(string userid, int? page)
        {
            var longevities = await PayrollDatabase.Instance.GetLongevityListByIdAsync(userid);
            int size = 9;
            var action = this.ControllerContext.RouteData.Values["action"].ToString();
            ViewBag.UserID = userid;
            return PartialView(PaginatedList<LongevityModel>.CreateAsync(longevities.OrderByDescending(x => x.Year).ThenByDescending(x => x.Month).ToList(), action, page ?? 1, size));
        }
        #endregion
    }
}
