using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DOHPayroll.ViewModels
{
    public partial class PageListModel
    {
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
        public int TotalPages { get; set; }
        public string Action { get; set; }
        public int PageIndex { get; set; }
        public string Container { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
    }
}
