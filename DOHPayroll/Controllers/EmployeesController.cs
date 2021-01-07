using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DOHPayroll.Controllers
{
    [Authorize(Policy = "Administrator")]
    public class EmployeesController : Controller
    {
        public IActionResult JobOrder(int index)
        {
            switch (index)
            {
                case 0:
                    {
                        ViewBag.Action = "Job Order - ATM";
                        ViewBag.Type = "ATM";
                        break;
                    }
                case 1:
                    {
                        ViewBag.Action = "Job Order - CASH CARD";
                        ViewBag.Type = "CASH_CARD";
                        break;
                    }
                case 2:
                    {
                        ViewBag.Action = "Job Order - W/O LBP CARD";
                        ViewBag.Type = "NO_CARD";
                        break;
                    }
                case 3:
                    {
                        ViewBag.Action = "Job Order - UNDER VTF";
                        ViewBag.Type = "UNDER_VTF";
                        break;
                    }
                default:
                    {
                        ViewBag.Action = "Job Order - ATM";
                        ViewBag.Type = "ATM";
                        break;
                    }
            }
            return View();
        }
        public IActionResult Regular()
        {
            ViewBag.Type = "ATM";
            return View();
        }
    }
}
