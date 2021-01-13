using DinkToPdf;
using DinkToPdf.Contracts;
using DOHPayroll.Databases;
using DOHPayroll.PayrollModels;
using DOHPayroll.Resources;
using DOHPayroll.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOHPayroll.Controllers
{
    [Authorize(Policy = "Administrator")]
    public class RemittanceController : Controller
    {
        private IConverter _converter;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public RemittanceController(IConverter converter)
        {
            _converter = converter;
        }

        #region REGULAR REMITTANCE
        [HttpPost]
        public async Task<IActionResult> RegularRemittance(string date, string type)
        {
            GetStartDay(date);
            if(type != "pagibig")
            {
                var remittanceList = await PayrollDatabase.Instance.GetRemittanceByDateAsync(StartDate, type);

                if (remittanceList.Count() > 0)
                {
                    var title = "";
                    if (type == "phic")
                        title = "Regular PhilHealth Remittance";
                    else
                        title = "Regular HWMPC Remittance";
                    var content = RegularRemittanceHtml(remittanceList, StartDate, title);
                    var file = SetPDF(content, title, Orientation.Portrait);

                    return File(file, "application/pdf");
                }
                else
                {
                    return View("~/Views/Report/NothingToGenerate.cshtml");
                }
            }
            else
            {
                GetStartDay(date);
                var pagibigList = await PayrollDatabase.Instance.GetPagibigByDateAsync(StartDate);

                var stream = SetExcel(pagibigList,"REGULAR");

                stream.Position = 0;
                string excelName = "DOH RO7 " + StartDate.Year + "" + StartDate.ToString("MM") + " - REGULAR (MDS - CONTRIBUTION AND LOAN PAYMENT).xlsx";

                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
        }
        #endregion

        #region JOB ORDER REMITTANCE
        [HttpPost]
        public async Task<IActionResult> JoRemittance(string date, string type)
        {
            GetStartDay(date);
            if (type != "pagibig")
            {
                var remittanceList = await PayrollDatabase.Instance.GetRemittanceByDateAsync(StartDate, EndDate, type);

                if (remittanceList.Count() > 0)
                {
                    var title = "";
                    if (type == "phic")
                        title = "Job Order PhilHealth Remittance";
                    else
                        title = "Job Order HWMPC Remittance";
                    var content = JoRemittanceHtml(remittanceList, StartDate, EndDate);
                    var file = SetPDF(content, title, Orientation.Portrait);

                    return File(file, "application/pdf");
                }
                else
                {
                    return View("~/Views/Report/NothingToGenerate.cshtml");
                }
            }
            else
            {
                GetStartDay(date);
                var pagibigList = await PayrollDatabase.Instance.GetPagibigByDateAsync(StartDate,EndDate);

                var stream = SetExcel(pagibigList,"JOB ORDER");

                stream.Position = 0;
                string excelName = "DOH RO7 " + StartDate.Year + "" + StartDate.ToString("MM") + " - JOB ORDER (MDS - CONTRIBUTION AND LOAN PAYMENT).xlsx";

                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
        }
        /*public IActionResult Test(string date, string type)
        {

            switch (type)
            {
                case "phic":
                    {
                        var phicList = await PayrollDatabase.Instance.GetPhicByDateAsync(StartDate, EndDate);

                        if (phicList.Count() > 0)
                        {
                            var content = JoPhicHtml(phicList, StartDate, EndDate);
                            var file = SetPDF(content, "Job Order PhilHealth Remittance", Orientation.Portrait);

                            return File(file, "application/pdf");
                        }
                        else
                        {
                            return View("~/Views/Report/NothingToGenerate.cshtml");
                        }
                    }
                case "pagibig":
                    {
                        string startDate = date.Split("-")[0];
                        string endDate = startDate;
                        try
                        {
                            endDate = date.Split("-")[1];
                        }
                        catch (IndexOutOfRangeException)
                        { }
                        int startMonth = int.Parse(startDate.Split("/")[0]);
                        int startYear = int.Parse(startDate.Split("/")[1]);
                        int endMonth = int.Parse(endDate.Split("/")[0]);
                        int endYear = int.Parse(endDate.Split("/")[1]);
                        var pagibigList = await PayrollDatabase.Instance.GetPagibigByDateAsync(new DateTime(startYear, startMonth, 1),
                            new DateTime(endYear, endMonth, DateTime.DaysInMonth(endYear, endMonth)));

                        if (pagibigList.Count() > 0)
                        {
                            var content = JoPagibigHtml(pagibigList, new DateTime(startYear, startMonth, 1), new DateTime(endYear, endMonth, 1));
                            var file = SetPDF(content, "Job Order Pagibig Remittance", Orientation.Landscape);

                            return File(file, "application/pdf");
                        }
                        else
                        {
                            return View("~/Views/Report/NothingToGenerate.cshtml");
                        }
                    }
            }
        }*/
        #endregion

        #region REGULAR PHILHEALTH REMITTANCE

        public string RegularRemittanceHtml(List<RemittanceModel> models, DateTime date, string title)
        {
            var pdf = new StringBuilder();
            pdf.Append(HtmlStart());
            if (models.Count() > 0)
            {
                var ctr = 1;
                int size = 45;
                for (int x = 0; x <= models.GroupBy(x => x.Fullname).Count() / size; x++)
                {
                    var pageTotal = new List<double>();
                    var limitPayroll = models.OrderBy(x=>x.Lname).GroupBy(x=>x.Fullname).Skip(x * size).Take(size);
                    //table start
                    pdf.AppendFormat(@"
                    <table class='arial table font-10 border-collapse regular'>
                        <tbody>");
                    //HEADER
                    pdf.AppendFormat(@"
                        <tr>
                            <th class='vertical-align-middle align-center font-12' colspan='7'>
                                Republic of Philippines
                            </th>
                        </tr>
                        <tr>
                            <th class='vertical-align-middle align-center font-12' colspan='7'>
                                Department of Health
                            </th>
                        </tr>
                        <tr>
                            <th class='vertical-align-middle bold align-center font-12' colspan='7'>
                                Regional Office VIICENTRAL VISAYAS CENTER for HEALTH DEVELOPMENT
                            </th>
                        </tr>
                        <tr>
                            <th class='vertical-align-middle align-center font-12' colspan='7'>
                                Osmeña Boulevard, Sambag II, Cebu City, 6000 Philippines
                            </th>
                        </tr>
                        <tr>
                            <th class='vertical-align-middle align-center font-12' colspan='7'>
                                Regional Director’s Office Tel. No. (032) 253-6355 Fax No. (032) 254-0109
                            </th>
                        </tr>
                        <tr>
                            <th class='vertical-align-middle align-center font-12' colspan='7'>
                                Official Website http://www.ro7.doh.gov.ph email Address: dohro7@gmail.com
                            </th>
                        </tr>
                        <tr>
                            <th class='vertical-align-middle align-center font-12' colspan='7'>
                                {0} REMITTANCE
                            </th>
                        </tr>
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
                                PHIC NO
                            </td>
                            <td class='vertical-align-middle align-center border bold font-12'>
                                LAST NAME
                            </td>
                            <td class='vertical-align-middle align-center border bold font-12'>
                                FIRST NAME
                            </td>
                            <td class='vertical-align-middle align-center border bold font-12'>
                                MIDDLE NAME
                            </td>
                            <td class='vertical-align-middle align-center border bold font-12'>
                                AMOUNT
                            </td>
                            <td class='vertical-align-middle align-center border bold font-12'>
              
                            </td>
                        </tr>
                    ");
                    //employee row
                    foreach (var payroll in limitPayroll)
                    {
                        pdf.AppendFormat(@"
                            <!-- EMPLOYEES -->
                            <!-- 1ST ROW -->
                            <tr>
                                <!-- NUMBER -->
                                <td class='vertical-align-middle align-center left bottom font-11'>
                                    {0}
                                </td>
                                <!-- PHIC NO -->
                                <td class='vertical-align-middle align-center left bottom font-11'>
                                    {1}
                                </td>
                                <!-- LAST NAME -->
                                <td class='vertical-align-middle align-center left bottom font-11'>
                                    {2}
                                </td>
                                <!-- FIRST NAME -->
                                <td class='vertical-align-middle align-center left bottom font-11'>
                                    {3}
                                </td>
                                <!-- MIDDLE NAME -->
                                <td class='vertical-align-middle align-center left bottom font-11'>
                                    {4}
                                </td>
                                <!-- AMOUNT -->
                                <td class='vertical-align-middle align-center left bottom font-11'>
                                    {5}
                                </td> 
                                <!-- IDK WHAT DIS IS -->
                                <td class='vertical-align-middle align-center left bottom right font-11' style='color: white !important;'>
                                    {6}
                                </td>
                            </tr>
                        ",ctr, payroll.First().PhicNo, payroll.First().Lname, payroll.First().Fname, payroll.First().Mname,
                        payroll.Sum(x=>x.Amount).FormalDigit(),"white");
                        pageTotal.Add(payroll.Sum(x => x.Amount));
                        ctr++;
                    }
                    //page total
                    pdf.AppendFormat(@"
                        <!-- PAGE TOTAL -->
                        <tr>
                            <!-- NUMBER -->
                            <td class='vertical-align-middle align-right left bottom bold font-12' colspan='5'>
                                Page Total: 
                            </td>
                            <!-- TOTAL -->
                            <td class='vertical-align-middle align-center left bottom bold font-11'>
                                {0}
                            </td> 
                            <!-- IDK WHAT DIS IS -->
                            <td class='vertical-align-middle align-center left bottom right bold font-11'>
              
                            </td>
                        </tr>
                    ", pageTotal.Sum().FormalDigit());
                    //page break
                    pdf.AppendFormat("<div style='page-break-after: always;'></div>");
                }
                //total
                pdf.AppendFormat(@"
                    <tr>
                        <td class='right' colspan='6'>
                        </td>
                        <td class='right'>
                        </td>
                    </tr>
                    <tr>
                        <!-- NUMBER -->
                        <td>
                        </td>
                        <td class='vertical-align-middle align-left bold font-11' colspan='3'>
                            Prepared by:
                        </td>
                        <td class='vertical-align-middle align-left bold right font-11' colspan='2'>
                            Prepared by:
                        </td>
                        <td class='right'>
                        </td>
                    </tr>
                    <tr>
                        <td class='right' colspan='6'>
                        </td>
                        <td class='right'>
                        </td>
                    </tr>
                    <tr>
                        <td>
                        </td>
                        <td class='vertical-align-middle align-left font-11' colspan='3'>
                            Sailen S. Papa
                        </td>
                        <td class='vertical-align-middle align-left right font-11' colspan='2'>
                            Theresa Q. Tragico
                        </td>
                        <td class='right'>
                        </td>
                    </tr>
                    <tr>
                        <td>
                        </td>
                        <td class='vertical-align-middle align-left font-11' colspan='3'>
                            Administrative Assistant I
                        </td>
                        <td class='vertical-align-middle align-left right font-11' colspan='2'>
                            Administrative Officer V
                        </td>
                        <td class='right'>
                        </td>
                    </tr>
                ");
                foreach (var disbursement_type in models.GroupBy(x => x.DisbursementType))
                {
                    //disbursements
                    pdf.AppendFormat(@"

                    <tr>
                        <td colspan='5'>
                        </td>
                        <td class='vertical-align-middle align-center right font-11'>
                            {0}
                        </td>
                        <td class='vertical-align-middle align-center right font-9' style='word-wrap: break-word;'>
                            {1}
                        </td>
                    </tr>
                ", models.Where(x => x.DisbursementType == disbursement_type.Key).Sum(x => x.Amount).FormalDigit(),
                    disbursement_type.Key);
                }
                pdf.AppendFormat(@"
                    <tr>
                        <td colspan='5'>
                        </td>
                        <td class='vertical-align-middle align-center bottom right font-11'>
                            {0}
                        </td>
                        <td class='vertical-align-middle align-center bottom right font-9'>
              
                        </td>
                    </tr>
                    ", models.Sum(x => x.Amount).FormalDigit());
                //table end
                pdf.AppendFormat(@"
                        </tbody>
                        </table>
                    ");
            }
            pdf.Append(HtmlEnd());

            return pdf.ToString();
        }



        #endregion

        #region JOB ORDER PHILHEALTH REMITTANCE
        public string JoRemittanceHtml(List<RemittanceModel> models, DateTime startDate, DateTime endDate)
        {
            var pdf = new StringBuilder();
            pdf.Append(HtmlStart());
            if (models.Count() > 0)
            {
                var ctr = 1;
                int size = 45;
                var date = startDate.Month == endDate.Month ? startDate.ToString("MMMM yyyy") :
                    startDate.ToString("MMMM yyyy") + " - " + endDate.ToString("MMMM yyyy");
                for (int x = 0; x <= models.GroupBy(x => x.Fullname).Count() / size; x++)
                {
                    var pageTotal = new List<double>();
                    var limitPayroll = models.OrderBy(x => x.Lname).GroupBy(x => x.Fullname).Skip(x * size).Take(size);
                    //table start
                    pdf.AppendFormat(@"
                    <table class='arial table font-10 border-collapse regular'>
                        <tbody>");
                    //HEADER
                    pdf.AppendFormat(@"
                        <tr>
                            <th class='vertical-align-middle align-center font-12' colspan='7'>
                                Republic of Philippines
                            </th>
                        </tr>
                        <tr>
                            <th class='vertical-align-middle align-center font-12' colspan='7'>
                                Department of Health
                            </th>
                        </tr>
                        <tr>
                            <th class='vertical-align-middle bold align-center font-12' colspan='7'>
                                Regional Office VIICENTRAL VISAYAS CENTER for HEALTH DEVELOPMENT
                            </th>
                        </tr>
                        <tr>
                            <th class='vertical-align-middle align-center font-12' colspan='7'>
                                Osmeña Boulevard, Sambag II, Cebu City, 6000 Philippines
                            </th>
                        </tr>
                        <tr>
                            <th class='vertical-align-middle align-center font-12' colspan='7'>
                                Regional Director’s Office Tel. No. (032) 253-6355 Fax No. (032) 254-0109
                            </th>
                        </tr>
                        <tr>
                            <th class='vertical-align-middle align-center font-12' colspan='7'>
                                Official Website http://www.ro7.doh.gov.ph email Address: dohro7@gmail.com
                            </th>
                        </tr>
                        <tr>
                            <th class='vertical-align-middle align-center font-12' colspan='7'>
                                {0} REMITTANCE
                            </th>
                        </tr>
                    ", date);
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
                                PHIC NO
                            </td>
                            <td class='vertical-align-middle align-center border bold font-12'>
                                LAST NAME
                            </td>
                            <td class='vertical-align-middle align-center border bold font-12'>
                                FIRST NAME
                            </td>
                            <td class='vertical-align-middle align-center border bold font-12'>
                                MIDDLE NAME
                            </td>
                            <td class='vertical-align-middle align-center border bold font-12'>
                                AMOUNT
                            </td>
                            <td class='vertical-align-middle align-center border bold font-12'>
              
                            </td>
                        </tr>
                    ");
                    //employee row
                    foreach (var payroll in limitPayroll)
                    {
                        pdf.AppendFormat(@"
                            <!-- EMPLOYEES -->
                            <!-- 1ST ROW -->
                            <tr>
                                <!-- NUMBER -->
                                <td class='vertical-align-middle align-center left bottom font-11'>
                                    {0}
                                </td>
                                <!-- PHIC NO -->
                                <td class='vertical-align-middle align-center left bottom font-11'>
                                    {1}
                                </td>
                                <!-- LAST NAME -->
                                <td class='vertical-align-middle align-center left bottom font-11'>
                                    {2}
                                </td>
                                <!-- FIRST NAME -->
                                <td class='vertical-align-middle align-center left bottom font-11'>
                                    {3}
                                </td>
                                <!-- MIDDLE NAME -->
                                <td class='vertical-align-middle align-center left bottom font-11'>
                                    {4}
                                </td>
                                <!-- AMOUNT -->
                                <td class='vertical-align-middle align-center left bottom font-11'>
                                    {5}
                                </td> 
                                <!-- IDK WHAT DIS IS -->
                                <td class='vertical-align-middle align-center left bottom right font-11' style='color: white !important;'>
                                    {6}
                                </td>
                            </tr>
                        ", ctr, payroll.First().PhicNo, payroll.First().Lname, payroll.First().Fname, payroll.First().Mname,
                        payroll.Sum(x => x.Amount).FormalDigit(), "white");
                        pageTotal.Add(payroll.Sum(x => x.Amount));
                        ctr++;
                    }
                    //page total
                    pdf.AppendFormat(@"
                        <!-- PAGE TOTAL -->
                        <tr>
                            <!-- NUMBER -->
                            <td class='vertical-align-middle align-right left bottom bold font-12' colspan='5'>
                                Page Total: 
                            </td>
                            <!-- TOTAL -->
                            <td class='vertical-align-middle align-center left bottom bold font-11'>
                                {0}
                            </td> 
                            <!-- IDK WHAT DIS IS -->
                            <td class='vertical-align-middle align-center left bottom right bold font-11'>
              
                            </td>
                        </tr>
                    ", pageTotal.Sum().FormalDigit());
                    //page break
                    pdf.AppendFormat("<div style='page-break-after: always;'></div>");
                }
                //total
                pdf.AppendFormat(@"
                    <tr>
                        <td class='right' colspan='6'>
                        </td>
                        <td class='right'>
                        </td>
                    </tr>
                    <tr>
                        <!-- NUMBER -->
                        <td>
                        </td>
                        <td class='vertical-align-middle align-left bold font-11' colspan='3'>
                            Prepared by:
                        </td>
                        <td class='vertical-align-middle align-left bold right font-11' colspan='2'>
                            Prepared by:
                        </td>
                        <td class='right'>
                        </td>
                    </tr>
                    <tr>
                        <td class='right' colspan='6'>
                        </td>
                        <td class='right'>
                        </td>
                    </tr>
                    <tr>
                        <td>
                        </td>
                        <td class='vertical-align-middle align-left font-11' colspan='3'>
                            Heidi C. Borbon
                        </td>
                        <td class='vertical-align-middle align-left right font-11' colspan='2'>
                            Theresa Q. Tragico
                        </td>
                        <td class='right'>
                        </td>
                    </tr>
                    <tr>
                        <td>
                        </td>
                        <td class='vertical-align-middle align-left font-11' colspan='3'>
                            Administrative Assistant I
                        </td>
                        <td class='vertical-align-middle align-left right font-11' colspan='2'>
                            Administrative Officer V
                        </td>
                        <td class='right'>
                        </td>
                    </tr>
                ");
                foreach(var disbursement_type in models.GroupBy(x => x.DisbursementType))
                {
                    //disbursements
                    pdf.AppendFormat(@"

                    <tr>
                        <td colspan='5'>
                        </td>
                        <td class='vertical-align-middle align-center right font-11'>
                            {0}
                        </td>
                        <td class='vertical-align-middle align-center right font-9' style='word-wrap: break-word;'>
                            {1}
                        </td>
                    </tr>
                ", models.Where(x => x.DisbursementType == disbursement_type.Key).Sum(x => x.Amount).FormalDigit(),
                    disbursement_type.Key);
                }
                pdf.AppendFormat(@"
                    <tr>
                        <td colspan='5'>
                        </td>
                        <td class='vertical-align-middle align-center bottom right font-11'>
                            {0}
                        </td>
                        <td class='vertical-align-middle align-center bottom right font-9'>
              
                        </td>
                    </tr>
                    ", models.Sum(x => x.Amount).FormalDigit());
                //table end
                pdf.AppendFormat(@"
                        </tbody>
                        </table>
                    ");
            }
            pdf.Append(HtmlEnd());

            return pdf.ToString();
        }
        #endregion

        #region JOB ORDER PHILHEALTH REMITTANCE

        public string JoPagibigHtml(List<PagibigModel> models, DateTime startDate, DateTime endDate)
        {
            var pdf = new StringBuilder();
            pdf.Append(HtmlStart());
            if (models.Count() > 0)
            {
                var ctr = 1;
                int size = 20;
                var date = startDate.Month == endDate.Month ? startDate.ToString("MMMM yyyy") :
                    startDate.ToString("MMMM yyyy") + " - " + endDate.ToString("MMMM yyyy");
                for (int x = 0; x <= models.GroupBy(x => x.Fullname).Count() / size; x++)
                {
                    var pageTotalEE = new List<double>();
                    var pageTotalER = new List<double>();
                    var limitPayroll = models.OrderBy(x => x.Lname).GroupBy(x => x.Fullname).Skip(x * size).Take(size);
                    //table start
                    pdf.AppendFormat(@"
                    <table class='arial table font-10 border-collapse regular'>
                        <tbody>");
                    //HEADER
                    pdf.AppendFormat(@"
                        <!-- START HEADER-->
                        <tr>
                            <td colspan='11'>
                            </td>
                        </tr>
                        <tr>
                            <td class='vertical-align-middle align-left font-12' colspan='2'>
                                EmployerID
                            </td>
                            <td class='vertical-align-middle align-left bold font-12' colspan='7'>
                                2088-3884-0005
                            </td>
                            <td class='vertical-align-middle align-right font-12' colspan='2'>
                                HQP-PFF-053
                            </td>
                        </tr>
                        <tr>
                            <td class='vertical-align-middle align-left font-12' colspan='2'>
                                EmployerName
                            </td>
                            <td class='vertical-align-middle align-left bold font-12' colspan='9'>
                                JOB ORDERS - DEPARTMENT OF HEALTH R7
                            </td>
                        </tr>
                        <tr>
                            <td class='vertical-align-middle align-left font-12' colspan='2'>
                                Address
                            </td>
                            <td class='vertical-align-middle align-left bold font-12' colspan='9'>
                                OSMENA BLVD.,CEBU CITY
                            </td>
                        </tr>
                    ", date);
                    //table header
                    pdf.AppendFormat(@"
                        <!-- TABLE HEADERS -->
                        <tr>
                            <td class='vertical-align-middle align-center left bottom top bold font-12'>
                                PAG-IBIG ID
                            </td>
                            <td class='vertical-align-middle align-center left bottom top bold font-12'>
                                ACCOUNT NO
                            </td>
                            <td class='vertical-align-middle align-center left bottom top bold font-12'>
                                MEMBERSHIP<br>
                                PROGRAM
                            </td>
                            <td class='vertical-align-middle align-center left bottom top bold font-12'>
                                LAST NAME
                            </td>
                            <td class='vertical-align-middle align-center left bottom top bold font-12'>
                                FIRST NAME
                            </td>
                            <td class='vertical-align-middle align-center rotate left bottom top bold font-10' style='width: 10px!important; height: 120px;'>
                                NAME EXTENSION
                            </td>
                            <td class='vertical-align-middle align-center left bottom top bold font-12'>
                                MIDDLE NAME
                            </td>
                            <td class='vertical-align-middle align-center left bottom top bold font-12'>
                                PERCOV<br>
                                (YYYYMM)
                            </td>
                            <td class='vertical-align-middle align-center left bottom top bold font-12'>
                                EE SHARE
                            </td>
                            <td class='vertical-align-middle align-center left bottom top bold font-12'>
                                ER SHARE
                            </td>
                            <td class='vertical-align-middle align-center left bottom top right bold font-12'>
                                REMARKS
                            </td>
                        </tr>
                    ");
                    //employee row
                    foreach (var pagibig in limitPayroll)
                    {
                        pdf.AppendFormat(@"
                            <!-- EMPLOYEES -->
                            <tr>
                            <!-- PAGIBIG NO -->
                                <td class='vertical-align-middle align-left left bottom font-11'>
                                    {0}
                                </td>
                                <!-- ACCOUNT NO -->
                                <td class='vertical-align-middle align-left left bottom font-11'>
                                    {1}
                                </td>
                                <!-- MEMBERSHIP PROGRAM -->
                                <td class='vertical-align-middle align-left left bottom font-11'>
                                    {2}
                                </td>
                                <!-- LNAME -->
                                <td class='vertical-align-middle align-left left bottom font-11'>
                                    {3}
                                </td>
                                <!-- FNAME -->
                                <td class='vertical-align-middle align-left left bottom font-11'>
                                    {4}
                                </td>
                                <!-- NAME EXTENSION -->
                                <td class='vertical-align-middle align-left left bottom font-11'>
                                    {5}
                                </td>
                                <!-- MNAME -->
                                <td class='vertical-align-middle align-left left bottom font-11'>
                                    {6}
                                </td>
                                <!-- PERCOV -->
                                <td class='vertical-align-middle align-left left bottom font-11'>
                                    {7}
                                </td>
                                <!-- EE SHARE -->
                                <td class='vertical-align-middle align-left left bottom font-11'>
                                    {8}
                                </td>
                                <!-- ER SHARE -->
                                <td class='vertical-align-middle align-left left bottom font-11'>
                                    {9}
                                </td>
                                <!-- REMARKS -->
                                <td class='vertical-align-middle align-left right left bottom font-11'>
                                    {10}
                                </td>
                            </tr>
                        ", pagibig.First().PagibigID, pagibig.First().PagibigAccNo, pagibig.First().MembershipPro,
                        pagibig.First().Lname, pagibig.First().Fname, pagibig.First().NameExtension,
                        pagibig.First().Mname, pagibig.First().PerCov, pagibig.Sum(x=>x.EEShare).FormalDigit(),
                        pagibig.Sum(x=>x.ERShare).FormalDigit(), pagibig.First().Remarks);
                        pageTotalEE.Add(pagibig.Sum(x => x.EEShare));
                        pageTotalER.Add(pagibig.Sum(x => x.ERShare));
                        ctr++;
                    }
                    //page total
                    pdf.AppendFormat(@"
                        <tr>
                            <td class='vertical-align-middle align-left left bottom bold font-12' colspan='8'>
                                PAGE TOTAL
                            </td>
                            <!-- EE SHARE TOTAL -->
                            <td class='vertical-align-middle align-left left bottom bold font-12'>
                                {0}
                            </td>
                            <!-- ER SHARE TOTAL -->
                            <td class='vertical-align-middle align-left left bottom bold font-12' >
                                {1}
                            </td>
                            <!-- REMARKS -->
                            <td class='vertical-align-middle align-left right left bottom font-11'>
              
                            </td>
                        </tr>
                    ", pageTotalEE.Sum().FormalDigit(), pageTotalER.Sum().FormalDigit());
                    //page break
                    pdf.AppendFormat("<div style='page-break-after: always;'></div>");
                }
                //total
                pdf.AppendFormat(@"
                    <tr>
                        <td class='vertical-align-middle align-left left bottom bold font-12' colspan='8'>
                            TOTAL
                        </td>
                        <!-- EE SHARE TOTAL -->
                        <td class='vertical-align-middle align-left left bottom bold font-12'>
                            {0}
                        </td>
                        <!-- ER SHARE TOTAL -->
                        <td class='vertical-align-middle align-left left bottom bold font-12' >
                            {1}
                        </td>
                        <!-- REMARKS -->
                        <td class='vertical-align-middle align-left right left bottom font-11'>
                        </td>
                ", models.Sum(x=>x.EEShare).FormalDigit(), models.Sum(x=>x.ERShare).FormalDigit());
                //table end
                pdf.AppendFormat(@"
                        </tbody>
                        </table>
                    ");
            }
            pdf.Append(HtmlEnd());

            return pdf.ToString();
        }



        #endregion

        #region HELPERS
        public MemoryStream SetExcel(List<PagibigModel> pagibigList, string job_status)
        {
            var stream = new MemoryStream();
            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets.Add("Monthly Contributions_" + StartDate.ToString("MMMM yyyy").ToUpper());
                worksheet.Cells.Style.Font.Size = 10;
                worksheet.Cells.Style.Font.Name = "Arial";
                #region Start Excel
                worksheet.Cells[1, 1].Value = "EmployerID";
                worksheet.Cells[1, 2].Value = "2088-3884-0005";
                worksheet.Cells[1, 2, 1, 9].Merge = true;
                worksheet.Cells[1, 10].Value = "HQP-PFF-053";
                worksheet.Cells[1, 10, 1, 11].Merge = true;
                worksheet.Cells[2, 1].Value = "EmployerName";
                worksheet.Cells[2, 2].Value = job_status + " - DEPARTMENT OF HEALTH R7";
                worksheet.Cells[2, 2, 2, 9].Merge = true;
                worksheet.Cells[3, 1].Value = "Address";
                worksheet.Cells[3, 2].Value = "OSMENA BLVD.,CEBU CITY";
                worksheet.Cells[3, 2, 3, 9].Merge = true;
                #endregion
                #region Table Header
                worksheet.Cells[4, 1].Value = "Pag-IBIGID";
                worksheet.Cells[4, 2].Value = "ACCO\nUNT\nNO";
                worksheet.Cells[4, 3].Value = "MEMB\nERSHI\nP\nPROG\nRAM";
                worksheet.Cells[4, 4].Value = "LASTNAME";
                worksheet.Cells[4, 5].Value = "FIRST NAME";
                worksheet.Cells[4, 6].Value = "NAME\nEXTENSIO\nN (JR, II)";
                worksheet.Cells[4, 6].Style.Font.Size = 8;
                worksheet.Cells[4, 7].Value = "MIDDLE NAME";
                worksheet.Cells[4, 8].Value = "PERCOV\n(YYYY\nMM)";
                worksheet.Cells[4, 9].Value = "EE SHARE";
                worksheet.Cells[4, 10].Value = "ER SHARE";
                worksheet.Cells[4, 11].Value = "REMARKS";
                worksheet.Cells[4, 1, 4, 11].Style.Font.Name = "Times New Roman";
                worksheet.Cells[4, 1, 4, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[4, 1, 4, 11].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Cells[4, 1, 4, 11].Style.Font.Bold = true;
                worksheet.Cells[4, 1, 4, 11].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[4, 1, 4, 11].Style.Fill.BackgroundColor.SetColor(Color.Gray);
                #endregion
                #region Table Content
                for (int x = 0; x < pagibigList.Count(); x++)
                {
                    worksheet.Cells[x + 5, 1].Value = pagibigList[x].PagibigID;
                    worksheet.Cells[x + 5, 2].Value = pagibigList[x].PagibigAccNo;
                    worksheet.Cells[x + 5, 3].Value = pagibigList[x].MembershipPro;
                    worksheet.Cells[x + 5, 4].Value = pagibigList[x].Lname.ToUpper();
                    worksheet.Cells[x + 5, 5].Value = pagibigList[x].Fname.ToUpper();
                    worksheet.Cells[x + 5, 6].Value = pagibigList[x].NameExtension;
                    worksheet.Cells[x + 5, 7].Value = pagibigList[x].Mname.ToUpper();
                    worksheet.Cells[x + 5, 8].Value = pagibigList[x].PerCov;
                    worksheet.Cells[x + 5, 9].Value = pagibigList[x].EEShare.FixDigit();
                    worksheet.Cells[x + 5, 10].Value = pagibigList[x].ERShare.FixDigit();
                    worksheet.Cells[x + 5, 11].Value = pagibigList[x].Remarks;
                }
                #endregion
                #region Total Row
                var lastRow = pagibigList.Count() + 5;
                worksheet.Cells[lastRow, 1, lastRow, 11].Style.Font.Bold = true;
                worksheet.Cells[lastRow, 1, lastRow, 11].Style.Font.Italic = true;
                worksheet.Cells[lastRow, 1, lastRow, 11].Style.Font.Size = 14;
                worksheet.Cells[lastRow, 4].Value = "TOTAL";
                worksheet.Cells[lastRow, 7].Value = "TOTAL";
                worksheet.Cells[lastRow, 7].Style.Font.Color.SetColor(Color.Red);
                worksheet.Cells[lastRow, 9].Value = pagibigList.Sum(x => x.EEShare).FormalDigit();
                worksheet.Cells[lastRow, 9].Style.Font.Color.SetColor(Color.Red);
                #endregion
                worksheet.Cells[4, 1, lastRow, 11].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[4, 1, lastRow, 11].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[4, 1, lastRow, 11].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[4, 1, lastRow, 11].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                worksheet.Cells.AutoFitColumns();

                package.Save();
            }

            return stream;
        }

        public byte[] SetPDF(string regularPayrolls, string document_title, Orientation test)
        {
            //new CustomAssemblyLoadContext().LoadUnmanagedLibrary(Path.Combine(Directory.GetCurrentDirectory(), "libwkhtmltox.dll"));
            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = test,
                PaperSize = PaperKind.Legal,
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
