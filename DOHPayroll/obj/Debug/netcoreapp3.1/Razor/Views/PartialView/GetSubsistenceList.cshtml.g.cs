#pragma checksum "C:\Users\Keith\source\repos\DOHPayroll\DOHPayroll\Views\PartialView\GetSubsistenceList.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "2b1625f7536aa65074124bcb7ff8ce7fc07b73e2"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_PartialView_GetSubsistenceList), @"mvc.1.0.view", @"/Views/PartialView/GetSubsistenceList.cshtml")]
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
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"2b1625f7536aa65074124bcb7ff8ce7fc07b73e2", @"/Views/PartialView/GetSubsistenceList.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"abf18aefd1c1ea6931b87dd65b78091d5f0ebf94", @"/Views/_ViewImports.cshtml")]
    public class Views_PartialView_GetSubsistenceList : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<PaginatedList<SubsistenceModel>>
    {
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
            WriteLiteral("\r\n<div class=\"row\">\r\n");
#nullable restore
#line 4 "C:\Users\Keith\source\repos\DOHPayroll\DOHPayroll\Views\PartialView\GetSubsistenceList.cshtml"
     if (Model.Count() > 0)
    {
        

#line default
#line hidden
#nullable disable
#nullable restore
#line 6 "C:\Users\Keith\source\repos\DOHPayroll\DOHPayroll\Views\PartialView\GetSubsistenceList.cshtml"
         foreach (var subistence in Model)
        {

#line default
#line hidden
#nullable disable
            WriteLiteral("            <div class=\"col-md-4\">\r\n                <div class=\"small-box bg-gradient-white\" style=\"padding-bottom: 5px !important;\">\r\n                    <div class=\"inner\">\r\n                        <h5>");
#nullable restore
#line 11 "C:\Users\Keith\source\repos\DOHPayroll\DOHPayroll\Views\PartialView\GetSubsistenceList.cshtml"
                       Write(subistence.ID);

#line default
#line hidden
#nullable disable
            WriteLiteral("</h5>\r\n\r\n                        <p>");
#nullable restore
#line 13 "C:\Users\Keith\source\repos\DOHPayroll\DOHPayroll\Views\PartialView\GetSubsistenceList.cshtml"
                      Write(subistence.Date);

#line default
#line hidden
#nullable disable
            WriteLiteral(@"</p>
                    </div>
                    <div class=""icon"">
                        <i class=""fas fa-file-alt""></i>
                    </div>
                    <div class=""form-inline"" style=""margin-left: 5px !important;"">
                        <button class=""btn btn-sm btn-danger btn-margin-right""");
            BeginWriteAttribute("value", "\r\n                                value=\"", 743, "\"", 809, 1);
#nullable restore
#line 20 "C:\Users\Keith\source\repos\DOHPayroll\DOHPayroll\Views\PartialView\GetSubsistenceList.cshtml"
WriteAttributeValue("", 784, subistence.SubsistenceId, 784, 25, false);

#line default
#line hidden
#nullable disable
            EndWriteAttribute();
            WriteLiteral("\r\n                                data-userid=\"");
#nullable restore
#line 21 "C:\Users\Keith\source\repos\DOHPayroll\DOHPayroll\Views\PartialView\GetSubsistenceList.cshtml"
                                        Write(subistence.ID);

#line default
#line hidden
#nullable disable
            WriteLiteral(@"""
                                onclick=""DeleteDbRow($(this).val(), 'subsistence', $(this).data('userid'));"">
                            <i class=""fas fa-trash-alt""></i>
                        </button>
                    </div>
                </div>
            </div>
");
#nullable restore
#line 28 "C:\Users\Keith\source\repos\DOHPayroll\DOHPayroll\Views\PartialView\GetSubsistenceList.cshtml"
        }

#line default
#line hidden
#nullable disable
#nullable restore
#line 28 "C:\Users\Keith\source\repos\DOHPayroll\DOHPayroll\Views\PartialView\GetSubsistenceList.cshtml"
         
    }
    else
    {

#line default
#line hidden
#nullable disable
            WriteLiteral("        <div class=\"alert alert-warning col-md-12\">\r\n            <span class=\"text-lg\">\r\n                <i class=\"fa fa-exclamation-triangle\"></i>\r\n                &nbsp;No data found!\r\n            </span>\r\n        </div>\r\n");
#nullable restore
#line 38 "C:\Users\Keith\source\repos\DOHPayroll\DOHPayroll\Views\PartialView\GetSubsistenceList.cshtml"
    }

#line default
#line hidden
#nullable disable
            WriteLiteral("</div>\r\n\r\n<div class=\"page-list-footer\">\r\n    ");
#nullable restore
#line 42 "C:\Users\Keith\source\repos\DOHPayroll\DOHPayroll\Views\PartialView\GetSubsistenceList.cshtml"
Write(await Html.PartialAsync("~/Views/Shared/_PageListsPartial.cshtml", new PageListModel
{
    Action = Model._action,
    HasNextPage = Model.HasNextPage,
    HasPreviousPage = Model.HasPreviousPage,
    PageIndex = Model._pageIndex,
    TotalPages = Model._totalPages,
    Container = "regular-subsistence-list",
    Parameters = new Dictionary<string, string>
    {
        { "userid", ViewBag.UserID},
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
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<PaginatedList<SubsistenceModel>> Html { get; private set; }
    }
}
#pragma warning restore 1591
