using DinkToPdf;
using DinkToPdf.Contracts;
using DOHPayroll.Databases;
using DOHPayroll.Resources;
using DOHPayroll.Services;
using DOHPayroll.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOHPayroll.Controllers
{
    [Authorize(Policy = "Administrator")]
    public class OtherPayrollController : Controller
    {
        private IConverter _converter;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public OtherPayrollController(IConverter converter)
        {
            _converter = converter;
        }

        #region OTHER PAYROLLS
        public async Task<IActionResult> GetOtherRemittance(string date, string type)
        {
            GetStartDay(date);
            switch (type)
            {
                case "longevity":
                    {
                        var longevities = await PayrollDatabase.Instance.GetLongevityPayrollAsync(StartDate.Month, StartDate.Year);

                        if (longevities.Count() > 0)
                        {
                            var content = LongevityHtml(longevities, StartDate);
                            var file = SetPDF(content, "Longevity Payroll", Orientation.Landscape, PaperKind.Legal);

                            return File(file, "application/pdf");
                        }
                        else
                        {
                            return View("~/Views/Report/NothingToGenerate.cshtml");
                        }
                    }
                case "subsistence":
                    {
                        var subsistenceList = await PayrollDatabase.Instance.GetSubsistencePayrollAsync(StartDate.Month, StartDate.Year);

                        if (subsistenceList.Count() > 0)
                        {
                            var content = SubsistenceHtml(subsistenceList, StartDate);
                            var file = SetPDF(content, "Subsistence Payroll", Orientation.Landscape, PaperKind.Legal);

                            return File(file, "application/pdf");
                        }
                        else
                        {
                            return View("~/Views/Report/NothingToGenerate.cshtml");
                        }
                    }
                case "hazard":
                    {
                        var hazardList = await PayrollDatabase.Instance.GetHazardByDateAsync(StartDate.Month, StartDate.Year);

                        if (hazardList.Count() > 0)
                        {
                            var content = HazardHtml(hazardList, StartDate);
                            var file = SetPDF(content, "Hazard Payroll", Orientation.Landscape, PaperKind.Legal);

                            return File(file, "application/pdf");
                        }
                        else
                        {
                            return View("~/Views/Report/NothingToGenerate.cshtml");
                        }
                    }
                case "cellphone":
                    {
                        var cellphoneList = await PayrollDatabase.Instance.GetCellphonePayrollAsync(StartDate.Month, StartDate.Year);

                        if (cellphoneList.Count() > 0)
                        {
                            var content = CellphoneHtml(cellphoneList, StartDate);
                            var file = SetPDF(content, "Communicable Allowance Payroll", Orientation.Landscape, PaperKind.LetterSmall);

                            return File(file, "application/pdf");
                        }
                        else
                        {
                            return View("~/Views/Report/NothingToGenerate.cshtml");
                        }
                    }
                case "rata":
                    {
                        var rataList = await PayrollDatabase.Instance.GetRataPayrollAsync(StartDate.Month, StartDate.Year);

                        if (rataList.Count() > 0)
                        {
                            var content = RataHtml(rataList, StartDate);
                            var file = SetPDF(content, "Regular RATA Payroll", Orientation.Landscape, PaperKind.LetterSmall);

                            return File(file, "application/pdf");
                        }
                        else
                        {
                            return View("~/Views/Report/NothingToGenerate.cshtml");
                        }
                    }
            }
            return NotFound();
        }
        #endregion

        #region CONTENTS
        //RATA CONTENT
        public string RataHtml(List<RATAModel> ratas, DateTime date)
        {
            var pdf = new StringBuilder();
            pdf.Append(HtmlStart());
            if (ratas.Count() > 0)
            {
                var ctr = 1;
                //table start
                pdf.AppendFormat(@"
                    <table class='arial table font-10 border-collapse regular'>
                        <tbody>");
                //HEADER
                pdf.AppendFormat(@"
                    <tr>
                        <td class='vertical-align-middle align-center bold font-15' colspan='8'>
                            GENERAL PAYROLL
                        </td>
                    </tr>
                    <tr>
                        <td class='vertical-align-middle align-left font-14' colspan='8'>
                            <p>
                            &nbsp;&nbsp;&nbsp;WE HEREBY ACKNOWLEDGE TO HAVE RECEIVED THE RATA - THE SUMS THEREIN SPECIFIED OPPOSITE OUT RESPECTIVE NAMES, BEING
                            IN FULL COMPENSATION FOR OUR SERVICE FOR THE MONTH OF <i class='bold' style='text-decoration: underline;'>{0}</i> EXCEPT AS NOTED 
                            OTHERWISE IN THE REMARKS COLUMN.
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td class='font-white'>
                            none
                        </td>
                    </td>
                    ", date.ToString("MMMM yyyy").ToUpper());
                //table header
                pdf.Append(@"
                    <tr>
                        <td colspan='8'>
              
                        </td>
                    </tr>
                    <tr>
                        <td class='vertical-align-middle align-center border bold font-12'>
                            NO
                        </td>
                        <td class='vertical-align-middle align-center border bold font-12'>
                            NAME
                        </td>
                        <td class='vertical-align-middle align-center border bold font-12'>
                            POSITION
                        </td>
                        <td class='vertical-align-middle align-center border bold font-12'>
                            RA
                        </td>
                        <td class='vertical-align-middle align-center border bold font-12'>
                            TA
                        </td>
                        <td class='vertical-align-middle align-center border bold font-12'>
                            Less:
                        </td>
                        <td class='vertical-align-middle align-center border bold font-12'>
                            TOTAL RATA
                        </td>
                        <td class='vertical-align-middle align-center border bold font-12'>
                            REMARKS
                        </td>
                    </tr>
                    ");
                //SPACE
                pdf.Append(HtmlSpaceRow(8));
                //employees
                foreach (var rata in ratas)
                {
                    pdf.AppendFormat(@"
                        <!-- EMPLOYEES -->
                        <!-- 1ST ROW -->
                        <tr>
                            <!-- NUMBER -->
                            <td class='vertical-align-middle align-center left bottom bold font-11'>
                                {0}
                            </td>
                            <!-- NAME -->
                            <td class='vertical-align-middle align-left left bottom bold font-11'>
                                {1}
                            </td>
                            <!-- POSITION -->
                            <td class='vertical-align-middle align-center left bottom bold font-11'>
                                {2}
                            </td>
                            <!-- RA -->
                            <td class='vertical-align-middle align-right left bottom bold font-11'>
                                {3}
                            </td>
                            <!-- TA -->
                            <td class='vertical-align-middle align-right left bottom bold font-11'>
                                {4}
                            </td>
                            <!-- LESS -->
                            <td class='vertical-align-middle align-right left bottom bold font-11'>
                                {5}
                            </td> 
                            <!-- TOTAL -->
                            <td class='vertical-align-middle align-right left bottom bold font-11'>
                                {6}
                            </td>
                            <!-- REMARKS -->
                            <td class='vertical-align-middle align-left left bottom right bold font-11 font-red'>
                                {7}
                            </td> 
                        </tr>
                        ", ctr, rata.Fullname, rata.Designation, rata.RA.FormalDigit(),rata.TA.FormalDigit(),
                        rata.Deductions.FormalDigit(),rata.Total.FormalDigit(), rata.Remarks);
                    ctr++;
                }
                for(int x = ctr; x <= 8; x++)
                {
                    pdf.Append(HtmlSpaceRow(8));
                }
                //total
                pdf.AppendFormat(@"
                    <!-- TOTAL -->
                    <tr>
                        <td class='vertical-align-middle align-right left bottom bold font-12'>
                        </td>
                        <td class='vertical-align-middle align-right left bottom bold font-12'>
                        </td>
                        <td class='vertical-align-middle align-right left bottom bold font-12'>
                        </td>
                        <!-- RA TOTAL -->
                        <td class='vertical-align-middle align-right left bottom bold font-12'>
                            {0}
                        </td>
                        <!-- TA TOTAL -->
                        <td class='vertical-align-middle align-right left bottom bold font-12'>
                            {1}
                        </td> 
                        <!-- LESS TOTAL -->
                        <td class='vertical-align-middle align-right left bottom bold font-12'>
                            {2}
                        </td>
                        <!-- IDK WHAT DIS IS -->
                        <td class='vertical-align-middle align-center left bottom right bold font-11'>
                            {3}
                        </td>
                        <td class='border'>
                        </td>
                    </tr>
                ", ratas.Sum(x=>x.RA).FormalDigit(), ratas.Sum(x=>x.TA).FormalDigit(), 
                ratas.Sum(x=>x.Deductions).FormalDigit(), ratas.Sum(x=>x.Total).FormalDigit());
                //asignatory
                pdf.Append(@"
                    <!-- A AND C-->
                    <tr>
                        <td class='font-11 vertical-align-top' colspan='5' rowspan='2'>
                            <span class='bold'>A.</span> Certified: Services duly rendered as stated.
                        </td>
                        <td class='font-11 vertical-align-top align-left' colspan='3' rowspan='2'>
                            <span class='bold'>C.</span> Approved Payment:
                        </td>
                    </tr>
                    <tr></tr>
                    <tr>
                        <td class='bold vertical-align-middle align-center font-12' colspan='3'>
                            Therese Q. Tragico
                        </td>
                        <td colspan='2'>
                        </td>
                        <td class='bold vertical-align-middle align-center font-12' colspan='3'>
                            Guy R. Perez, MD, RPT, FPSMS, MBAHA, CESE
                        </td>
                    </tr>
                    <tr>
                        <td class='vertical-align-middle align-center font-12' colspan='3' rowspan='2'>
                            Administrative Officer V
                        </td>
                        <td colspan='2'>
                        </td>
                        <td class='vertical-align-middle align-center font-12' colspan='3' rowspan='2'>
                            Director III
                        </td>
                    </tr>
                    <tr></tr>
                    <tr>
                        <td class='font-11 vertical-align-top' colspan='5' rowspan='2'>
                            <span class='bold'>B.</span>Certified:<br>
                            Supporting documents complete and proper, Cash available<br>
                            Subject to ADA (where applicable)
                        </td>
                        <td class='font-11 vertical-align-top align-left' colspan='3' rowspan='2'>
                            <span class='bold'>D.</span> Certified:<br>
                            Each employee whose name appears above<br>
                            has been paid the amount indicated opposite<br>
                            his/her name
                        </td>
                    </tr>
                    <tr></tr>
                    <tr>
                        <td class='bold vertical-align-middle align-center font-12' colspan='3'>
                            Angieline T. Adlaon, CPA, MBA
                        </td>
                        <td colspan='2'>
                        </td>
                        <td class='bold vertical-align-middle align-center font-12' colspan='3'>
                            Josephine D. Vergara
                        </td>
                    </tr>
                    <tr>
                        <td class='vertical-align-middle align-center font-12' colspan='3' rowspan='2'>
                            Accountant III
                        </td>
                        <td colspan='2'>
                        </td>
                        <td class='vertical-align-middle align-center font-12' colspan='3' rowspan='2'>
                            Administrative Officer V
                        </td>
                    </tr>
                    ");
                //table end
                pdf.AppendFormat(@"
                        </tbody>
                        </table>
                    ");
            }
            pdf.Append(HtmlEnd());

            return pdf.ToString();
        }

        //CELLPHONE CONTENT
        public string CellphoneHtml(List<CellphoneModel> cellphones, DateTime date)
        {
            var pdf = new StringBuilder();
            pdf.Append(HtmlStart());
            if (cellphones.Count() > 0)
            {
                var ctr = 1;
                //table start
                pdf.AppendFormat(@"
                    <table class='arial table font-10 border-collapse regular'>
                        <tbody>");
                //HEADER
                pdf.AppendFormat(@"
                    <tr>
                        <td class='vertical-align-middle align-center bold font-15' colspan='8'>
                            GENERAL PAYROLL
                        </td>
                    </tr>
                    <tr>
                        <td class='vertical-align-middle align-left font-14' colspan='8'>
                            <p>
                            &nbsp;&nbsp;&nbsp;WE HEREBY ACKNOWLEDGE TO HAVE RECEIVED THE CELLPHONE COMMUNICATION ALLOWANCE OF DOH OFFICIALS AND STAFF
                            FOR THE MONTH OF <i class='bold' style='text-decoration: underline;'>{0}</i> THE SUMS THEREIN SPECIFIED OPPOSITE OUR
                            RESPECTIVE NAMES BEING IN FULL COMPENSATION.
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td class='font-white'>
                            none
                        </td>
                    </td>
                    ", date.ToString("MMMM yyyy").ToUpper());
                //table header
                pdf.AppendFormat(@"
                    <tr>
                        <td colspan='7'>
              
                        </td>
                    </tr>
                    <tr>
                        <td class='vertical-align-middle align-center border bold font-12'>
                            NO
                        </td>
                        <td class='vertical-align-middle align-center border bold font-12'>
                            NAME
                        </td>
                        <td class='vertical-align-middle align-center border bold font-12'>
                            POSITION
                        </td>
                        <td class='vertical-align-middle align-center border bold font-12'>
                            AMOUNT
                        </td>
                        <td class='vertical-align-middle align-center border bold font-12'>
                            LESS: {0}<br>
                            Digitel Billing
                        </td>
                        <td class='vertical-align-middle align-center border bold font-12'>
                            NET TOTAL
                        </td>
                        <td class='vertical-align-middle align-center border bold font-12'>
                            REMARKS
                        </td>
                    </tr>
                    ", cellphones.First().MonthBill.ToUpper());
                //SPACE
                pdf.Append(HtmlSpaceRow(7));
                //employees
                foreach (var cellphone in cellphones)
                {
                    pdf.AppendFormat(@"
                        <!-- 1ST ROW -->
                        <tr>
                            <!-- NUMBER -->
                            <td class='vertical-align-middle align-center left bottom bold font-11'>
                                {0}
                            </td>
                            <!-- NAME -->
                            <td class='vertical-align-middle align-left left bottom bold font-11'>
                                {1}
                            </td>
                            <!-- POSITION -->
                            <td class='vertical-align-middle align-center left bottom bold font-11'>
                                {2}
                            </td>
                            <!-- AMOUNT -->
                            <td class='vertical-align-middle align-right left bottom bold font-11'>
                                {3}
                            </td>
                            <!-- LESS -->
                            <td class='vertical-align-middle align-right left bottom bold font-11'>
                                {4}
                            </td>
                            <!-- NET -->
                            <td class='vertical-align-middle align-right left bottom bold font-11'>
                                {5}
                            </td> 
                            <!-- REMARKS -->
                            <td class='vertical-align-middle align-left left bottom right bold font-11 font-red'>
                                {6}
                            </td> 
                        </tr>
                        ", ctr, cellphone.Fullname, cellphone.Designation, cellphone.Amount.FormalDigit(), cellphone.MonthBillAmount.FormalDigit(),
                        cellphone.NetAmount.FormalDigit(), cellphone.Remarks);
                    ctr++;
                }
                for (int x = ctr; x <= 8; x++)
                {
                    pdf.Append(HtmlSpaceRow(7));
                }
                //total
                pdf.AppendFormat(@"
                    <!-- TOTAL -->
                    <tr>
                        <td class='vertical-align-middle align-right left bottom bold font-12'>
                        </td>
                        <td class='vertical-align-middle align-right left bottom bold font-12'>
                        </td>
                        <td class='vertical-align-middle align-right left bottom bold font-12'>
                        </td>
                        <!-- AMOUNT TOTAL -->
                        <td class='vertical-align-middle align-right left bottom bold font-12'>
                            {0}
                        </td>
                        <!-- LESS TOTAL -->
                        <td class='vertical-align-middle align-right left bottom bold font-12'>
                            {1}
                        </td> 
                        <!-- NET AMOUNT TOTAL -->
                        <td class='vertical-align-middle align-right left bottom bold font-12'>
                            {2}
                        </td>
                        <td class='border'>
                        </td>
                    </tr>
                ", cellphones.Sum(x => x.Amount).FormalDigit(), cellphones.Sum(x => x.MonthBillAmount).FormalDigit(),
                cellphones.Sum(x => x.NetAmount).FormalDigit());
                //asignatory
                pdf.Append(@"
                    <!-- A AND C-->
                    <tr>
                        <td class='font-11 vertical-align-top' colspan='4' rowspan='2'>
                            <span class='bold'>A.</span> Certified: Services duly rendered as stated.
                        </td>
                        <td class='font-11 vertical-align-top align-left' colspan='3' rowspan='2'>
                            <span class='bold'>C.</span> Approved Payment:
                        </td>
                    </tr>
                    <tr></tr>
                    <tr>
                        <td class='bold vertical-align-middle align-center font-12' colspan='3'>
                            Therese Q. Tragico
                        </td>
                        <td></td>
                        <td class='bold vertical-align-middle align-center font-12' colspan='3'>
                            Guy R. Perez, MD, RPT, FPSMS, MBAHA, CESE
                        </td>
                    </tr>
                    <tr>
                        <td class='vertical-align-middle align-center font-12' colspan='3' rowspan='2'>
                            Administrative Officer V
                        </td>
                        <td></td>
                        <td class='vertical-align-middle align-center font-12' colspan='3' rowspan='2'>
                            Director III
                        </td>
                    </tr>
                    <tr></tr>
                    <tr>
                        <td class='font-11 vertical-align-top' colspan='4' rowspan='2'>
                            <span class='bold'>B.</span>Certified:<br>
                            Supporting documents complete and proper, Cash available<br>
                            Subject to ADA (where applicable)
                        </td>
                        <td class='font-11 vertical-align-top align-left' colspan='3' rowspan='2'>
                            <span class='bold'>D.</span> Certified:<br>
                            Each employee whose name appears above<br>
                            has been paid the amount indicated opposite<br>
                            his/her name
                        </td>
                    </tr>
                    <tr></tr>
                    <tr>
                        <td class='bold vertical-align-middle align-center font-12' colspan='3'>
                            Angieline T. Adlaon, CPA, MBA
                        </td>
                        <td></td>
                        <td class='bold vertical-align-middle align-center font-12' colspan='3'>
                            Josephine D. Vergara
                        </td>
                    </tr>
                    <tr>
                        <td class='vertical-align-middle align-center font-12' colspan='3' rowspan='2'>
                            Accountant III
                        </td>
                        <td></td>
                        <td class='vertical-align-middle align-center font-12' colspan='3' rowspan='2'>
                            Administrative Officer V
                        </td>
                    </tr>
                    ");
                //table end
                pdf.AppendFormat(@"
                        </tbody>
                        </table>
                    ");
            }
            pdf.Append(HtmlEnd());

            return pdf.ToString();
        }

        //HAZARD CONTENT
        public string HazardHtml(List<HazardModel> hazards, DateTime date)
        {
            var pdf = new StringBuilder();
            pdf.Append(HtmlStart());
            if (hazards.Count() > 0)
            {
                var ctr = 1;
                var flag = 1;
                int size = 20;
                foreach (var limitPayrolls in hazards.GroupBy(x => x.Section).OrderBy(x => x.Key))
                {
                    ctr = 1;

                    for (int x = 0; x <= limitPayrolls.Count() / size; x++)
                    {
                        var limitPayroll = limitPayrolls.Skip(x * size).Take(size);
                        //table start
                        pdf.AppendFormat(@"
                            <table class='arial table font-11 border-collapse regular'>
                                <tbody>
                        ");
                        //HEADER
                        pdf.AppendFormat(@"
                            <tr>
                                <td class='vertical-align-middle align-center bold font-15' colspan='15'>
                                    GENERAL PAYROLL
                                </td>
                            </tr>
                            <tr>
                                <td class='vertical-align-middle align-left font-14' colspan='15'>
                                    <p>
                                    &nbsp;&nbsp;&nbsp;WE HEREBY ACKNOWLEDGE TO HAVE RECEIVED THE PAYROLL OF <span class='bold'>HAZARD PAY</span>
                                    FOR THE MONTH OF <i class='bold' style='text-decoration: underline;'>{0}</i> THE SUMS THEREIN SPECIFIED OPPOSITE OUR
                                    RESPECTIVE NAMES BEING IN FULL COMPENSATION.
                                    </p>
                                </td>
                            </tr>
                            <tr>
                                <td class='font-white' colspan='15'>
                                    none
                                </td>
                            </td>
                            ", date.ToString("MMMM yyyy").ToUpper());
                        //table header
                        pdf.AppendFormat(@"
                            <!-- TABLE HEADERS -->
                            <tr>
                                <td class='vertical-align-middle align-center left bottom top bold font-12' colspan='3' rowspan='2'>
                                    NAME
                                </td>
                                <td class='vertical-align-middle align-center rotate left bottom top bold font-10' style='width: 10px !important; height: 120px;' rowspan='2'>
                                    INITIAL
                                </td>
                                <td class='vertical-align-middle align-center left bottom top bold font-12' rowspan='2'>
                                    POSITION
                                </td>
                                <td class='vertical-align-middle align-center left bottom top bold font-12' rowspan='2'>
                                    SALARY<br>
                                    GRADE
                                </td>
                                <td class='vertical-align-middle align-center left bottom top bold font-12' rowspan='2'>
                                    SALARY
                                </td>
                                <td class='vertical-align-middle align-center left bottom top bold font-12' rowspan='2'>
                                    HAZARD PAY
                                </td>
                                <td class='vertical-align-middle align-center left bottom top bold font-12' colspan='3'>
                                    DEDUCTION
                                </td>
                                <td class='vertical-align-middle align-center left bottom top bold font-12' rowspan='2'>
                                    NET<br>
                                    AMOUNT
                                </td>
                                <td class='vertical-align-middle align-center left bottom top bold font-12' rowspan='2'>
                                    # of days<br>
                                    w/ O.O
                                </td>
                                <td class='vertical-align-middle align-center left bottom top bold font-12' rowspan='2'>
                                    # of days<br>
                                    w/ leave
                                </td>
                                <td class='vertical-align-middle align-center left bottom top right bold font-white font-12' rowspan='2'>
                                    50%
                                </td>
                            </tr>
                            <tr>
                                <td class='vertical-align-middle align-center left bottom top bold font-11'>
                                    HWMPC Loan
                                </td>
                                <td class='vertical-align-middle align-center left bottom top bold font-11'>
                                    Globe Bill
                                </td>
                                <td class='vertical-align-middle align-center left bottom top bold font-11'>
                                    Sun Bill
                                </td>
                            </tr>
                            <tr>
                                <td class='font-12 bold bottom align-left' colspan='15'>
                                    {0}
                                </td>
                            </tr>
                        ", limitPayrolls.Key);
                        //employee row
                        foreach (var payroll in limitPayroll)
                        {
                            pdf.AppendFormat(@"
                                <!-- EMPLOYEES -->
                                <tr>
                                    <!-- NUMBER -->
                                    <td class='vertical-align-middle align-left left bottom font-11'>
                                        {0}
                                    </td>
                                    <!-- LNAME NAME -->
                                    <td class='vertical-align-middle align-left left bottom font-11'>
                                        {1}
                                    </td>
                                    <!-- FNAME NAME -->
                                    <td class='vertical-align-middle align-left left bottom font-11'>
                                        {2}
                                    </td>
                                    <!-- INITIAL -->
                                    <td class='vertical-align-middle align-left left bottom font-11'>
                                        {3}
                                    </td>
                                    <!-- POSITION -->
                                    <td class='vertical-align-middle align-left left bottom font-11'>
                                        {4}
                                    </td>
                                    <!-- SALARY GRADE -->
                                    <td class='vertical-align-middle align-left left bottom font-11'>
                                        {5}
                                    </td>
                                    <!-- SALARY -->
                                    <td class='vertical-align-middle align-right left bottom font-11'>
                                        {6}
                                    </d>
                                    <!-- HAZARD PAY -->
                                    <td class='vertical-align-middle align-right left bottom font-11'>
                                        {7}
                                    </td>
                                    <!-- HWMPC -->
                                    <td class='vertical-align-middle align-right left bottom font-11'>
                                        {8}
                                    </td>
                                    <!-- GLOBE -->
                                    <td class='vertical-align-middle align-right left bottom font-11'>
                                        {9}
                                    </td>
                                    <!-- SUN -->
                                    <td class='vertical-align-middle align-right left bottom font-11'>
                                        {10}
                                    </td>
                                    <!-- NET AMOUNT  -->
                                    <td class='vertical-align-middle align-right left bottom font-11'>
                                        {11}
                                    </td>
                                    <!-- W/ O.O  -->
                                    <td class='vertical-align-middle align-left left bottom font-11'>
                                        {12}
                                    </td>
                                    <!-- W/ LEAVE  -->
                                    <td class='vertical-align-middle align-left left bottom font-11'>
                                        {13}
                                    </td>
                                    <!-- REMARKS -->
                                    <td class='vertical-align-middle align-right right left bottom font-11'>
                                        {14}
                                    </td>
                                </tr>
                                ", ctr,payroll.Lname, payroll.Fname, payroll.Mname.Initial(), payroll.Designation,
                                payroll.SalaryGrade, payroll.Salary.FormalDigit(), payroll.HazardPay.FormalDigit(),
                                payroll.HWMPCLoan.FormalDigit(), payroll.GlobeBill.FormalDigit(), payroll.SunBill.FormalDigit(),
                                payroll.NetAmountNoMortuary.FormalDigit(),payroll.DaysWithOO,payroll.DaysWithLeave,payroll.HazardSalaryPercentage);
                            ctr++;
                            flag++;
                        }
                        //page total
                        pdf.AppendFormat(@"
                            <tr>
                                <td class='vertical-align-middle align-right left bottom bold font-12' colspan='6'>
                                PAGE TOTAL
                                </td>
                                <!-- SALARY TOTAL -->
                                <td class='vertical-align-middle align-left left bottom bold font-12'>
                                {0}
                                </td>
                                <!-- HAZARD TOTAL -->
                                <td class='vertical-align-middle align-left left bottom bold font-12' >
                                {1}
                                </td>
                                <!-- HWMPC TOTAL -->
                                <td class='vertical-align-middle align-left left bottom bold font-12'>
                                {2}
                                </td>
                                <!-- GLOBE TOTAL -->
                                <td class='vertical-align-middle align-left left bottom bold font-12' >
                                {3}
                                </td>
                                <!-- SUN TOTAL -->
                                <td class='vertical-align-middle align-left left bottom bold font-12'>
                                {4}
                                </td>
                                <!-- NET AMOUNT TOTAL -->
                                <td class='vertical-align-middle align-left left bottom bold font-12' >
                                {5}
                                </td>
                                <td class='vertical-align-middle align-left left bottom font-11'>
              
                                </td>
                                <td class='vertical-align-middle align-left left bottom font-11'>
              
                                </td>
                                <td class='vertical-align-middle align-left left bottom right font-11'>
              
                                </td>
                            </tr>
                            ",
                            limitPayroll.Sum(x => double.Parse(x.Salary)).FormalDigit(),
                            limitPayroll.Sum(x => x.HazardPay).FormalDigit(),
                            limitPayroll.Sum(x => x.HWMPCLoan).FormalDigit(),
                            limitPayroll.Sum(x => x.GlobeBill).FormalDigit(),
                            limitPayroll.Sum(x => x.SunBill).FormalDigit(),
                            limitPayroll.Sum(x => x.NetAmountNoMortuary).FormalDigit());
                        //table end
                        pdf.AppendFormat(@"
                        </tbody>
                        </table>
                    ");
                    }

                    if (flag == hazards.Count())
                        pdf.AppendFormat("<div style='page-break-after: always;'></div>");
                }
            }
            pdf.Append(HtmlEnd());

            return pdf.ToString();
        }


        //SUBSISTENCE CONTENT
        public string SubsistenceHtml(List<SubsistenceModel> subsistences, DateTime date)
        {
            var pdf = new StringBuilder();
            pdf.Append(HtmlStart());
            if (subsistences.Count() > 0)
            {
                var ctr = 1;
                var flag = 0;
                int size = 15;
                foreach (var limitPayrolls in subsistences.GroupBy(x => x.SalaryCharge).OrderBy(x => x.Key))
                {
                    ctr = 1;

                    for (int x = 0; x <= limitPayrolls.Count() / size; x++)
                    {
                        var limitPayroll = limitPayrolls.Skip(x * size).Take(size);
                        //table start
                        pdf.AppendFormat(@"
                            <table class='arial table border-collapse regular'>
                                <tbody>
                        ");
                        //HEADER
                        pdf.AppendFormat(@"
                            <tr>
                                <td class='vertical-align-middle align-center bold font-15' colspan='15'>
                                    GENERAL PAYROLL
                                </td>
                            </tr>
                            <tr>
                                <td class='vertical-align-middle align-left font-13' colspan='15'>
                                    <p>
                                    &nbsp;&nbsp;&nbsp;WE HEREBY ACKNOWLEDGE TO HAVE RECEIVED THE <i class='bold'>PAYROLL OF SUBSISTENCE AND LAUNDRY
                                    ALLOWANCE FOR <span style='text-decoration: underline;'>{0}</span></i> THE SUMS THERE IN SPECIFIED OPPOSITE
                                    OUR RESPECTIVE NAMES BEING COMPENSATION, EXCEPT AS NOTED OTHERWISE IN THE REMARKS COLUMN.
                                    </p>
                                </td>
                            </tr>
                            <tr>
                                <td class='font-white'>
                                    none
                                </td>
                            </tr>
                            ", date.ToString("MMMM yyyy").ToUpper());
                        //table header
                        pdf.AppendFormat(@"
                            <!-- TABLE HEADERS -->
                            <tr>
                                <td class='vertical-align-middle align-center left bottom top bold font-12' colspan='3' rowspan='2'>
                                    NAME
                                </td>
                                <td class='vertical-align-middle align-center rotate left bottom top bold font-10' style='width: 10px !important; height: 120px;' rowspan='2'>
                                    INITIAL
                                </td>
                                <td class='vertical-align-middle align-center left bottom top bold font-12' rowspan='2'>
                                    POSITION
                                </td>
                                <td class='vertical-align-middle align-center left bottom top bold font-12' rowspan='2'>
                                    Subsistence<br>
                                    Allowance
                                </td>
                                <td class='vertical-align-middle align-center left bottom top bold font-12' rowspan='2'>
                                    Laundry<br>
                                    Allowance
                                </td>
                                <td class='vertical-align-middle align-center left bottom top bold font-12' rowspan='2'>
                                    LESS:<br>
                                    S/A
                                </td>
                                <td class='vertical-align-middle align-center left bottom top bold font-12' rowspan='2'>
                                    LESS:<br>
                                    L/A
                                </td>
                                <td class='vertical-align-middle align-center left bottom top bold font-12' rowspan='2'>
                                    TOTAL<br>
                                    LV/ABS.
                                </td>
                                <td class='vertical-align-middle align-center left bottom top bold font-12' colspan='2'>
                                    DEDUCTIONS
                                </td>
                                <td class='vertical-align-middle align-center left bottom top  bold font-12' rowspan='2'>
                                    NET<br>
                                    AMOUNT
                                </td>
                                <td class='vertical-align-middle align-center left bottom top bold font-12' rowspan='2'>
                                    # of<br>
                                    days
                                </td>
                                <td class='vertical-align-middle align-center left bottom top right bold font-12' rowspan='2'>
                                    LEAVE/<br>
                                    ABSENT
                                </td>
                            </tr>
                            <tr>
                                <td class='vertical-align-middle align-center left bottom top bold font-11'>
                                    HWMPC
                                </td>
                                <td class='vertical-align-middle align-center left bottom top bold font-11'>
                                    OTHERS
                                </td>
                            </tr>
                            <!-- SECTION -->
                            <tr>
                                <td class='vertical-align-middle align-left bottom bold font-11' colspan='15'>
                                    {0}
                                </td>
                            </tr>
                            ", limitPayrolls.Key);
                        //employee row
                        foreach (var payroll in limitPayroll)
                        {
                            pdf.AppendFormat(@"
                                <!-- EMPLOYEES -->
                                <tr>
                                    <!-- NUMBER -->
                                    <td class='vertical-align-middle align-center left bottom font-11' style='width: 10px !important;'>
                                        {0}
                                    </td>
                                    <!-- LNAME NAME -->
                                    <td class='vertical-align-middle align-left left bottom font-11'>
                                        {1}
                                    </td>
                                    <!-- FNAME NAME -->
                                    <td class='vertical-align-middle align-left left bottom font-11'>
                                        {2}
                                    </td>
                                    <!-- INITIAL -->
                                    <td class='vertical-align-middle align-center left bottom font-11'>
                                        {3}
                                    </td>
                                    <!-- POSITION -->
                                    <td class='vertical-align-middle align-left left bottom font-11'>
                                        {4}
                                    </td>
                                    <!-- SUBSISTENCE -->
                                    <td class='vertical-align-middle align-right left bottom font-11'>
                                        {5}
                                    </td>
                                    <!-- LAUNDRY -->
                                    <td class='vertical-align-middle align-right left bottom font-11'>
                                        {6}
                                    </td>
                                    <!-- LESS S/A -->
                                    <td class='vertical-align-middle align-right left bottom font-11'>
                                        {7}
                                    </d>
                                    <!-- LESS L/A -->
                                    <td class='vertical-align-middle align-right left bottom font-11'>
                                        {8}
                                    </td>
                                    <!-- TOTAL LV/ABS. -->
                                    <td class='vertical-align-middle align-right left bottom font-11'>
                                        {9}
                                    </td>
                                    <!-- HWMPC -->
                                    <td class='vertical-align-middle align-right left bottom font-11'>
                                        {10}
                                    </td>
                                    <!-- OTHERS -->
                                    <td class='vertical-align-middle align-right left bottom font-11'>
                                        {11}
                                    </td>
                                    <!-- NET AMOUNT -->
                                    <td class='vertical-align-middle align-right left bottom font-11'>
                                        {12}
                                    </td>
                                    <!-- #days -->
                                    <td class='vertical-align-middle align-left left bottom font-11'>
                                        {13}
                                    </td>
                                    <!-- REMARKS -->
                                    <td class='vertical-align-middle align-left left right bottom font-11'>
                                        {14}
                                    </td>
                                </tr>
                                ", ctr, payroll.Lname, payroll.Fname, payroll.Mname.Initial(), payroll.Designation,
                                payroll.Subsistence.FormalDigit(), payroll.Laundry.FormalDigit(), payroll.LessSubsistenceAllowance.FormalDigit(),
                                payroll.LessLaundryAllowance.FormalDigit(), payroll.LessTotal.FormalDigit(), payroll.HWMPC.FormalDigit(), 0.00.FormalDigit(),
                                payroll.NetAmount.FormalDigit(), payroll.NoDaysAbsent, payroll.Remarks);
                            ctr++;
                            flag++;
                        }
                        for(int i = ctr; i <= size; i++)
                        {
                            pdf.Append(HtmlSpaceRow(15));
                        }
                        //page total
                        pdf.AppendFormat(@"
                            <!-- PAGE TOTAL-->
                            <tr>
                                <td class='vertical-align-middle align-right left bottom bold font-12' colspan='5'>
                                    PAGE TOTAL
                                </td>
                                <!-- SUBSISTENCE TOTAL -->
                                <td class='vertical-align-middle align-left left bottom bold font-12'>
                                    {0}
                                </td>
                                <!-- LAUNDRY TOTAL -->
                                <td class='vertical-align-middle align-left left bottom bold font-12'>
                                    {1}
                                </td>
                                <!-- LESS SUBSISTENCE TOTAL -->
                                <td class='vertical-align-middle align-left left bottom bold font-12' >
                                    {2}
                                </td>
                                <!-- LESS LAUNDRY TOTAL -->
                                <td class='vertical-align-middle align-left left bottom bold font-12'>
                                    {3}
                                </td>
                                <!-- TOTAL ABS TOTAL -->
                                <td class='vertical-align-middle align-left left bottom bold font-12' >
                                    {4}
                                </td>
                                <!-- HWMPC TOTAL -->
                                <td class='vertical-align-middle align-left left bottom bold font-12'>
                                    {5}
                                </td>
                                <!-- OTHERS TOTAL -->
                                <td class='vertical-align-middle align-left left bottom bold font-12' >
                                    {6}
                                </td>
                                <!-- NET AMOUNT TOTAL -->
                                <td class='vertical-align-middle align-left left bottom bold font-12' >
                                    {7}
                                </td>
                                <td class='vertical-align-middle align-left left bottom font-11'>
              
                                </td>
                                <td class='vertical-align-middle align-left left bottom right font-11'>
              
                                </td>
                            </tr>
                            ",
                            limitPayroll.Sum(x => x.Subsistence).FormalDigit(),
                            limitPayroll.Sum(x => x.Laundry).FormalDigit(),
                            limitPayroll.Sum(x => x.LessSubsistenceAllowance).FormalDigit(),
                            limitPayroll.Sum(x => x.LessLaundryAllowance).FormalDigit(),
                            limitPayroll.Sum(x => x.LessTotal).FormalDigit(),
                            limitPayroll.Sum(x => x.HWMPC).FormalDigit(),
                            0.00.FormalDigit(),
                            limitPayroll.Sum(x => x.NetAmount).FormalDigit());
                        //table end
                        pdf.AppendFormat(@"
                        </tbody>
                        </table>
                    ");
                    if(flag != subsistences.Count())
                        pdf.AppendFormat(@"<div style='page-break-after: always;'></div>");
                    }
                }
            }
            pdf.Append(HtmlEnd());

            return pdf.ToString();
        }


        //LONGEVITY CONTENT
        public string LongevityHtml(List<LongevityModel> longevities, DateTime date)
        {
            var pdf = new StringBuilder();
            pdf.Append(HtmlStart());
            if (longevities.Count() > 0)
            {
                var ctr = 1;
                var flag = 0;
                int size = 17;
                foreach (var limitPayrolls in longevities.GroupBy(x => x.SalaryCharge).OrderBy(x => x.Key))
                {
                    ctr = 1;

                    for (int x = 0; x <= limitPayrolls.Count() / size; x++)
                    {
                        var limitPayroll = limitPayrolls.Skip(x * size).Take(size);
                        //table start
                        pdf.AppendFormat(@"
                            <table class='arial table border-collapse regular'>
                                <tbody>
                        ");
                        //HEADER
                        pdf.AppendFormat(@"
                            <tr>
                                <td class='vertical-align-middle align-center bold font-15' colspan='15'>
                                    GENERAL PAYROLL
                                </td>
                            </tr>
                            <tr>
                                <td class='vertical-align-middle align-left font-13' colspan='15'>
                                    <p>
                                    &nbsp;&nbsp;&nbsp;WE HEREBY ACKNOWLEDGE TO HAVE RECEIVED THE <i class='bold'>LONGEVITY 
                                    PAY FOR THE MONTH OF <span style='text-decoration: underline;'>JUNE 2020</span></i> 
                                    THE SUMS THERE IN SPECIFIED OPPOSITE OUR RESPECTIVE NAMES BEING COMPENSATION, EXCEPT 
                                    AS NOTED OTHERWISE IN THE REMARKS COLUMN.
                                    </p>
                                </td>
                            </tr>
                            <tr>
                                <td class='font-white'>
                                    none
                                </td>
                            </tr>
                            ", date.ToString("MMMM yyyy").ToUpper());
                        //table header
                        pdf.AppendFormat(@"
                            <tr>
                                <td colspan='16'>
                                </td>
                            </tr>
                            <!-- SALARY CHARGE -->
                            <tr>
                                <td class='vertical-align-middle align-left bottom bold font-11' colspan='15'>
                                    {0}
                                </td>
                            </tr>
                            <!-- TABLE HEADERS -->
                            <tr>
                                <td class='vertical-align-middle align-center left bottom top bold font-12'>
                                    NAME
                                </td>
                                <td class='vertical-align-middle align-center left bottom top bold font-10'>
                                    POSITION TITLE
                                </td>
                                <td class='vertical-align-middle align-center left bottom top bold font-12'>
                                    ENTRANCE<br>
                                    TO DUTY
                                </td>
                                <td class='vertical-align-middle align-center left bottom top bold font-12'>
                                    SALARY
                                </td>
                                <td class='vertical-align-middle align-center left bottom top bold font-12'>
                                    5%<br>
                                    LP(1)
                                </td>
                                <td class='vertical-align-middle align-center left bottom top bold font-12'>
                                    SALARY
                                </td>
                                <td class='vertical-align-middle align-center left bottom top bold font-12'>
                                    5%<br>
                                    LP(2)
                                </td>
                                <td class='vertical-align-middle align-center left bottom top bold font-12'>
                                    SALARY
                                </td>
                                <td class='vertical-align-middle align-center left bottom top bold font-12'>
                                    5%<br>
                                    LP(3)
                                </td>
                                <td class='vertical-align-middle align-center left bottom top bold font-12'>
                                    SALARY
                                </td>
                                <td class='vertical-align-middle align-center left bottom top bold font-12'>
                                    5%<br>
                                    LP(4)
                                </td>
                                <td class='vertical-align-middle align-center left bottom top bold font-12'>
                                    SALARY
                                </td>
                                <td class='vertical-align-middle align-center left bottom top bold font-12'>
                                    5%<br>
                                    LP(5)
                                </td>
                                <td class='vertical-align-middle align-center left bottom top bold font-12'>
                                    SALARY
                                </td>
                                <td class='vertical-align-middle align-center left bottom top bold font-12'>
                                    DISAL<br>
                                    LOWANCE
                                </td>
                                <td class='vertical-align-middle align-center left bottom right top bold font-12'>
                                    TOTAL<br>
                                    LP/MO
                                </td>
                            </tr>
                        ", limitPayrolls.Key);
                        //employee row
                        foreach (var payroll in limitPayroll)
                        {
                            pdf.AppendFormat(
                                @"
                                <!-- EMPLOYEES -->
                                <tr>
                                    <!-- NAME -->
                                    <td class='vertical-align-middle align-left left bottom font-11' style='max-width: 100%; white-space: nowrap;'>
                                        {0}
                                    </td>
                                    <!-- POSITION TITLE -->
                                    <td class='vertical-align-middle align-left left bottom font-11'>
                                        {1}
                                    </td>
                                    <!-- ENTRANCE TO DUTY -->
                                    <td class='vertical-align-middle align-left left bottom font-11'>
                                        {2}
                                    </td>
                                    <!-- SALARY -->
                                    <td class='vertical-align-middle align-left left bottom font-11'>
                                        {3}
                                    </td>
                                    <!-- LP1 -->
                                    <td class='vertical-align-middle align-left left bottom font-11'>
                                        {4}
                                    </td>
                                    <!-- SALARY -->
                                    <td class='vertical-align-middle align-left left bottom font-11'>
                                        {5}
                                    </td>
                                    <!-- LP2 -->
                                    <td class='vertical-align-middle align-left left bottom font-11'>
                                        {6}
                                    </td>
                                    <!-- SALARY -->
                                    <td class='vertical-align-middle align-left left bottom font-11'>
                                        {7}
                                    </td>
                                    <!-- LP3 -->
                                    <td class='vertical-align-middle align-left left bottom font-11'>
                                        {8}
                                    </td>
                                    <!-- SALARY -->
                                    <td class='vertical-align-middle align-left left bottom font-11'>
                                        {9}
                                    </td>
                                    <!-- LP4 -->
                                    <td class='vertical-align-middle align-left left bottom font-11'>
                                        {10}
                                    </td>
                                    <!-- SALARY -->
                                    <td class='vertical-align-middle align-left left bottom font-11'>
                                        {11}
                                    </td>
                                    <!-- LP5 -->
                                    <td class='vertical-align-middle align-left left bottom font-11'>
                                        {12}
                                    </td>
                                    <!-- SALARY -->
                                    <td class='vertical-align-middle align-left left bottom font-11'>
                                        {13}
                                    </td>
                                    <!-- DISALLOWANCE -->
                                    <td class='vertical-align-middle align-left left bottom font-11'>
                                        {14}
                                    </td>
                                    <!-- TOTAL LP/MO -->
                                    <td class='vertical-align-middle align-left left right bottom font-11'>
                                        {15}
                                    </td>
                                </tr>", payroll.Fullname, payroll.Designation, payroll.EntranceToDuty,
                                payroll.Salaries[0].Salary.FormalDigit(), payroll.Salaries[0].LP.FormalDigit(),
                                payroll.Salaries[1].Salary.FormalDigit(), payroll.Salaries[1].LP.FormalDigit(),
                                payroll.Salaries[2].Salary.FormalDigit(), payroll.Salaries[2].LP.FormalDigit(),
                                payroll.Salaries[3].Salary.FormalDigit(), payroll.Salaries[3].LP.FormalDigit(),
                                payroll.Salaries[4].Salary.FormalDigit(), payroll.Salaries[4].LP.FormalDigit(),
                                "", payroll.Disallowance.FormalDigit(), payroll.TotalLPMO.FormalDigit());
                            ctr++;
                            flag++;
                        }
                        for (int i = ctr; i <= size; i++)
                        {
                            pdf.Append(HtmlSpaceRow(16));
                        }
                        //page total
                        pdf.AppendFormat(@"
                            <!-- PAGE TOTAL-->
                            <tr>
                                <td class='vertical-align-middle align-left left bold bottom font-12'>
                                    PAGE TOTAL
                                </td>
                                <td class='vertical-align-middle align-left left bottom font-11'>
                                </td>
                                <td class='vertical-align-middle align-left left bottom font-11'>
                                </td>
                                <td class='vertical-align-middle align-left left bottom font-11'>
                                </td>
                                <td class='vertical-align-middle align-left left bottom font-11'>
                                </td>
                                <td class='vertical-align-middle align-left left bottom font-11'>
                                </td>
                                <td class='vertical-align-middle align-left left bottom font-11'>
                                </td>
                                <td class='vertical-align-middle align-left left bottom font-11'>
                                </td>
                                <td class='vertical-align-middle align-left left bottom font-11'>
                                </td>
                                <td class='vertical-align-middle align-left left bottom font-11'>
                                </td>
                                <td class='vertical-align-middle align-left left bottom font-11'>
                                </td>
                                <td class='vertical-align-middle align-left left bottom font-11'>
                                </td>
                                <td class='vertical-align-middle align-left left bottom font-11'>
                                </td>
                                <td class='vertical-align-middle align-left left bottom font-11'>
                                </td>
                                <td class='vertical-align-middle align-left left right bottom font-11'>
                                </td>
                                <!-- TOTAL -->
                                <td class='vertical-align-middle align-left left bold bottom right font-12'>
                                    {0}
                                </td>
                            </tr>",
                            limitPayroll.Sum(x => x.TotalLPMO).FormalDigit());
                        //table end
                        pdf.AppendFormat(@"
                        </tbody>
                        </table>
                    ");
                        if (flag != longevities.Count())
                            pdf.AppendFormat(@"<div style='page-break-after: always;'></div>");
                    }
                }
            }
            pdf.Append(HtmlEnd());

            return pdf.ToString();
        }
        #endregion

        #region HELPERS
        public byte[] SetPDF(string regularPayrolls, string document_title, Orientation orientation, PaperKind paperSize)
        {
            new CustomAssemblyLoadContext().LoadUnmanagedLibrary(Path.Combine(Directory.GetCurrentDirectory(), "libwkhtmltox.dll"));
            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = orientation,
                PaperSize = paperSize,
                Margins = new MarginSettings { Top = 1.27, Bottom = 1.27, Left = 1.27, Right = 1.27, Unit = Unit.Centimeters },
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
        public string HtmlSpaceRow(int ctr)
        {
            var pdf = new StringBuilder();
            pdf.Append("<tr>");
            for(int x= 0; x<ctr; x++)
            {
                pdf.Append(@"
                    <td class='border font-white'>
                        0
                    </ td >");
            }
            pdf.Append("</tr>");
            return pdf.ToString();
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
