#pragma checksum "C:\Users\Keith\source\repos\DOHPayroll\DOHPayroll\Views\PartialView\JobOrderList.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "a2fbb8da9311217b3063ef51da45da81111cb80c"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_PartialView_JobOrderList), @"mvc.1.0.view", @"/Views/PartialView/JobOrderList.cshtml")]
namespace AspNetCore
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
#nullable restore
#line 1 "C:\Users\Keith\source\repos\DOHPayroll\DOHPayroll\Views\_ViewImports.cshtml"
using DOHPayroll;

#line default
#line hidden
#nullable disable
#nullable restore
#line 2 "C:\Users\Keith\source\repos\DOHPayroll\DOHPayroll\Views\_ViewImports.cshtml"
using DOHPayroll.Models;

#line default
#line hidden
#nullable disable
#nullable restore
#line 3 "C:\Users\Keith\source\repos\DOHPayroll\DOHPayroll\Views\_ViewImports.cshtml"
using DOHPayroll.ViewModels;

#line default
#line hidden
#nullable disable
#nullable restore
#line 4 "C:\Users\Keith\source\repos\DOHPayroll\DOHPayroll\Views\_ViewImports.cshtml"
using System.Security.Claims;

#line default
#line hidden
#nullable disable
#nullable restore
#line 5 "C:\Users\Keith\source\repos\DOHPayroll\DOHPayroll\Views\_ViewImports.cshtml"
using DOHPayroll.Resources;

