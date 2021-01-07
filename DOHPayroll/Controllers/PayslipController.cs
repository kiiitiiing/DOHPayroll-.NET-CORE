using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DinkToPdf;
using DinkToPdf.Contracts;
using DOHPayroll.Databases;
using DOHPayroll.Services;
using DOHPayroll.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DOHPayroll.Controllers
{
    public class PayslipController : Controller
    {
        #region CLASSES
        public string fontwhite = "font-white";
        public string font7 = "font-7";
        public string font9 = "font-9";
        public string font10 = "font-10";
        public string font11 = "font-11";
        public string invert = "invert";
        public string bold = "bold";
        public string aligncenter = "align-center";
        public string alignleft = "align-left";
        public string alignright = "align-right";
        public string width22 = "width-22";
        #endregion
        private IConverter _converter;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public PayslipController(IConverter converter)
        {
            _converter = converter;
        }

        #region REGULAR PAYSLIP
        public async Task<IActionResult> RegularPayslip(string userid, int month, int year)
        {
            StartDate = new DateTime(year, month, 1);
            var payslip = await PayrollDatabase.Instance.GetPayslipAsync(userid, StartDate);

            var content = RegularPayslipHtml(payslip, StartDate);

            var file = SetPDF(content, payslip.FirstHalf.RegularPayroll.Fullname +" Payslip");

            return File(file, "application/pdf");
        }
        #endregion

        #region JOB ORDER PAYSLIP 
        public async Task<IActionResult> JoPayslip(string userid, string fname, string mname, string lname, string salary, int payrollId)
        {
            var model = await PayrollDatabase.Instance.GetJoPayrollByID(userid, fname, mname, lname, salary, payrollId);

            var content = JoPayslipHtml(model);

            var file = SetPDF(content, model.Fullname + " Payslip");

            return File(file, "application/pdf");
        }
        #endregion

        #region REGULAR PDF

        public string RegularPayslipHtml(RegularPayslipModel payslip, DateTime datetime)
        {
            var pdf = new StringBuilder();
            var date = datetime.ToString("MMM") + " " + datetime.Day + " - " + DateTime.DaysInMonth(datetime.Year, datetime.Month) + ", " + datetime.Year;
            pdf.Append(HtmlStart());
            //employee info
            pdf.AppendFormat(@"
                 <table class='table align-center arial border-separate'>
                    <tbody>
                        <!-- 1 -->
                        <tr>
                            <td class='font-11' colspan='2'>
                                Republic of the Philippines
                            </td>
                        </tr>
                        <!-- 2 -->
                        <tr>
                            <td class='font-11 bold' colspan='2'>
                                DEPARTMENT OF HEALTH 
                            </td>
                        </tr>
                        <!-- 3 -->
                        <tr>
                            <td class='font-11' colspan='2'>
                                Regional Office VII
                            </td>
                        </tr>
                        <!-- 4 -->
                        <tr>
                            <td class='font-11' colspan='2'>
                                Osmeña Blvd. Cebu City
                            </td>
                        </tr>
                        <!-- 5 -->
                        <tr>
                            <td class='font-11' colspan='2'>
                                Tel. No.: (032) 418-7125
                            </td>
                        </tr>
                        <!-- 6 -->
                        <tr>
                            <td class='font-11 bold' colspan='2'>
                                Salary and Other Benefits Slip For The Month Of {0}
                            </td>
                        </tr>
                        <!-- 7 -->
                        <tr>
                            <td class='font-11 tr-10' colspan='2'>
                            </td>
                        </tr>
                
                        <!-- 8 -->
                        <tr class='align-left'>
                            <td class='font-11 invert bold width-4'>
                                ID No.
                            </td>
                            <td class='font-11 bold width-12'>
                                {1}
                            </td>
                        </tr>

                
                        <!-- 9 -->
                        <tr class='align-left'>
                            <td class='font-11 invert bold width-4'>
                                Employee name
                            </td>
                            <td class='font-11 bold width-12'>
                                {2}
                            </td>
                        </tr>

                
                        <!-- 9 -->
                        <tr class='align-left'>
                            <td class='font-11 invert bold width-4'>
                                Designation
                            </td>
                            <td class='font-11 bold width-12'>
                                {3}
                            </td>
                        </tr>
                    </tbody>
                </table>
            ", date, payslip.UserID, payslip.FullName, payslip.Designation);
            //employee kuan
            //TABLE START
            pdf.Append(@"
                 <table class='table align-left border-separate arial font-11' style='margin-top: 10px;'>
                    <tbody>
                ");
            //row 1
            pdf.Append(Row(new List<string>
            {
                //td 1,2
                Contents("FIRST HALF SALARY",
                    Classes(width22, font11, invert, bold),
                    ColSpan(2)),
                //td 3
                MidBlank(1),
                //td 4,5
                Contents("SECOND HALF SALARY",
                    Classes(width22, font11, invert, bold),
                    ColSpan(2))
            }));
            //ROW 2
            pdf.Append(Row(new List<string>
            {
                //td 1
                Contents("BASIC SALARY",
                    Classes(width22, font11),
                    ""),
                //td 2
                Contents(payslip.Salary.FormalDigit(),
                    Classes(width22, bold),
                    ""),
                //td 3
                MidBlank(2),
                //td 4
                Contents("SALARY & PERA/ACA",
                    Classes(width22, font11,invert,bold),
                    ""),
                //td 5
                Contents(payslip.FirstHalf.RegularPayroll.TotalSalary.FixDigit(),
                    Classes(width22, font11,invert,bold),
                    "")
            }));
            //ROW 3
            pdf.Append(Row(new List<string>
            {
                //td 1
                Contents("PERA / ACA",
                    Classes(width22, font11),
                    ""),
                //td 2
                Contents(payslip.FirstHalf.RegularPayroll.PERA.FixDigit(),
                    Classes(width22, bold),
                    ""),
                //td 3
                MidBlank(3),
                //td 4
                Contents("",
                    Classes(width22, font11),
                    ""),
                //td 5
                Contents("",
                    Classes(width22, font11),
                    "")
            }));
            //ROW 4
            pdf.Append(Row(new List<string>
            {
                //td 1
                Contents("TOTAL SALARY",
                    Classes(width22, font11, invert, bold),
                    ""),
                //td 2
                Contents(payslip.FirstHalf.RegularPayroll.TotalSalary.FixDigit(),
                    Classes(width22, bold, invert),
                    ""),
                //td 3
                MidBlank(4),
                //td 4
                Contents("HAZARD PAY",
                    Classes(width22, font11),
                    ""),
                //td 5
                Contents(payslip.SecondHalf.Hazard.HazardPay.FixDigit(),
                    Classes(width22, font11),
                    "")
            }));
            //ROW 5
            pdf.Append(Row(new List<string>
            {
                //td 1,2
                Contents("DEDUCTIONS",
                    Classes(width22, font11, invert, bold),
                    Attributes("colspan='2'","style='width: 22%;'")),
                //td 3
                MidBlank(5),
                //td 4
                Contents("MORTUARY",
                    Classes(width22, font11),
                    ""),
                //td 5
                Contents(payslip.SecondHalf.Hazard.Mortuary.FixDigit().Enclose(),
                    Classes(width22, font11,alignright),
                    "")
            }));
            //ROW 6
            pdf.Append(Row(new List<string>
            {
                //td 1
                Contents("DEDUCTION (Late)",
                    Classes(width22, font11),
                    ""),
                //td 2
                Contents(payslip.FirstHalf.RegularPayroll.DeductionLate.FixDigit().Enclose(),
                    Classes(width22, font11, alignright),
                    ""),
                //td 3
                MidBlank(6),
                //td 4
                Contents("HWMPC",
                    Classes(width22, font11),
                    ""),
                //td 5
                Contents(payslip.SecondHalf.Hazard.HWMPCLoan.FixDigit().Enclose(),
                    Classes(width22, font11,alignright),
                    "")
            }));
            //ROW 7
            pdf.Append(Row(new List<string>
            {
                //td 1
                Contents("TAX",
                    Classes(width22, font11),
                    ""),
                //td 2
                Contents(payslip.FirstHalf.RegularPayroll.ProfessionalTax.FixDigit().Enclose(),
                    Classes(width22, font11, alignright),
                    ""),
                //td 3
                MidBlank(7),
                //td 4
                Contents("OTHER ACCOUNTS PAYABLE",
                    Classes(width22, font11),
                    ""),
                //td 5
                Contents(payslip.SecondHalf.Hazard.DigitelBilling.FixDigit().Enclose(),
                    Classes(width22, font11,alignright),
                    "")
            }));
            //ROW 8
            pdf.Append(Row(new List<string>
            {
                //td 1
                Contents("PHILHEALTH",
                    Classes(width22, font11),
                    ""),
                //td 2
                Contents(payslip.FirstHalf.RegularPayroll.Phic.FixDigit().Enclose(),
                    Classes(width22, font11, alignright),
                    ""),
                //td 3
                MidBlank(8),
                //td 4
                Contents("NET HAZARD PAY",
                    Classes(width22, font11,invert,bold),
                    ""),
                //td 5
                Contents(payslip.SecondHalf.Hazard.NetAmount.FixDigit(),
                    Classes(width22, font11,invert,bold),
                    "")
            }));
            //ROW 9
            pdf.Append(Row(new List<string>
            {
                //td 1
                Contents("GSIS PREMIUM",
                    Classes(width22, font11),
                    ""),
                //td 2
                Contents(payslip.FirstHalf.RegularPayroll.GSIS_Premium.FixDigit().Enclose(),
                    Classes(width22, font11, alignright),
                    ""),
                //td 3
                MidBlank(9),
                //td 4
                Contents("CELLPHONE COMMUNICATION",
                    Classes(width22, font11),
                    ""),
                //td 5
                Contents(payslip.SecondHalf.CommAll.Amount.FixDigit(),
                    Classes(width22, font11),
                    "")
            }));
            //ROW 10
            pdf.Append(Row(new List<string>
            {
                //td 1
                Contents("GSIS GFAL",
                    Classes(width22, font11),
                    ""),
                //td 2
                Contents(payslip.FirstHalf.RegularPayroll.GSIS_GFAL.Item.FixDigit().Enclose(),
                    Classes(width22, font11, alignright),
                    ""),
                //td 3
                MidBlank(10),
                //td 4
                Contents("DEDUCTION",
                    Classes(width22, font11),
                    ""),
                //td 5
                Contents(payslip.SecondHalf.CommAll.MonthBillAmount.FixDigit().Enclose(),
                    Classes(width22, font11, alignright),
                    "")
            }));
            //ROW 11
            pdf.Append(Row(new List<string>
            {
                //td 1
                Contents("GSIS COMP",
                    Classes(width22, font11),
                    ""),
                //td 2
                Contents(payslip.FirstHalf.RegularPayroll.GSIS_COMP.Item.FixDigit().Enclose(),
                    Classes(width22, font11, alignright),
                    ""),
                //td 3
                MidBlank(11),
                //td 4
                Contents("NET CELLPHONE COMM.",
                    Classes(width22, font11,invert,bold),
                    ""),
                //td 5
                Contents(payslip.SecondHalf.CommAll.NetAmount.FixDigit(),
                    Classes(width22, font11,invert,bold),
                    "")
            }));
            //ROW 12
            pdf.Append(Row(new List<string>
            {
                //td 1
                Contents("GSIS EDU",
                    Classes(width22, font11),
                    ""),
                //td 2
                Contents(payslip.FirstHalf.RegularPayroll.GSIS_EDU.Item.FixDigit().Enclose(),
                    Classes(width22, font11, alignright),
                    ""),
                //td 3
                MidBlank(12),
                //td 4
                Contents("ANNIVERSARY ALLOWANCE",
                    Classes(width22, font11),
                    ""),
                //td 5
                Contents("0.00",
                    Classes(width22, font11),
                    "")
            }));
            //ROW 13
            pdf.Append(Row(new List<string>
            {
                //td 1
                Contents("GSIS CONSOLOAN",
                    Classes(width22, font11),
                    ""),
                //td 2
                Contents(payslip.FirstHalf.RegularPayroll.GSIS_Consoloan.Item.FixDigit().Enclose(),
                    Classes(width22, font11, alignright),
                    ""),
                //td 3
                MidBlank(13),
                //td 4
                Contents("UNIFORM AND CLOTHING ALL",
                    Classes(width22, font11),
                    ""),
                //td 5
                Contents("0.00",
                    Classes(width22, font11),
                    "")
            }));
            //ROW 14
            pdf.Append(Row(new List<string>
            {
                //td 1
                Contents("GSIS POLICY LOAN",
                    Classes(width22, font11),
                    ""),
                //td 2
                Contents(payslip.FirstHalf.RegularPayroll.GSIS_PolicyLoan.Item.FixDigit().Enclose(),
                    Classes(width22, font11, alignright),
                    ""),
                //td 3
                MidBlank(14),
                //td 4
                Contents("MASTERAL",
                    Classes(width22, font11),
                    ""),
                //td 5
                Contents("0.00",
                    Classes(width22, font11),
                    "")
            }));
            //ROW 15
            pdf.Append(Row(new List<string>
            {
                //td 1
                Contents("GSIS EML",
                    Classes(width22, font11),
                    ""),
                //td 2
                Contents(payslip.FirstHalf.RegularPayroll.GSIS_EML.Item.FixDigit().Enclose(),
                    Classes(width22, font11, alignright),
                    ""),
                //td 3
                MidBlank(15),
                //td 4
                Contents("DEDUCTION",
                    Classes(width22, font11),
                    ""),
                //td 5
                Contents("0.00".Enclose(),
                    Classes(width22, font11, alignright),
                    "")
            }));
            //ROW 16
            pdf.Append(Row(new List<string>
            {
                //td 1
                Contents("GSIS UOLI",
                    Classes(width22, font11),
                    ""),
                //td 2
                Contents(payslip.FirstHalf.RegularPayroll.GSIS_UOLI.Item.FixDigit().Enclose(),
                    Classes(width22, font11, alignright),
                    ""),
                //td 3
                MidBlank(16),
                //td 4
                Contents("NET MASTERAL ",
                    Classes(width22, font11, invert, bold),
                    ""),
                //td 5
                Contents("0.00",
                    Classes(width22, font11, invert, bold),
                    "")
            }));
            //ROW 17
            pdf.Append(Row(new List<string>
            {
                //td 1
                Contents("PAGIBIG PREMIUM",
                    Classes(width22, font11),
                    ""),
                //td 2
                Contents(payslip.FirstHalf.RegularPayroll.PagibigPremium.FixDigit().Enclose(),
                    Classes(width22, font11, alignright),
                    ""),
                //td 3
                MidBlank(17),
                //td 4
                Contents("LOYALTY BONUS",
                    Classes(width22, font11),
                    ""),
                //td 5
                Contents("0.00",
                    Classes(width22, font11),
                    "")
            }));
            //ROW 18
            pdf.Append(Row(new List<string>
            {
                //td 1
                Contents("PAGIBIG LOAN",
                    Classes(width22, font11),
                    ""),
                //td 2
                Contents(payslip.FirstHalf.RegularPayroll.PagibigLoan.Item.FixDigit().Enclose(),
                    Classes(width22, font11, alignright),
                    ""),
                //td 3
                MidBlank(18),
                //td 4
                Contents("MID-YEAR BONUS",
                    Classes(width22, font11),
                    ""),
                //td 5
                Contents(payslip.SecondHalf.MidYearBonus.FixDigit(),
                    Classes(width22, font11),
                    "")
            }));
            //ROW 19
            pdf.Append(Row(new List<string>
            {
                //td 1
                Contents("PAGIBIG CALAMITY",
                    Classes(width22, font11),
                    ""),
                //td 2
                Contents(payslip.FirstHalf.RegularPayroll.PagibigLoan.Item.FixDigit().Enclose(),
                    Classes(width22, font11, alignright),
                    ""),
                //td 3
                MidBlank(19),
                //td 4
                Contents("DEDUCTION",
                    Classes(width22, font11),
                    ""),
                //td 5
                Contents("0.00".Enclose(), //mid year deduction
                    Classes(width22, font11),
                    "")
            }));
            //ROW 20
            pdf.Append(Row(new List<string>
            {
                //td 1
                Contents("CFI",
                    Classes(width22, font11),
                    ""),
                //td 2
                Contents(payslip.FirstHalf.RegularPayroll.CFI.Item.FixDigit().Enclose(),
                    Classes(width22, font11, alignright),
                    ""),
                //td 3
                MidBlank(20),
                //td 4
                Contents("NET MID-YEAR BONUS",
                    Classes(width22, font11, invert, bold),
                    ""),
                //td 5
                Contents(payslip.SecondHalf.MidYearBonus.FixDigit(), //net mid year 
                    Classes(width22, font11, invert, bold),
                    "")
            }));
            //ROW 21
            pdf.Append(Row(new List<string>
            {
                //td 1
                Contents("SIMC",
                    Classes(width22, font11),
                    ""),
                //td 2
                Contents(payslip.FirstHalf.RegularPayroll.SIMC.Item.FixDigit().Enclose(),
                    Classes(width22, font11, alignright),
                    ""),
                //td 3
                MidBlank(21),
                //td 4
                Contents("YEAR-END BONUS",
                    Classes(width22, font11, bold),
                    ""),
                //td 5
                Contents(payslip.SecondHalf.YearEndBonus.FixDigit(),
                    Classes(width22, font11, bold),
                    "")
            }));
            //ROW 22
            pdf.Append(Row(new List<string>
            {
                //td 1
                Contents("HWMPC",
                    Classes(width22, font11),
                    ""),
                //td 2
                Contents(payslip.FirstHalf.RegularPayroll.HWMPC.Item.FixDigit().Enclose(),
                    Classes(width22, font11, alignright),
                    ""),
                //td 3
                MidBlank(22),
                //td 4
                Contents("DEDUCTION",
                    Classes(width22, font11, bold),
                    ""),
                //td 5
                Contents("0.00".Enclose(),
                    Classes(width22, font11, bold),
                    "")
            }));
            //ROW 23
            pdf.Append(Row(new List<string>
            {
                //td 1
                Contents("GSIS EDU ASST",
                    Classes(width22, font11),
                    ""),
                //td 2
                Contents(payslip.FirstHalf.RegularPayroll.GSIS_EDU.Item.FixDigit().Enclose(),
                    Classes(width22, font11, alignright),
                    ""),
                //td 3
                MidBlank(23),
                //td 4
                Contents("NET-YEAR END BONUS",
                    Classes(width22, font11, invert, bold),
                    ""),
                //td 5
                Contents(payslip.SecondHalf.YearEndBonus.FixDigit(),
                    Classes(width22, font11, invert, bold),
                    "")
            }));
            //ROW 24
            pdf.Append(Row(new List<string>
            {
                //td 1
                Contents("DISALLOWANCES",
                    Classes(width22, font11),
                    ""),
                //td 2
                Contents(payslip.FirstHalf.RegularPayroll.Disallowances.Item.FixDigit().Enclose(),
                    Classes(width22, font11, alignright),
                    ""),
                //td 3
                MidBlank(24),
                //td 4
                Contents("MONETIZATION",
                    Classes(width22, font11, bold),
                    ""),
                //td 5
                Contents("0.00",
                    Classes(width22, font11, bold),
                    "")
            }));
            //ROW 25
            pdf.Append(Row(new List<string>
            {
                //td 1
                Contents("DBP",
                    Classes(width22, font11),
                    ""),
                //td 2
                Contents(payslip.FirstHalf.RegularPayroll.DBP.Item.FixDigit().Enclose(),
                    Classes(width22, font11, alignright),
                    ""),
                //td 3
                MidBlank(25),
                //td 4
                Contents("DEDUCTION",
                    Classes(width22, font11),
                    ""),
                //td 5
                Contents("0.00",
                    Classes(width22, font11, alignright),
                    "")
            }));
            //ROW 26
            pdf.Append(Row(new List<string>
            {
                //td 1
                Contents("GSIS HELP",
                    Classes(width22, font11),
                    ""),
                //td 2
                Contents(payslip.FirstHalf.RegularPayroll.GSIS_Help.Item.FixDigit().Enclose(),
                    Classes(width22, font11, alignright),
                    ""),
                //td 3
                MidBlank(26),
                //td 4
                Contents("NET MONETIZATION",
                    Classes(width22, font11, invert, bold),
                    ""),
                //td 5
                Contents("0.00",
                    Classes(width22, font11, invert, bold),
                    "")
            }));
            //ROW 27
            pdf.Append(Row(new List<string>
            {
                //td 1
                Contents("REL",
                    Classes(width22, font11),
                    ""),
                //td 2
                Contents(payslip.FirstHalf.RegularPayroll.GSIS_Rel.Item.FixDigit().Enclose(),
                    Classes(width22, font11, alignright),
                    ""),
                //td 3
                MidBlank(27),
                //td 4
                Contents(Span("PBB (Performance Based Bonus)",
                        Classes(font7)),
                    Classes(width22, font9),
                    ""),
                //td 5
                Contents("0.00",
                    Classes(width22, font11),
                    "")
            }));
            //ROW 28
            pdf.Append(Row(new List<string>
            {
                //td 1
                Contents("TOTAL DEDUCTIONS",
                    Classes(width22, font11, invert, bold),
                    ""),
                //td 2
                Contents(payslip.FirstHalf.RegularPayroll.TotalDeductions.FixDigit().Enclose(),
                    Classes(width22, font11, invert, bold, alignright),
                    ""),
                //td 3
                MidBlank(28),
                //td 4
                Contents("ONA",
                    Classes(width22, font11),
                    ""),
                //td 5
                Contents("0.00",
                    Classes(width22, font11),
                    "")
            }));
            //ROW 29
            pdf.Append(Row(new List<string>
            {
                //td 1
                Contents("NET SALARY & PERA/ACA ",
                    Classes(width22, font11, invert, bold),
                    ""),
                //td 2
                Contents(payslip.FirstHalf.NetSalary.FixDigit(),
                    Classes(width22, font11, invert, bold, alignright),
                    ""),
                //td 3
                MidBlank(29),
                //td 4
                Contents("DEDUCTION",
                    Classes(width22, font11),
                    ""),
                //td 5
                Contents("0.00".Enclose(),
                    Classes(width22, font11, alignright),
                    "")
            }));
            //ROW 30
            pdf.Append(Row(new List<string>
            {
                //td 1
                Contents("SUBSISTENCE",
                    Classes(width22, font11),
                    ""),
                //td 2
                Contents(payslip.FirstHalf.Subsistence.Subsistence.FixDigit(),
                    Classes(width22, font11),
                    ""),
                //td 3
                MidBlank(30),
                //td 4
                Contents("NET ONA",
                    Classes(width22, font11, invert, bold),
                    ""),
                //td 5
                Contents("0.00".Enclose(),
                    Classes(width22, font11, invert, bold),
                    "")
            }));
            //ROW 31
            pdf.Append(Row(new List<string>
            {
                //td 1
                Contents("DEDUCTION (Absences)",
                    Classes(width22, font11),
                    ""),
                //td 2
                Contents(payslip.FirstHalf.Subsistence.LessTotal.FixDigit().Enclose(),
                    Classes(width22, font11, alignright),
                    ""),
                //td 3
                MidBlank(31),
                //td 4
                Contents("PEI",
                    Classes(width22, font11),
                    ""),
                //td 5
                Contents("0.00",
                    Classes(width22, font11),
                    "")
            }));
            //ROW 32
            pdf.Append(Row(new List<string>
            {
                //td 1
                Contents("NET SUBSISTENCE",
                    Classes(width22, font11, invert, bold),
                    ""),
                //td 2
                Contents(payslip.FirstHalf.Subsistence.NetAmount.FixDigit(),
                    Classes(width22, font11, invert, bold),
                    ""),
                //td 3
                MidBlank(32),
                //td 4
                Contents("LONGEVITY DIFFERENTIAL",
                    Classes(width22, font11),
                    ""),
                //td 5
                Contents("0.00",
                    Classes(width22, font11),
                    "")
            }));
            //ROW 33
            pdf.Append(Row(new List<string>
            {
                //td 1
                Contents("LONGEVITY",
                    Classes(width22, font11),
                    ""),
                //td 2
                Contents(payslip.FirstHalf.Longevity.TotalLPMO.FixDigit(),
                    Classes(width22, font11),
                    ""),
                //td 3
                MidBlank(33),
                //td 4
                Contents("RATA",
                    Classes(width22, font11),
                    ""),
                //td 5
                Contents(payslip.SecondHalf.Rata.Rata.FixDigit(),
                    Classes(width22, font11),
                    "")
            }));
            //ROW 34
            pdf.Append(Row(new List<string>
            {
                //td 1
                Contents("DEDUCTION",
                    Classes(width22, font11),
                    ""),
                //td 2
                Contents(payslip.FirstHalf.Longevity.Disallowance.FixDigit().Enclose(),
                    Classes(width22, font11, alignright),
                    ""),
                //td 3
                MidBlank(34),
                //td 4
                Contents("OTHERS",
                    Classes(width22, font11),
                    ""),
                //td 5
                Contents("0.00",
                    Classes(width22, font11),
                    "")
            }));
            //ROW 35
            pdf.Append(Row(new List<string>
            {
                //td 1
                Contents("NET LONGEVITY",
                    Classes(width22, font11, invert, bold),
                    ""),
                //td 2
                Contents(payslip.FirstHalf.Longevity.NetPay.FixDigit(),
                    Classes(width22, font11, invert, bold),
                    ""),
                //td 3
                MidBlank(35),
                //td 4
                Contents("",
                    Classes(width22, font11),
                    ""),
                //td 5
                Contents("",
                    Classes(width22, font11),
                    "")
            }));
            //ROW 36
            pdf.Append(Row(new List<string>
            {
                //td 1
                Contents("HAZARD DIFFERENTIAL",
                    Classes(width22, font11),
                    ""),
                //td 2
                Contents("0.00",
                    Classes(width22, font11),
                    ""),
                //td 3
                MidBlank(36),
                //td 4
                Contents("",
                    Classes(width22, font11),
                    ""),
                //td 5
                Contents("",
                    Classes(width22, font11),
                    "")
            }));
            //ROW 37
            pdf.Append(Row(new List<string>
            {
                //td 1
                Contents("SALARY DIFFERENTIAL",
                    Classes(width22, font11),
                    ""),
                //td 2
                Contents("0.00",
                    Classes(width22, font11),
                    ""),
                //td 3
                MidBlank(37),
                //td 4
                Contents("",
                    Classes(width22, font11),
                    ""),
                //td 5
                Contents("",
                    Classes(width22, font11),
                    "")
            }));
            //ROW 38
            pdf.Append(Row(new List<string>
            {
                //td 1
                Contents("MID BLANK 38",
                    Classes(width22, font11, fontwhite),
                    ColSpan(5))
            }));
            //NET PAYMENT
            pdf.Append(Row(new List<string>
            {
                //td 1
                Contents("NET PAYMENT",
                    Classes(font11,invert,bold,alignright,width22),
                    ColSpan(4)),
                //td 2
                Contents(payslip.NetPayment.FixDigit(),
                    Classes(font11, invert, bold, alignright, width22),
                    "")
            }));
            //TABLE END
            pdf.Append(@"
                    </tbody>
                </table>
            ");
            pdf.Append(HtmlEnd());
            var product = pdf.ToString();
            return product;
        }
        #endregion

        #region JOB ORDER PDF

        public string JoPayslipHtml(JoPayrollModel model)
        {
            var pdf = new StringBuilder();
            var date = model.StartDate.ToString("MMM") + " " + model.StartDate.Day + "-" + model.EndDate.Day + ", " + model.StartDate.Year;
            pdf.Append(HtmlStart());
            //pdf header
            pdf.AppendFormat(@"
                <table class='table align-center arial border-separate'>
                    <tbody>
                        <!-- 1 -->
                        <tr>
                            <td class='font-11' colspan='2'>
                                Republic of the Philippines
                            </td>
                        </tr>
                        <!-- 2 -->
                        <tr>
                            <td class='font-11 bold' colspan='2'>
                                DEPARTMENT OF HEALTH 
                            </td>
                        </tr>
                        <!-- 3 -->
                        <tr>
                            <td class='font-11' colspan='2'>
                                Regional Office VII
                            </td>
                        </tr>
                        <!-- 4 -->
                        <tr>
                            <td class='font-11' colspan='2'>
                                Osmeña Blvd. Cebu City
                            </td>
                        </tr>
                        <!-- 5 -->
                        <tr>
                            <td class='font-11' colspan='2'>
                                Tel. No.: (032) 418-7125
                            </td>
                        </tr>
                        <!-- 6 -->
                        <tr>
                            <td class='font-11 bold' colspan='2'>
                                Job Order Payslip for {0}
                            </td>
                        </tr>
                        <!-- 7 -->
                        <tr>
                            <td class='font-11 tr-10' colspan='2'>
                            </td>
                        </tr>
                
                        <!-- 8 -->
                        <tr class='align-left'>
                            <td class='font-11 invert bold width-4'>
                                ID No.
                            </td>
                            <td class='font-11 bold width-12'>
                                {1}
                            </td>
                        </tr>

                
                        <!-- 9 -->
                        <tr class='align-left'>
                            <td class='font-11 invert bold width-4'>
                                Employee name
                            </td>
                            <td class='font-11 bold width-12'>
                                {2}
                            </td>
                        </tr>
                        <!-- 9 -->
                        <tr class='align-left'>
                            <td class='font-11 invert bold width-4'>
                                Designation
                            </td>
                            <td class='font-11 bold width-12'>
                                {3}
                            </td>
                        </tr>
                    </tbody>
                </table>
            ", date, model.ID, model.Fullname, model.Designation);
            //pdf values
            pdf.AppendFormat(@"
                <!-- NET AMOUNT --> 
                <table class='table align-left arial border-collapse font-11'>
                    <tbody>
                        <tr>
                            <td class='width-6 bold tr-20' colspan='2'>
                            </td>
                        </tr>
                        <tr>
                            <td class='width-6 bold'>
                                Basic Salary:
                            </td>
                            <td class='width-6 bold'>
                                {0}
                            </td>
                        </tr>
                        <tr>
                            <td class='width-6 bold'>
                                Adjustments (+):
                            </td>
                            <td class='width-6 bold'>
                                {1}
                            </td>
                        </tr>
                        <tr>
                            <td class='width-6 bold' colspan='2'>
                                Deductions:
                            </td>
                        </tr>
                        <tr>
                            <td class='width-6 bold'>
                                &#8202;&#8202;Tardiness/Absences:
                            </td>
                            <td class='width-6 bold'>
                                {2}
                            </td>
                        </tr>
                        <tr>
                            <td class='width-6 bold tr-10' colspan='2' >
                            </td>
                        </tr>
                        <tr>
                            <td class='width-6 bold invert'>
                                Net Amount:
                            </td>
                            <td class='width-6 bold invert'>
                                {3}
                            </td>
                        </tr>
                    </tbody>
                </table>
                <!-- DEDUCTIONS -->
                <table class='table align-left arial border-collapse font-11'>
                    <tbody>
                        <tr>
                            <td class='width-6 bold tr-20' colspan='2'>
                            </td>
                        </tr>
                        <tr>
                            <td class='width-6 bold' colspan='2'>
                                Additional Option:
                            </td>
                        </tr>
                        <tr>
                            <td class='width-6 bold'>
                                &#8202;&#8202;Adjustment:
                            </td>
                            <td class='width-6 bold'>
                                {4}
                            </td>
                        </tr>
                        <tr>
                            <td class='width-6 bold' colspan='2'>
                                Deductions:
                            </td>
                        </tr>
                        <tr>
                            <td class='width-6 bold'>
                                &#8202;&#8202;5% EWT:
                            </td>
                            <td class='width-6 bold'>
                                {5}
                            </td>
                        </tr>
                        <tr>
                            <td class='width-6 bold'>
                                &#8202;&#8202;3% Professional Tax:
                            </td>
                            <td class='width-6 bold'>
                                {6}
                            </td>
                        </tr>
                        <tr>
                            <td class='width-6 bold'>
                                &#8202;&#8202;HWMPC:
                            </td>
                            <td class='width-6 bold'>
                                {7}
                            </td>
                        </tr>
                        <tr>
                            <td class='width-6 bold'>
                                &#8202;&#8202;Pag-Ibig:
                            </td>
                            <td class='width-6 bold'>
                                {8}
                            </td>
                        </tr>
                        <tr>
                            <td class='width-6 bold'>
                                &#8202;&#8202;PHIC:
                            </td>
                            <td class='width-6 bold'>
                                {9}
                            </td>
                        </tr>
                        <tr>
                            <td class='width-6 bold'>
                                &#8202;&#8202;GSIS:
                            </td>
                            <td class='width-6 bold'>
                                {10}
                            </td>
                        </tr>
                        <tr>
                            <td class='width-6 bold'>
                                &#8202;&#8202;Pagibig Loan:
                            </td>
                            <td class='width-6 bold'>
                                {11}
                            </td>
                        </tr>
                        <tr>
                            <td class='width-6 bold tr-10' colspan='2'>
                            </td>
                        </tr>
                        <tr>
                            <td class='width-6 bold invert'>
                                Total Deductions:
                            </td>
                            <td class='width-6 bold invert'>
                                {12}
                            </td>
                        </tr>
                        <tr>
                            <td class='width-6 bold tr-10' colspan='2'>
                            </td>
                        </tr>
                        <tr>
                            <td class='width-6 bold invert'>
                                Net Income:
                            </td>
                            <td class='width-6 bold invert'>
                                {13}
                            </td>
                        </tr>
                    </tbody>
                </table>

                <table class='table width-3 border-separate arial'>
                    <tbody class='align-center font-11 bold'>
                        <tr>
                            <td class='tr-20'>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                Certified true and correct:
                            </td>
                        </tr>
                        <tr>
                            <td class='tr-20 bottom'>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                Theresa Q. Tragico
                            </td>
                        </tr>
                        <tr>
                            <td>
                                Administrative Office V
                            </td>
                        </tr>
                        <tr>
                            <td>
                                Human Resource
                            </td>
                        </tr>
                        <tr>
                            <td>
                                Management
                            </td>
                        </tr>
                    </tbody>
                </table>
            ", model.Salary, model.Adjustment, (model.MinutesLate + model.AbsentDays.MinsFromAbsences()),
            model.NetAmount.ToString("F2"), model.OtherAdjustments.ToString("F2"), model.EWT.ToString("F2"), model.ProfessionalTax.ToString("F2"),
            model.HWMPC.Item.ToString("F2"), model.Pagibig.Item.ToString("F2"), model.PHIC.Item.ToString("F2"), model.GSIS.Item.ToString("F2"), model.PagibigLoan.Item.ToString("F2"),
            model.TotalDeductions.ToString("F2"), model.NetPay.ToString("F2"));
            pdf.Append(HtmlEnd());

            return pdf.ToString();
        }
        #endregion

        #region HELPERS
        public string Span(string content, string classes)
        {
            var text = new StringBuilder();
            text.AppendFormat(@"<span class='{0}'>{1}</span>", classes, content);
            return text.ToString();
        }
        public string ColSpan(int value)
        {
            return "colspan='" + value + "'";
        }
        public string Width(int value)
        {
            return "width: " + value + "%;";
        }
        public string Style(string attribs)
        {
            return "style='" + attribs + "'";
        }
        public string Classes(params string[] classes)
        {
            string classArr = "class='";
            foreach (var htmlClass in classes)
            {
                classArr += htmlClass+" ";
            }
            classArr += "'";
            return classArr;
        }
        public string Attributes(params string[] attributes)
        {
            string attribArr = "";
            foreach (var htmlAttrib in attributes)
            {
                attribArr += " " + htmlAttrib;
            }
            return attribArr;
        }
        public string Row(List<string> contents)
        {
            var text = new StringBuilder();
            text.Append(@"<tr>");
            foreach(var content in contents)
            {
                text.Append(content);
            }
            text.Append(@"</tr>");
            return text.ToString();
        }
        public string Contents(string value, string classes, string attributes)
        {
            var td = new StringBuilder();
            td.AppendFormat(@"
            <td {0} {1}>
                {2}
            </td>
            ", classes, attributes, value);

            return td.ToString();
        }
        public string MidBlank(int? row)
        {
            var text = new StringBuilder();
            text.AppendFormat(@"
                <td class='bold' style='color: white; width: 12%;'>
                    MID BLANK {0}
                </td>", row);
            return text.ToString();
        }
        public byte[] SetPDF(string content, string title)
        {
            new CustomAssemblyLoadContext().LoadUnmanagedLibrary(Path.Combine(Directory.GetCurrentDirectory(), "libwkhtmltox.dll"));
            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.Letter,
                Margins = new MarginSettings { Top = 1.54, Bottom = 1.34, Left = 1.54, Right = 1.54, Unit = Unit.Centimeters },
                DocumentTitle = title
            };
            var objectSettings = new ObjectSettings
            {
                PagesCount = true,
                HtmlContent = content,
                WebSettings = { DefaultEncoding = "utf-8", UserStyleSheet = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "css", "pdf_payslip.css") }
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

        #endregion
    }
}
