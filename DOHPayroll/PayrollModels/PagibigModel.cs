using DOHPayroll.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DOHPayroll.PayrollModels
{
    public partial class PagibigModel : Employee
    {
        public string PagibigID { get; set; }
        public string PagibigAccNo { get; set; }
        public string MembershipPro { get; set; }
        public string PerCov { get; set; }
        public double EEShare { get; set; }
        public double ERShare { get; set; }
        public string Remarks { get; set; }
    }
}