#line default
#line hidden
#nullable disable
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"a2fbb8da9311217b3063ef51da45da81111cb80c", @"/Views/PartialView/JobOrderList.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"abf18aefd1c1ea6931b87dd65b78091d5f0ebf94", @"/Views/_ViewImports.cshtml")]
    public class Views_PartialView_JobOrderList : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<PaginatedList<Employee>>
    {
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
            WriteLiteral("\r\n");
#nullable restore
#line 3 "C:\Users\Keith\source\repos\DOHPayroll\DOHPayroll\Views\PartialView\JobOrderList.cshtml"
 if (Model.Count() > 0)
{

#line default
#line hidden
#nullable disable
            WriteLiteral(@"    <table class=""table table-striped"">
        <thead>
            <tr>
                <th>
                    USER ID
                </th>
                <th>
                    FULL NAME
                </th>
                <th>
                    DESIGNATION
                </th>
                <th>
                    SECTION
                </th>
                <th>
                    SALARY
                </th>
                <th>
                    &nbsp;
                </th>
            </tr>
        </thead>
        <tbody>
");
#nullable restore
#line 29 "C:\Users\Keith\source\repos\DOHPayroll\DOHPayroll\Views\PartialView\JobOrderList.cshtml"
             foreach (var jo in Model)
            {
                var trbg = string.IsNullOrEmpty(jo.Salary) ? "no-salary" : "";

#line default
#line hidden
#nullable disable
            WriteLiteral("                <tr");
            BeginWriteAttribute("class", " class=\"", 794, "\"", 807, 1);
#nullable restore
#line 32 "C:\Users\Keith\source\repos\DOHPayroll\DOHPayroll\Views\PartialView\JobOrderList.cshtml"
WriteAttributeValue("", 802, trbg, 802, 5, false);

#line default
#line hidden
#nullable disable
            EndWriteAttribute();
            BeginWriteAttribute("id", " id=\"", 808, "\"", 819, 1);
#nullable restore
#line 32 "C:\Users\Keith\source\repos\DOHPayroll\DOHPayroll\Views\PartialView\JobOrderList.cshtml"
WriteAttributeValue("", 813, jo.ID, 813, 6, false);

#line default
#line hidden
#nullable disable
            EndWriteAttribute();
            WriteLiteral(">\r\n                    <td>\r\n                        ");
#nullable restore
#line 34 "C:\Users\Keith\source\repos\DOHPayroll\DOHPayroll\Views\PartialView\JobOrderList.cshtml"
                   Write(jo.ID);

#line default
#line hidden
#nullable disable
            WriteLiteral("\r\n                    </td>\r\n                    <td>\r\n                        ");
#nullable restore
#line 37 "C:\Users\Keith\source\repos\DOHPayroll\DOHPayroll\Views\PartialView\JobOrderList.cshtml"
                   Write(jo.Fullname.ToUpper());

#line default
#line hidden
#nullable disable
            WriteLiteral("\r\n                    </td>\r\n                    <td>\r\n                        ");
#nullable restore
#line 40 "C:\Users\Keith\source\repos\DOHPayroll\DOHPayroll\Views\PartialView\JobOrderList.cshtml"
                   Write(jo.Designation);

#line default
#line hidden
#nullable disable
            WriteLiteral("\r\n                    </td>\r\n                    <td>\r\n                        ");
#nullable restore
#line 43 "C:\Users\Keith\source\repos\DOHPayroll\DOHPayroll\Views\PartialView\JobOrderList.cshtml"
                   Write(jo.Section);

#line default
#line hidden
#nullable disable
            WriteLiteral("\r\n                    </td>\r\n                    <td");
            BeginWriteAttribute("id", " id=\"", 1216, "\"", 1234, 2);
            WriteAttributeValue("", 1221, "salary_", 1221, 7, true);
#nullable restore
#line 45 "C:\Users\Keith\source\repos\DOHPayroll\DOHPayroll\Views\PartialView\JobOrderList.cshtml"
WriteAttributeValue("", 1228, jo.ID, 1228, 6, false);

#line default
#line hidden
#nullable disable
            EndWriteAttribute();
            WriteLiteral(">\r\n                        ");
#nullable restore
#line 46 "C:\Users\Keith\source\repos\DOHPayroll\DOHPayroll\Views\PartialView\JobOrderList.cshtml"
                   Write(jo.Salary);

#line default
#line hidden
#nullable disable
            WriteLiteral("\r\n                    </td>\r\n                    <td class=\"hidden\">\r\n                        ");
#nullable restore
#line 49 "C:\Users\Keith\source\repos\DOHPayroll\DOHPayroll\Views\PartialView\JobOrderList.cshtml"
                   Write(jo.Fname);

#line default
#line hidden
#nullable disable
            WriteLiteral("\r\n                    </td>\r\n                    <td class=\"hidden\">\r\n                        ");
#nullable restore
#line 52 "C:\Users\Keith\source\repos\DOHPayroll\DOHPayroll\Views\PartialView\JobOrderList.cshtml"
                   Write(jo.Mname);

#line default
#line hidden
#nullable disable
            WriteLiteral("\r\n                    </td>\r\n                    <td class=\"hidden\">\r\n                        ");
#nullable restore
#line 55 "C:\Users\Keith\source\repos\DOHPayroll\DOHPayroll\Views\PartialView\JobOrderList.cshtml"
                   Write(jo.Lname);

#line default
#line hidden
#nullable disable
            WriteLiteral("\r\n                    </td>\r\n                    <td>\r\n                        <button type=\"button\" class=\"btn btn-sm bg-gradient-success open_payroll\"");
            BeginWriteAttribute("value", " value=\"", 1733, "\"", 1747, 1);
#nullable restore
#line 58 "C:\Users\Keith\source\repos\DOHPayroll\DOHPayroll\Views\PartialView\JobOrderList.cshtml"
WriteAttributeValue("", 1741, jo.ID, 1741, 6, false);

#line default
#line hidden
#nullable disable
            EndWriteAttribute();
            WriteLiteral(" onclick=\"LoadJoPayrollandList($(this).parent().parent());\">\r\n                            <i class=\"fa fa-file\"></i>&nbsp;Payroll\r\n                        </button>\r\n                    </td>\r\n                </tr>\r\n");
#nullable restore
#line 63 "C:\Users\Keith\source\repos\DOHPayroll\DOHPayroll\Views\PartialView\JobOrderList.cshtml"
            }

#line default
#line hidden
#nullable disable
            WriteLiteral("        </tbody>\r\n    </table>\r\n");
#nullable restore
#line 66 "C:\Users\Keith\source\repos\DOHPayroll\DOHPayroll\Views\PartialView\JobOrderList.cshtml"
}
else
{

#line default
#line hidden
#nullable disable
            WriteLiteral("    <div class=\"alert alert-warning\">\r\n        <span class=\"text-lg\">\r\n            <i class=\"fa fa-exclamation-triangle\"></i>\r\n            &nbsp;No data found!\r\n        </span>\r\n    </div>\r\n");
#nullable restore
#line 75 "C:\Users\Keith\source\repos\DOHPayroll\DOHPayroll\Views\PartialView\JobOrderList.cshtml"
}

#line default
#line hidden
#nullable disable
            WriteLiteral("\r\n<div class=\"page-list-footer\">\r\n    ");
#nullable restore
#line 78 "C:\Users\Keith\source\repos\DOHPayroll\DOHPayroll\Views\PartialView\JobOrderList.cshtml"
Write(await Html.PartialAsync("~/Views/Shared/_PageListsPartial.cshtml", new PageListModel
{
    Action = Model._action,
    HasNextPage = Model.HasNextPage,
    HasPreviousPage = Model.HasPreviousPage,
    PageIndex = Model._pageIndex,
    TotalPages = Model._totalPages,
    Container = "job_order_container",
    Parameters = new Dictionary<string, string>
    {
        { "disbursementType", ViewBag.Type },
        { "search", ViewBag.CurrentSearch},
        { "page", Model._pageIndex.ToString() }
    }
}));

#line default
#line hidden
#nullable disable
            WriteLiteral("\r\n</div>");
        }
        #pragma warning restore 1998
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IUrlHelper Url { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IViewComponentHelper Component { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<PaginatedList<Employee>> Html { get; private set; }
    }
}
#pragma warning restore 1591
