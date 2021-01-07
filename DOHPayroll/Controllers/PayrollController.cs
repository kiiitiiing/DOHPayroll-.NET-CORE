using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DinkToPdf;
using DinkToPdf.Contracts;
using DOHPayroll.Databases;
using DOHPayroll.Resources;
using DOHPayroll.Services;
using DOHPayroll.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DOHPayroll.Controllers
{
    [Authorize(Policy = "Administrator")]
    public class PayrollController : Controller
    {
        private IConverter _converter;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public PayrollController(IConverter converter)
        {
            _converter = converter;
        }

        #region GENERATE GENERAL PAYROLL
        [HttpPost]
        public async Task<IActionResult> GenerateGeneralPayroll(string date, int division)
        {
            GetStartDay(date);
            var model = await PayrollDatabase.Instance.GetRegularPayslipsAsync(StartDate, division);
            if (model.Count() > 0)
            {
                var file = GeneralPayrollHtml(model);

                return File(file, "application/pdf");
            }
            else
            {
                return View("~/Views/Report/NothingToGenerate.cshtml");
            }
        }

        public byte[] GeneralPayrollHtml(List<RegularPayslipModel> payslipModels)
        {
            var pdf = new PDFEz(_converter)
            {
                Top = 0.5
            };
            var date = DateTime.Now.ToString("MMM") + " 1-" + DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
            int headerFontSize = 13;
            int contentFontSize = 12;    

            var table = new Table(4,
                new Row()
                {
                    Columns =
                    {
                        new Column("GENERAL PAYROLL")
                        {
                            FontSize = headerFontSize,
                            Bold = true,
                            ColSpan = 4,
                            TextAlignment = Alignment.CENTER
                        }
                    }
                },
                new Row() 
                {
                    Columns =
                    {
                        new Column("RD/ARD")
                        {
                            FontSize = headerFontSize,
                            Bold = true,
                            ColSpan = 4,
                            TextAlignment = Alignment.CENTER
                        }
                    }
                }, 
                new Row()
                {
                    Columns =
                    {
                        new Column("SUMMARY Sheet")
                        {
                            FontSize = headerFontSize,
                            Bold = true,
                            ColSpan = 4,
                            TextAlignment = Alignment.CENTER
                        }
                    }
                }, 
                new Row()
                {
                    Columns =
                    {
                        new Column("GRAND TOTALS")
                        {
                            FontSize = headerFontSize,
                            Bold = true,
                            ColSpan = 4,
                            TextAlignment = Alignment.CENTER
                        }
                    }
                }, 
                new Row()
                {
                    Columns =
                    {
                        new Column("For month of " + date)
                        {
                            FontSize = headerFontSize - 1,
                            Bold = true,
                            ColSpan = 4,
                            TextAlignment = Alignment.CENTER
                        }
                    }
                },
                new Row(Column.Blank(4)), 
                new Row(Column.Blank(4)),
                new Row()
                {
                    Columns =
                    {
                        new Column("Basic Rate:")
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT 
                        },
                        new Column(payslipModels.Sum(x=>x.Salary.CheckSalaryIfEmpty()).FixDigit())
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        },
                        Column.Blank(2)
                    }
                },
                new Row()
                {
                    Columns =
                    {
                        new Column("Basic Increment:")
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        },
                        new Column("0.00")
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        },
                        new Column("NET PAY FOR")
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        },
                        new Column("HALF 1 : "+(payslipModels.Sum(x=>x.Salary.CheckSalaryIfEmpty())/2).FixDigit())
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        }
                    }
                },
                new Row()
                {
                    Columns =
                    {
                        new Column("Basic Adjustment:")
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        },
                        new Column("0.00")
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        },
                        Column.Blank(),
                        new Column("HALF 2 : "+(payslipModels.Sum(x=>x.Salary.CheckSalaryIfEmpty())/2).FixDigit())
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        }
                    }
                },
                new Row()
                {
                    Columns =
                    {
                        new Column("PERA:")
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        },
                        new Column(payslipModels.Sum(x=>x.FirstHalf.RegularPayroll.PERA).FixDigit())
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        },
                        Column.Blank(2)
                    }
                },
                new Row()
                {
                    Columns =
                    {
                        new Column("Laundry Allowances:")
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        },
                        new Column(payslipModels.Sum(x=>x.FirstHalf.Subsistence.Laundry).FixDigit())
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        },
                        Column.Blank(2)
                    }
                },
                new Row()
                {
                    Columns =
                    {
                        new Column("Subsistence:")
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        },
                        new Column(payslipModels.Sum(x=>x.FirstHalf.Subsistence.Subsistence).FixDigit())
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        },
                        Column.Blank(2)
                    }
                },
                new Row()
                {
                    Columns =
                    {
                        new Column("RA:")
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        },
                        new Column(payslipModels.Sum(x=>x.SecondHalf.Rata.RA).FixDigit())
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        },
                        Column.Blank(2)
                    }
                },
                new Row()
                {
                    Columns =
                    {
                        new Column("TA:")
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        },
                        new Column(payslipModels.Sum(x=>x.SecondHalf.Rata.TA).FixDigit())
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        },
                        Column.Blank(2)
                    }
                },
                new Row()
                {
                    Columns =
                    {
                        new Column("Hazard Pay:")
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        },
                        new Column(payslipModels.Sum(x=>x.SecondHalf.Hazard.HazardPay).FixDigit())
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT,
                            Borders = {Borders.Bottom},
                            BorderThickness = 3
                        },
                        Column.Blank(2)
                    }
                },
                new Row()
                {
                    Columns =
                    {
                        new Column("GROSS:")
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT,
                            Bold = true,
                        },
                        new Column(
                            (payslipModels.Sum(x=>x.SecondHalf.Hazard.HazardPay) +
                            payslipModels.Sum(x=>x.FirstHalf.Subsistence.Laundry) +
                            payslipModels.Sum(x=>x.FirstHalf.Subsistence.Subsistence) +
                            payslipModels.Sum(x=>x.FirstHalf.RegularPayroll.PERA) +
                            payslipModels.Sum(x=>x.SecondHalf.Rata.RA) +
                            payslipModels.Sum(x=>x.SecondHalf.Rata.TA) +
                            payslipModels.Sum(x=>x.Salary.CheckSalaryIfEmpty())).FixDigit())
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        },
                        Column.Blank(2)
                    }
                },
                new Row(Column.Blank(4)),
                new Row()
                {
                    Columns =
                    {
                        new Column("(--)DEDUCTIONS:")
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT,
                            Bold = true,
                        },
                        Column.Blank(3)
                    }
                },
                new Row(Column.Blank(4)),
                new Row()
                {
                    Columns =
                    {
                        new Column("Withholding Tax:")
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        },
                        new Column(payslipModels.Sum(x=>x.FirstHalf.RegularPayroll.ProfessionalTax).FixDigit())
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        },
                        new Column("PhilHealth (Medicare):")
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        },
                        new Column(payslipModels.Sum(x=>x.FirstHalf.RegularPayroll.Phic).FixDigit())
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        }
                    }
                },
                new Row()
                {
                    Columns =
                    {
                        new Column("PAGIBIG FUND:")
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        },
                        new Column(payslipModels.Sum(x=>x.FirstHalf.RegularPayroll.PagibigPremium).FixDigit())
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        },
                        new Column("PAGIBIG Multi-Purpose:")
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        },
                        new Column(payslipModels.Sum(x=>x.FirstHalf.RegularPayroll.PagibigLoan.Item).FixDigit())
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        }
                    }
                },
                new Row()
                {
                    Columns =
                    {
                        new Column("PAGIBIG MP2:")
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        },
                        new Column(payslipModels.Sum(x=>x.FirstHalf.RegularPayroll.PagibigMP2.Item).FixDigit())
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        },
                        new Column("PAGIBIG Calamity Loan:")
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        },
                        new Column(payslipModels.Sum(x=>x.FirstHalf.RegularPayroll.PagibigCalimity.Item).FixDigit())
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        }
                    }
                },
                new Row()
                {
                    Columns =
                    {
                        new Column("GSIS Life_Retr. Insurance:")
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        },
                        new Column(payslipModels.Sum(x=>x.FirstHalf.RegularPayroll.GSIS_Premium).FixDigit())
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        },
                        new Column("GSIS CONSO LOAN:")
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        },
                        new Column(payslipModels.Sum(x=>x.FirstHalf.RegularPayroll.GSIS_Consoloan.Item).FixDigit())
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        }
                    }
                },
                new Row()
                {
                    Columns =
                    {
                        new Column("GSIS Policy Loan:")
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        },
                        new Column(payslipModels.Sum(x=>x.FirstHalf.RegularPayroll.GSIS_PolicyLoan.Item).FixDigit())
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        },
                        new Column("GSIS COMP LOAN:")
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        },
                        new Column(payslipModels.Sum(x=>x.FirstHalf.RegularPayroll.GSIS_COMP.Item).FixDigit())
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        }
                    }
                },
                new Row()
                {
                    Columns =
                    {
                        new Column("GSIS GFAL:")
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        },
                        new Column(payslipModels.Sum(x=>x.FirstHalf.RegularPayroll.GSIS_GFAL.Item).FixDigit())
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        },
                        new Column("GSIS HELP:")
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        },
                        new Column(payslipModels.Sum(x=>x.FirstHalf.RegularPayroll.GSIS_Help.Item).FixDigit())
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        }
                    }
                },
                new Row()
                {
                    Columns =
                    {
                        new Column("GSIS EDU:")
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        },
                        new Column(payslipModels.Sum(x=>x.FirstHalf.RegularPayroll.GSIS_EDU.Item).FixDigit())
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        },
                        new Column("GSIS EML:")
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        },
                        new Column(payslipModels.Sum(x=>x.FirstHalf.RegularPayroll.GSIS_EML.Item).FixDigit())
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        }
                    }
                },
                new Row()
                {
                    Columns =
                    {
                        new Column("GSIS UOLI:")
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        },
                        new Column(payslipModels.Sum(x=>x.FirstHalf.RegularPayroll.GSIS_UOLI.Item).FixDigit())
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        },
                        new Column("DBP Loan:")
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        },
                        new Column(payslipModels.Sum(x=>x.FirstHalf.RegularPayroll.DBP.Item).FixDigit())
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        }
                    }
                },
                new Row()
                {
                    Columns =
                    {
                        new Column("SIMC Loan:")
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        },
                        new Column(payslipModels.Sum(x=>x.FirstHalf.RegularPayroll.SIMC.Item).FixDigit())
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        },
                        new Column("HWMPC Loan:")
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        },
                        new Column(payslipModels.Sum(x=>x.FirstHalf.RegularPayroll.HWMPC.Item).FixDigit())
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        }
                    }
                },
                new Row()
                {
                    Columns =
                    {
                        new Column("CFI Loan:")
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT,
                            Borders = {Borders.Bottom},
                            BorderThickness = 3
                        },
                        new Column(payslipModels.Sum(x=>x.FirstHalf.RegularPayroll.CFI.Item).FixDigit())
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT,
                            Borders = {Borders.Bottom},
                            BorderThickness = 3
                        },
                        new Column("")
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT,
                            Borders = {Borders.Bottom},
                            BorderThickness = 3
                        },
                        new Column("")
                        {
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT,
                            Borders = {Borders.Bottom},
                            BorderThickness = 3
                        }
                    }
                },
                new Row()
                {
                    Columns =
                    {
                        Column.Blank(2),
                        new Column("Total Deduction")
                        {
                            Bold = true,
                            PaddingTop = 10,
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        },
                        new Column("0.00")
                        {
                            Bold = true,
                            PaddingTop = 3,
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        }
                    }
                },
                new Row(Column.Blank(4)),
                new Row()
                {
                    Columns =
                    {
                        Column.Blank(2),
                        new Column("Total Net")
                        {
                            Bold = true,
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        },
                        new Column("0.00")
                        {
                            Bold = true,
                            FontSize = contentFontSize,
                            TextAlignment = Alignment.RIGHT
                        }
                    }
                }
            );



            pdf.AddTable(table);
            var file = pdf.GeneratePdf();

            return file;
        }
        #endregion

        #region GENERATE ALL RATA
        /*public async Task<bool> GenerateRata(string date)
        {
            GetStartDay(date);

        }*/
        #endregion

        #region GENERATE SUBSISTENCE ON DB
        public async Task<bool> GenerateEmployeeSubsistence(string date, double subsistence_allowance, double laundry_allowance)
        {
            GetStartDay(date);
            var users = await PISDatabase.Instance.GetRegularsByDisbursementType("ATM");
            int size = 8;
            users = users.Where(x => !string.IsNullOrEmpty(x.Salary)).ToList();
            var listEmployees = new List<List<Employee>>();
            if (users.Count() >= size)
            {
                var listCtr = users.Count() / size;
                for (int x = 0; x <= size; x++)
                {
                    var listEmployee = users.Skip(x * listCtr).Take(listCtr).ToList();
                    listEmployees.Add(listEmployee);
                }
                var subsistences = new List<SubsistenceModel>();

                var tasks = new List<Task<List<SubsistenceModel>>>();

                foreach (var listEmployee in listEmployees)
                {
                    tasks.Add(Task.Run(() => GetSubsistenceModels(listEmployee, date, subsistence_allowance, laundry_allowance)));
                }

                foreach (var task in tasks)
                {
                    subsistences.AddRange(await task);
                }

                if (await PayrollDatabase.Instance.SaveSubsistencesAsync(subsistences))
                    return true;
                else
                    return false;
            }
            else
            {
                var subsistences = GetSubsistenceModels(users, date, subsistence_allowance, laundry_allowance);

                if (await PayrollDatabase.Instance.SaveSubsistencesAsync(subsistences))
                    return true;
                else
                    return false;
            }
        }
        #endregion

        #region GENERATE REPORT PAYROLL JOB ORDER
        [HttpPost]
        public async Task<bool> GenerateReportJo(string date, int range, string type)
        {
            GetStartDay(date, range.ToString());
            var users = await PISDatabase.Instance.GetJobOrdersByDisbursementType(type);
            int size = 8;
            users = users.Where(x => !string.IsNullOrEmpty(x.Salary)).ToList();
            var listEmployees = new List<List<Employee>>();
            if (users.Count() >= size)
            {
                var listCtr = users.Count() / size;
                for (int x = 0; x <= size; x++)
                {
                    var listEmployee = users.Skip(x * listCtr).Take(listCtr).ToList();
                    listEmployees.Add(listEmployee);
                }
                var payrolls = new List<JoPayrollModel>();

                var tasks = new List<Task<List<JoPayrollModel>>>();

                foreach(var listEmployee in listEmployees)
                {
                    tasks.Add(Task.Run(() => GetJoPayroll(listEmployee, date, range)));
                }

                foreach(var task in tasks)
                {
                    payrolls.AddRange(await task);
                }

                if (await PayrollDatabase.Instance.SaveJoPayrollsAsync(payrolls, date, range.ToString()))
                    return true;
                else
                    return false;
            }
            else
            {
                var payrolls = GetJoPayroll(users, date, range);

                if (await PayrollDatabase.Instance.SaveJoPayrollsAsync(payrolls, date, range.ToString()))
                    return true;
                else
                    return false;
            }
        }
        #endregion

        #region GENERATE REPORT PAYROLL REGULAR
        [HttpPost]
        public async Task<bool> GenerateReportRegular(string date)
        {
            GetStartDay(date);
            var users = await PISDatabase.Instance.GetRegularsByDisbursementType("ATM");
            int size = 8;
            users = users.Where(x => !string.IsNullOrEmpty(x.Salary)).ToList();
            var listEmployees = new List<List<Employee>>();
            if (users.Count() >= size)
            {
                var listCtr = users.Count() / size;
                for (int x = 0; x <= size; x++)
                {
                    var listEmployee = users.Skip(x * listCtr).Take(listCtr).ToList();
                    listEmployees.Add(listEmployee);
                }
                var payrolls = new List<RegularPayrollModel>();

                var tasks = new List<Task<List<RegularPayrollModel>>>();

                foreach (var listEmployee in listEmployees)
                {
                    tasks.Add(Task.Run(() => GetRegularPayroll(listEmployee, date)));
                }

                foreach (var task in tasks)
                {
                    payrolls.AddRange(await task);
                }

                if (await PayrollDatabase.Instance.SaveRegularPayrollsAsync(payrolls))
                    return true;
                else
                    return false;
            }
            else
            {
                var payrolls = GetRegularPayroll(users, date);

                if (await PayrollDatabase.Instance.SaveRegularPayrollsAsync(payrolls))
                    return true;
                else
                    return false;
            }
        }
        #endregion

        #region GENERATE REGULAR PAYROLL
        [HttpPost]
        public async Task<IActionResult> GenerateRegularPayroll(string date, string salary_charge)
        {
            GetStartDay(date);
            var model = await PayrollDatabase.Instance.GetRegularPayrollListByDate(StartDate.Month, StartDate.Year, salary_charge);
            if(model.Count() > 0)
            {
                var content = RegularPayrollHtml(model);
                var file = SetPDF(content, "Regular Payroll for " + salary_charge); 

                return File(file, "application/pdf");
            }
            else
            {
                return View("~/Views/Report/NothingToGenerate.cshtml");
            }
        }

        public string RegularPayrollHtml(List<RegularPayrollModel> models)
        {
            var pdf = new StringBuilder();
            pdf.Append(HtmlStart());
            if(models.Count() > 0)
            {
                var ctr = 1;
                int size = 5;
                foreach (var limitPayrolls in models.GroupBy(x=>x.SalaryCharge).OrderBy(x=>x.Key))
                {
                    for (int x = 0; x <= limitPayrolls.Count() / size; x++)
                    {
                        var limitPayroll = models.Skip(x * size).Take(size);
                        //HEADER
                        pdf.AppendFormat(@"
                            <h3 class='align-center'>
                                GENERAL PAYROLL
                            </h3>
                            <p>
                                <span class='align-left bold font-13'>
                                    REGIONAL DIRECTOR/ADMINISTRATIVE REGIONAL DIRECTOR
                                </span>
                                <span class='font-12' style='float: right; white-space: nowrap;'>
                                    Journal Voucher No. ______________
                                </span>
                                <br>
                                <span class='align-left font-13'>
                                    WE HEREBY ACKNOWLEDGE to have received from the DOH - RO7, Osmeña Blvd, Cebu City, the sums therein specified opposite our respective names,
                                </span><br>
                                <span class='align-left font-13'>
                                    being in full compensation for our services for the period {0} {1}-{2}, {3}, except as noted otherwise in the Remarks columns.
                                </span>
                            </p>
                        ",
                        StartDate.ToString("MMM"), StartDate.Day, EndDate.Day, StartDate.Year);
                        //table start
                        pdf.AppendFormat(@"
                            <table class='arial table font-11 border-collapse regular'>
                                <tbody>
                        ");
                        //table header
                        pdf.AppendFormat(@"
                            <tr>
                                <td colspan='25'>
              
                                </td>
                            </tr>
                            <tr>
                                <td class='border'>
                                </td>
                                <td class='border'>
                                </td>
                                <td class='border'>
                                </td>
                                <td class='border align-center bold font-13' colspan='3'>
                                    SALARY
                                </td>
                                <td class='border align-center bold font-13' colspan='13'>
                                    DEDUCTIONS
                                </td>
                                <td class='border'>
                                </td>
                                <td class='border' colspan='2'>
                                </td>
                                <td class='border'>
                                </td>
                                <td class='border'>
                                </td>
                                <td class='border'>
                                </td>
                            </tr>
                            <tr>
                                <td class='align-center border bold font-12'>
                                    NO.
                                </td>
                                <td class='align-center border bold font-12'>
                                    NAME
                                </td>
                                <td class='align-center border bold font-12'>
                                    DESIGNATION
                                </td>
                                <td class='align-center border bold font-12'>
                                    BASIC SALARY
                                </td>
                                <td class='align-center border bold font-12'>
                                    PERA
                                </td>
                                <td class='align-center border bold font-12'>
                                    GROSS<br>
                                    INCOME<br>
                                </td>
                                <td class='align-center border bold font-12'>
                                    WTAX/<br>
                                    PHIC
                                </td>
                                <td class='align-center border bold font-12' colspan='6'>
                                    GSIS
                                </td>
                                <td class='align-center border bold font-12' colspan='2'>
                                    PAGIBIG
                                </td>
                                <td class='align-center border bold font-12' colspan='4'>
                                    OTHERS
                                </td>
                                <td class='align-center border bold font-12'>
                                    TOTAL<br>
                                    DEDUCTIONS
                                </td>
                                <td class='align-center border bold font-12' colspan='2'>
                                    NET AMOUNT<br>
                                    RECEIVED
                                </td>
                                <td class='align-cente border bold font-12'>
                                    SIGNATURE
                                </td>
                                <td class='rotate bold font-12 border' style='padding: 9px; width: 8px !important;'>
                                    INITIAL
                                </td>
                                <td class='align-center border bold font-12'>
                                    REMARKS
                                </td>
                            </tr>
                            <tr>
                                <td class='left bottom right vertical-align-middle align center bold font-15' colspan='25'>
                                    {0}
                                </td>
                            </tr>
                        ", limitPayrolls.Key);
                        //employee row
                        foreach (var payroll in limitPayroll)
                        {
                            pdf.AppendFormat(@"
                                <!-- EMPLOYEES -->
                                <!-- 1ST ROW -->
                                <tr>
                                <td class='align-center border vertical-align-top font-12' rowspan='4'>
                                    {0} 
                                </td>
                                <td class='align-left border vertical-align-top font-12 now-wrap' rowspan='4'>
                                    {1}
                                </td>
                                <td class='align-left border vertical-align-top font-12 now-wrap' rowspan='4'>
                                    {2}
                                </td>
                                <td class='align-right left vertical-align-top font-11 now-wrap'>
                                    {3}
                                </td>
                                <td class='align-right left font-11 now-wrap'>
                                    {4}
                                </td>
                                <td class='align-right vertical-align-top left bottom font-11 now-wrap' rowspan='4'>
                                    {5}
                                </td>
                                <td class='align-right vertical-align-top border font-11 now-wrap' rowspan='2'>
                                    {6}
                                </td>
                                <td class='align-left left font-11 now-wrap'>
                                    PRM
                                </td>
                                <td class='align-right font-11 now-wrap'>
                                    {7}
                                </td>
                                <td class='align-left left font-11 now-wrap'>
                                    POL
                                </td>
                                <td class='align-right font-11 now-wrap'>
                                    {8}
                                </td>
                                <td class='align-left left font-11 now-wrap'>
                                    REL
                                </td>
                                <td class='align-right font-11 now-wrap'>
                                    {9}
                                </td>
                                <td class='align-left left font-11 now-wrap'>
                                    PRM
                                </td>
                                <td class='align-right font-11 now-wrap'>
                                    {10}
                                </td>
                                <td class='align-left left font-11 now-wrap'>
                                    CFI
                                </td>
                                <td class='align-right font-11 now-wrap'>
                                    {11}
                                </td>
                                <td class='align-left left font-11 now-wrap'>
                                    HWMPC
                                </td>
                                <td class='align-right font-11 now-wrap'>
                                    {12}
                                </td>
                                <td class='align-right border vertical-align-top font-11 now-wrap' rowspan='4'>
                                    {13}
                                </td>
                                <td class='align-right left vertical-align-top font-11 now-wrap' rowspan='2'>
                                    {14}
                                </td>
                                <td class='align-center vertical-align-middle bottom font-10 now-wrap'>
                                    15
                                </td>
                                <td class='align-right left bottom font-11 now-wrap' rowspan='4'>
              
                                </td>
                                <td class='align-right left bottom font-11 now-wrap' rowspan='4'>
              
                                </td>
                                <td class='align-left border vertical-align-bottom bottom font-9 now-wrap' rowspan='4'>
                                    {15}<br>
                                    {16} - (No Picture)
                                </td>
                                </tr>
                                <!-- 2ND ROW-->
                                <tr>
                                <td class='align-right left font-11 now-wrap'>
                                    ({17})
                                </td>
                                <td class='align-right vertical-align-top left bottom font-11 now-wrap' rowspan='3'>
                                    {18}
                                </td>
                                <td class='align-left left font-11 now-wrap'>
                                    CON
                                </td>
                                <td class='align-right font-11 now-wrap'>
                                    {19}
                                </td>
                                <td class='align-left left vertical-align-top font-11 now-wrap'>
                                    EML
                                </td>
                                <td class='align-right vertical-align-top font-11 now-wrap'>
                                    {20}
                                </td>
                                <td class='align-left vertical-align-top left font-11 now-wrap'>
                                    EDU
                                </td>
                                <td class='align-right vertical-align-top font-11 now-wrap'>
                                    {21}
                                </td>
                                <td class='align-left left font-11 now-wrap'>
                                    MPL
                                </td>
                                <td class='align-right font-11 now-wrap'>
                                    {22}
                                </td>
                                <td class='align-left left font-11 now-wrap'>
                                    SIMC
                                </td>
                                <td class='align-right font-11 now-wrap'>
                                    {23}
                                </td>
                                <td class='align-left left font-11 now-wrap'>
                                    DIS
                                </td>
                                <td class='align-right font-11 now-wrap'>
                                    {24}
                                </td>
                                </tr>
                                <!-- 3RD ROW -->
                                <tr>
                                <td class='align-right left bottom vertical-align-top font-11 now-wrap' rowspan='2'>
                                    {25}
                                </td>
                                <td class='align-right border vertical-align-top font-11 now-wrap' rowspan='2'>
                                    {26}
                                </td>
                                <td class='align-left vertical-align-top left bottom font-11 now-wrap' rowspan='2'>
                                    HLP
                                </td>
                                <td class='align-right bottom vertical-align-top font-11 now-wrap' rowspan='2'>
                                    {27}
                                </td>
                                <td class='align-left vertical-align-top left bottom font-11 now-wrap' rowspan='2'>
                                    GFAL
                                </td>
                                <td class='align-right bottom vertical-align-top font-11 now-wrap' rowspan='2'>
                                    {28}
                                </td>
                                <td class='align-left vertical-align-top left bottom font-11 now-wrap' rowspan='2'>
                                    COMP
                                </td>
                                <td class='align-right bottom vertical-align-top font-11 now-wrap' rowspan='2'>
                                    {29}
                                </td>
                                <td class='align-left left font-11 now-wrap'>
                                    MP2
                                </td>
                                <td class='align-right font-11 now-wrap'>
                                    {30}
                                </td>
                                <td class='align-left vertical-align-top left bottom font-11 now-wrap' rowspan='2'>
                                    REL
                                </td>
                                <td class='align-right vertical-align-top bottom font-11 now-wrap' rowspan='2'>
                                    0.00
                                </td>
                                <td class='align-left left bottom vertical-align-top font-11 now-wrap' rowspan='2'>
                                    DBP
                                </td>
                                <td class='align-right bottom vertical-align-top font-11 now-wrap' rowspan='2'>
                                    {31}
                                </td>
                                <td class='align-right bottom left vertical-align-top font-11 now-wrap' rowspan='2'>
                                    {32}
                                </td>
                                <td class='align-center vertical-align-middle bottom font-10 now-wrap'>
                                    {33}
                                </td>
                                </tr>
                                <!-- 4TH ROW -->
                                <tr>
                                <td class='align-center left bottom vertical-align-top font-11 now-wrap'>
                                    CAL
                                </td>
                                <td class='align-right bottom vertical-align-top font-11 now-wrap'>
                                    {34}
                                </td>
                                <td class='align-center bottom font-9 now-wrap'>
              
                                </td>
                                </tr>
                                ",
                            ctr, payroll.Fullname, payroll.Designation, payroll.Salary.FormalDigit(),
                            payroll.PERA.FormalDigit(), payroll.GrossIncome.FormalDigit(), payroll.ProfessionalTax.FormalDigit(),
                            payroll.GSIS_Premium.FormalDigit(), payroll.GSIS_PolicyLoan.Item.FormalDigit(), payroll.GSIS_Rel.Item.FormalDigit(),
                            payroll.PagibigPremium.FormalDigit(), payroll.CFI.Item.FormalDigit(), payroll.HWMPC.Item.FormalDigit(),
                            payroll.TotalDeductions.FormalDigit(), (payroll.NetPay / 2).FormalDigit(), payroll.Fullname, payroll.ID,
                            payroll.Deduction.FormalDigit(), "", payroll.GSIS_Consoloan.Item.FormalDigit(),
                            payroll.GSIS_EML.Item.FormalDigit(), payroll.GSIS_EDU.Item.FormalDigit(), payroll.PagibigLoan.Item.FormalDigit(),
                            payroll.SIMC.Item.FormalDigit(), payroll.Disallowances.Item.FormalDigit(), "",
                            payroll.Phic.FormalDigit(), payroll.GSIS_Help.Item.FormalDigit(), payroll.GSIS_GFAL.Item.FormalDigit(),
                            payroll.GSIS_COMP.Item.FormalDigit(), payroll.PagibigMP2.Item.FormalDigit(),
                            payroll.DBP.Item.FormalDigit(), (payroll.NetPay / 2).FormalDigit(), DateTime.DaysInMonth(payroll.Year, payroll.Month),
                            payroll.PagibigCalimity.Item.FormalDigit()
                            );
                            ctr++;
                        }
                        //page total
                        pdf.AppendFormat(@"
                            <tr>
                            <td class='align-right bold vertical-align-middle border font-12 now-wrap' colspan='3'>
                                TOTAL THIS PAGE >>>
                            </td>
                            <td class='align-right vertical-align-middle border font-10 now-wrap'>
                                {0}<br>
                                ({1})
                            </td>
                            <td class='align-right vertical-align-middle border font-10 now-wrap'>
                                {2}{3}
                            </td>
                            <td class='align-right vertical-align-middle border font-10 now-wrap'>
                                {4}
                            </td>
                            <td class='align-right vertical-align-middle border font-10 now-wrap'>
                                {5}<br>
                                {6}
                            </td>
                            <td class='align-right vertical-align-middle border font-10 now-wrap' colspan='6'>
                                {7}
                            </td>
                            <td class='align-right vertical-align-middle border font-10 now-wrap' colspan='2'>
                                {8}
                            </td>
                            <td class='align-right vertical-align-middle border font-10 now-wrap' colspan='4'>
                                {9}
                            </td>
                            <td class='align-right vertical-align-middle border font-10 now-wrap'>
                                {10}
                            </td>
                            <td class='align-right vertical-align-middle border font-10 now-wrap' colspan='2'>
                                {11}<br>
                                {12}
                            </td>
                            <td class='align-right vertical-align-middle border font-10 now-wrap' colspan='3'>
                            </td>
                            </tr>
                            ",
                        limitPayroll.Sum(x => double.Parse(x.Salary)).FormalDigit(),
                        limitPayroll.Sum(x => x.Deduction).FormalDigit(),
                        limitPayroll.Sum(x => x.PERA).FormalDigit(),
                        "",
                        limitPayroll.Sum(x => x.GrossIncome).FormalDigit(),
                        limitPayroll.Sum(x => x.ProfessionalTax).FormalDigit(),
                        limitPayroll.Sum(x => x.Phic).FormalDigit(),
                        limitPayroll.Sum(x => x.TotalGsis).FormalDigit(),
                        limitPayroll.Sum(x => x.TotalPagibig).FormalDigit(),
                        limitPayroll.Sum(x => x.TotalOthers).FormalDigit(),
                        limitPayroll.Sum(x => x.TotalDeductions).FormalDigit(),
                        limitPayroll.Sum(x => x.NetPay / 2).FormalDigit(),
                        limitPayroll.Sum(x => x.NetPay / 2).FormalDigit()
                        );
                        //table end
                        pdf.AppendFormat(@"
                        </tbody>
                        </table>
                    ");
                        if (x != ((models.Count() / size)))
                        {
                            pdf.AppendFormat("<div style='page-break-after: always;'></div>");
                        }
                    }
                }
            }
            pdf.Append(HtmlEnd());

            return pdf.ToString();
        }

        #endregion

        #region GENERATE JOB ORDER PAYROLL
        [HttpPost]
        public async Task<IActionResult> GenerateJobOrderPayroll(string date, int range, string disbursement, string salary_charge)
        {
            var model = await PayrollDatabase.Instance.GetJoPayrollByDate(date, range, disbursement, salary_charge);
            if (model.Count() > 0)
            {
                var content = JobOrderPayrollHtml(model);
                var file = SetPDF(content, salary_charge + "Job Order Payroll");

                return File(file, "application/pdf");
            }
            else
            {
                return View("~/Views/Report/NothingToGenerate.cshtml");
            }
        }

        public string JobOrderPayrollHtml(List<JoPayrollModel> models)
        {
            var pdf = new StringBuilder();
            pdf.Append(HtmlStart());
            if (models.Count() > 0)
            {
                var ctr = 0;
                int size = 20;
                var month = models.First().StartDate.ToString("MMM");
                var year = models.First().StartDate.Year;
                var start = models.First().StartDate.Day;
                var end = models.First().EndDate.Day;
                var date = month + " " + start + "-" + end + ", " + year;

                foreach(var limitPayrolls in models.GroupBy(x=>x.SalaryCharge).OrderBy(x=>x.Key))
                {
                    for (int x = 0; x <= limitPayrolls.Count() / size; x++)
                    {
                        var limitPayroll = limitPayrolls.Skip(x * size).Take(size);
                        //HEADER
                        pdf.AppendFormat(@"
                        <h3 class='align-center'>
                            PAYROLL FOR JOB PERSONNEL<br>
                            DOH-RO7<br>
                            {0}
                        </h3><br>
                        <span class='align-left bold font-13'>
                            We acknowledge receipt of the sum shown opposite our names as full renumeration for the period started:
                        </span>
                        <span class='font-12' style='float: right; white-space: nowrap;'>
                            *NO WORK NO PAY POLICY*
                        </span>
                        </p>
                    ", date);
                        //table start
                        pdf.AppendFormat(@"
                        <table class='arial table font-10 border-collapse regular'>
                            <tbody>");
                        //table header
                        pdf.AppendFormat(@"
                        <tr>
                            <td colspan='19'>
                            </td>
                        </tr>
                        <tr>
                            <td class='vertical-align-middle align-center border bold font-12' rowspan='2' >
                                TIN
                            </td>
                            <td class='vertical-align-middle align-center border bold font-12' colspan='2' rowspan='2'>
                                NAME
                            </td>
                            <td class='vertical-align-middle align-center border bold font-12' rowspan='2'>
                                POSITION
                            </td>
                            <td class='vertical-align-middle align-center border bold font-12' rowspan='2'>
                                MO. RATE
                            </td>
                            <td class='vertical-align-middle align-center border bold font-12' rowspan='2'>
                                HALF MO.
                            </td>
                            <td class='vertical-align-middle align-center border bold font-12' rowspan='2'>
                                ADJUSTMENT
                            </td>
                            <td class='vertical-align-middle align-center border bold font-12' rowspan='2'>
                                TARDINESS<br>
                                ABSENCES
                            </td>
                            <td class='vertical-align-middle align-center border bold font-12' rowspan='2'>
                                GROSS<br>
                                AMOUNT
                            </td>
                            <td class='vertical-align-middle align-center border bold font-12'>
                                ADJUSTMENT
                            </td>
                            <td class='vertical-align-middle align-center border bold font-12' colspan='7'>
                                DEDUCTIONS
                            </td>
                            <td class='vertical-align-middle align-center border bold font-12' rowspan='2'>
                                NET AMT.
                            </td>
                            <td class='vertical-align-middle align-center border bold font-12' rowspan='2'>
                                REMARKS
                            </td>
                            </tr>
                            <tr>
                            <td class='vertical-align-middle align-center border bold font-11'>
                                Add:
                            </td>
                            <td class='vertical-align-middle align-center border bold font-11'>
                                5% EWT
                            </td>
                            <td class='vertical-align-middle align-center border bold font-11'>
                                3% Prof.
                            </td>
                            <td class='vertical-align-middle align-center border bold font-11'>
                                Phic
                            </td>
                            <td class='vertical-align-middle align-center border bold font-11'>
                                HWMPC
                            </td>
                            <td class='vertical-align-middle align-center border bold font-11'>
                                Pagibig
                            </td>
                            <td class='vertical-align-middle align-center border bold font-11'>
                                GSIS
                            </td>
                            <td class='vertical-align-middle align-center border bold font-11'>
                                Excess<br>
                                Mobile
                            </td>
                        </tr>");
                        //salary charge
                        pdf.AppendFormat(@"
                        <!-- 1ST ROW -->
                        <tr>
                            <td class='vertical-align-middle align-center border bold font-13' colspan='19'>
                                {0}
                            </td>
                        </tr>", limitPayrolls.Key);
                        //employee row
                        foreach (var payroll in limitPayroll)
                        {
                            pdf.AppendFormat(@"
                            <!-- EMPLOYEES -->
                            <!-- 2ND ROW-->
                            <tr>
                                <td class='align-left left bottom font-10 now-wrap'>
                                    {0}
                                </td>
                                <td class='align-left left bottom font-10 now-wrap'>
                                    {1}
                                </td>
                                <td class='align-left left bottom font-10 now-wrap'>
                                    {2}
                                </td>
                                <td class='align-left left bottom font-10 now-wrap'>
                                    {3}
                                </td>
                                <!-- SALARY -->
                                <td class='align-right left bottom font-10 now-wrap'>
                                    {4}
                                </td>
                                <!-- HALF SALARY -->
                                <td class='align-right left bottom font-10 now-wrap'>
                                    {5}
                                </td>
                                <!-- ADJUSTMENTS -->
                                <td class='align-right left bottom font-10 now-wrap'>
                                    {6}
                                </td>
                                <!-- LATES AND ABSENCES -->
                                <td class='align-right left bottom font-10 now-wrap'>
                                    {7}
                                </td>
                                <!-- GROSS INCOME -->
                                <td class='align-right left bottom font-10 now-wrap'>
                                    {8}
                                </td>
                                <!-- ADJUSTMENT ADD: -->
                                <td class='align-right left bottom font-10 now-wrap'>
                                    {9}
                                </td>
                                <!-- EWT -->
                                <td class='align-right left bottom font-10 now-wrap'>
                                    {10}
                                </td>
                                <!-- PROF TAX -->
                                <td class='align-right left bottom font-10 now-wrap'>
                                    {11}
                                </td>
                                <!-- HWMPC -->
                                <td class='align-right left bottom font-10 now-wrap'>
                                    {12}
                                </td>
                                <!-- PAGIBIG -->
                                <td class='align-right left bottom font-10 now-wrap'>
                                    {13}
                                </td>
                                <!-- PHIC -->
                                <td class='align-right left bottom font-10 now-wrap'>
                                    {14}
                                </td>
                                <!-- GSIS -->
                                <td class='align-right left bottom font-10 now-wrap'>
                                    {15}
                                </td>
                                <!-- EXCESS MOBILE -->
                                <td class='align-right left bottom font-10 now-wrap'>
                                    {16}
                                </td>
                                <!-- NET AMOUNT -->
                                <td class='align-right left bottom font-10 now-wrap'>
                                    {17}
                                </td>
                                <!-- REMARKS -->
                                <td class='align-center left bottom right font-10 now-wrap'>
                                    {18}
                                </td>
                            </tr>", payroll.Tin, payroll.Lname.ToUpper(), payroll.Fname.ToUpper(), payroll.Designation, payroll.Salary.FormalDigit(),
                                (double.Parse(payroll.Salary) / 2).FormalDigit(), payroll.Adjustment.FormalDigit(),
                                payroll.Deduction.FormalDigit(), payroll.NetAmount.FormalDigit(), payroll.OtherAdjustments.FormalDigit(),
                                payroll.EWT.FormalDigit(), payroll.ProfessionalTax.FormalDigit(), payroll.HWMPC.Item.FormalDigit(),
                                payroll.Pagibig.Item.FormalDigit(), payroll.PHIC.Item.FormalDigit(), payroll.GSIS.Item.FormalDigit(),
                                0.00.FormalDigit(), payroll.NetPay.FormalDigit(), payroll.Remarks);
                            ctr++;
                        }
                        //page total
                        pdf.AppendFormat(@"
                        <!-- PAGE TOTAL -->
                        <tr>
                            <td class='align-center vertical-align-middle bold left bottom font-11 now-wrap' colspan='4'>
                                PAGE TOTAL
                            </td>
                            <!-- MONTH RATE TOTAL -->
                            <td class='align-right left bold bottom font-11 now-wrap'>
                                {0}
                            </td>
                            <!-- HALF MONTH TOTAL -->
                            <td class='align-right left bold bottom font-11 now-wrap'>
                                {1}
                            </td>
                            <!-- ADJUSTMENT TOTAL -->
                            <td class='align-right left bold bottom font-11 now-wrap'>
                                {2}
                            </td>
                            <!-- LATES AND ABSENCES TOTAL -->
                            <td class='align-right left bold bottom font-11 now-wrap'>
                                {3}
                            </td>
                            <!-- GROSS AMOUNT TOTAL -->
                            <td class='align-right left bold bottom font-11 font-red now-wrap'>
                                {4}
                            </td>
                            <!-- ADJUSTMENT ADD TOTAL -->
                            <td class='align-right left bold bottom font-11 now-wrap'>
                                {5}
                            </td>
                            <!-- EWT TOTAL -->
                            <td class='align-right left bold bottom font-11 now-wrap'>
                                {6}
                            </td>
                            <!-- PROF TAX TOTAL -->
                            <td class='align-right left bold bottom font-11 now-wrap'>
                                {7}
                            </td>
                            <!-- HWMPC TOTAL -->
                            <td class='align-right left bold bottom font-11 now-wrap'>
                                {8}
                            </td>
                            <!-- PAGIBIG TOTAL -->
                            <td class='align-right left bold bottom font-11 now-wrap'>
                                {9}
                            </td>
                            <!-- PHIC TOTAL -->
                            <td class='align-right left bold bottom font-11 now-wrap'>
                                {10}
                            </td>
                            <!-- GSIS TOTAL -->
                            <td class='align-right left bold bottom font-11 now-wrap'>
                                {11}
                            </td>
                            <!-- EXCESS MOBILE TOTAL -->
                            <td class='align-right left bold bottom font-11 now-wrap'>
                                {12}
                            </td>
                            <!-- NET AMOUNT TOTAL -->
                            <td class='align-right left bold bottom font-red font-11 now-wrap'>
                                {13}
                            </td>
                            <!-- NET AMOUNT TOTAL -->
                            <td class='align-right left bold bottom right font-red font-11 now-wrap'>
              
                            </td>
                        </tr>
                    ", limitPayroll.Sum(x => double.Parse(x.Salary)).FormalDigit(),
                        limitPayroll.Sum(x => double.Parse(x.Salary) / 2).FormalDigit(),
                        limitPayroll.Sum(x => x.Adjustment).FormalDigit(),
                        limitPayroll.Sum(x => x.Deduction).FormalDigit(),
                        limitPayroll.Sum(x => x.NetAmount).FormalDigit(),
                        limitPayroll.Sum(x => x.OtherAdjustments).FormalDigit(),
                        limitPayroll.Sum(x => x.EWT).FormalDigit(),
                        limitPayroll.Sum(x => x.ProfessionalTax).FormalDigit(),
                        limitPayroll.Sum(x => x.HWMPC.Item).FormalDigit(),
                        limitPayroll.Sum(x => x.Pagibig.Item).FormalDigit(),
                        limitPayroll.Sum(x => x.PHIC.Item).FormalDigit(),
                        limitPayroll.Sum(x => x.GSIS.Item).FormalDigit(),
                        0.00.FormalDigit(),
                        limitPayroll.Sum(x => x.NetPay).FormalDigit()
                        );
                        //table balance forwarded
                        pdf.AppendFormat(@"
                        <tr>
                            <td class='align-center vertical-align-middle bold left bottom font-11 now-wrap' colspan='4'>
                                Balance Forwarded
                            </td>
                            <!-- MONTH RATE TOTAL -->
                            <td class='align-right left bold bottom font-11 now-wrap'>
              
                            </td>
                            <!-- HALF MONTH TOTAL -->
                            <td class='align-right left bold bottom font-11 now-wrap'>
              
                            </td>
                            <!-- ADJUSTMENT TOTAL -->
                            <td class='align-right left bold bottom font-11 now-wrap'>
              
                            </td>
                            <!-- LATES AND ABSENCES TOTAL -->
                            <td class='align-right left bold bottom font-11 now-wrap'>
              
                            </td>
                            <!-- GROSS AMOUNT TOTAL -->
                            <td class='align-right left bold bottom font-11 font-red now-wrap'>
              
                            </td>
                            <!-- ADJUSTMENT ADD TOTAL -->
                            <td class='align-right left bold bottom font-11 now-wrap'>
              
                            </td>
                            <!-- EWT TOTAL -->
                            <td class='align-right left bold bottom font-11 now-wrap'>
              
                            </td>
                            <!-- PROF TAX TOTAL -->
                            <td class='align-right left bold bottom font-11 now-wrap'>
              
                            </td>
                            <!-- HWMPC TOTAL -->
                            <td class='align-right left bold bottom font-11 now-wrap'>
              
                            </td>
                            <!-- PAGIBIG TOTAL -->
                            <td class='align-right left bold bottom font-11 now-wrap'>
              
                            </td>
                            <!-- PHIC TOTAL -->
                            <td class='align-right left bold bottom font-11 now-wrap'>
              
                            </td>
                            <!-- GSIS TOTAL -->
                            <td class='align-right left bold bottom font-11 now-wrap'>
              
                            </td>
                            <!-- EXCESS MOBILE TOTAL -->
                            <td class='align-right left bold bottom font-11 now-wrap'>
              
                            </td>
                            <!-- NET AMOUNT TOTAL -->
                            <td class='align-right left bold bottom font-red font-11 now-wrap'>
                                {0}
                            </td>
                            <!-- NET AMOUNT TOTAL -->
                            <td class='align-right left bold bottom right font-red font-11 now-wrap'>
              
                            </td>
                        </tr>
                    ", limitPayroll.Sum(x => x.NetPay).FormalDigit());
                        //table end
                        pdf.AppendFormat(@"
                        </tbody>
                        </table>
                    ");
                        //page break
                        if(ctr != models.Count())
                            pdf.AppendFormat("<div style='page-break-after: always;'></div>");
                    }
                }

            }
            pdf.Append(HtmlEnd());

            return pdf.ToString();
        }

        #endregion

        #region HELPERS

        public byte[] SetPDF(string regularPayrolls, string document_title)
        {
            new CustomAssemblyLoadContext().LoadUnmanagedLibrary(Path.Combine(Directory.GetCurrentDirectory(), "libwkhtmltox.dll"));
            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Landscape,
                PaperSize = PaperKind.Legal,
                Margins = new MarginSettings { Top = 0.5, Bottom = 0.5, Left = 0.5, Right = 0.5, Unit = Unit.Centimeters },
                DocumentTitle = document_title
            };
            var objectSettings = new ObjectSettings
            {
                PagesCount = true,
                HtmlContent = regularPayrolls,
                WebSettings = { DefaultEncoding = "utf-8", UserStyleSheet = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "css", "pdf.css") },
                FooterSettings = { FontName = "Arial", FontSize = 9, Line = false, Right = "Page [page] of [toPage]" }
            };

            var pdf = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings }
            };
            return _converter.Convert(pdf);
        }
        public string HtmlStart()
        {
            return @"<html>
                    <head></head>
                    <body>".ToString();
        }

        public string HtmlEnd()
        {
            return @"
                </body>
            </html>".ToString();
        }
        public List<JoPayrollModel> GetJoPayroll(List<Employee> employees, string date, int range)
        {
            var payrolls = new List<JoPayrollModel>();
            foreach(var employee in employees)
            {
                payrolls.Add(PayrollDatabase.Instance.GetJoPayrollByID(employee.ID, employee.Fname, employee.Mname, employee.Lname, employee.Salary, StartDate, EndDate).Result);
            }

            return payrolls;
        }

        public List<SubsistenceModel> GetSubsistenceModels(List<Employee> employees, string date, double subsistence, double laundry)
        {
            GetStartDay(date);
            var sub = new List<SubsistenceModel>();
            foreach(var employee in employees)
            {
                sub.Add(new SubsistenceModel
                {
                    SubsistenceId = PayrollDatabase.Instance.SubsistenceExists(employee.ID, StartDate.Month, StartDate.Year),
                    ID = employee.ID,
                    Date = date,
                    Subsistence = subsistence,
                    Laundry = laundry,
                    NoDaysAbsent = 0,
                    HWMPC = 0,
                    Remarks = ""
                });
            }
            return sub;
        }

        public List<RegularPayrollModel> GetRegularPayroll(List<Employee> employees, string date)
        {
            var payrolls = new List<RegularPayrollModel>();
            foreach (var employee in employees)
            {
                payrolls.Add(PayrollDatabase.Instance.GetRegularPayrollByID(employee.ID, employee.Fname, employee.Mname, employee.Lname, employee.Salary, date).Result);
            }

            return payrolls;
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
