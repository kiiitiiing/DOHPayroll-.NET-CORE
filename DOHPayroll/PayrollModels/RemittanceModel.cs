using DOHPayroll.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DOHPayroll.PayrollModels
{
    public partial class RemittanceModel : Employee
    {
        public string PhicNo { get; set; }
        public double Amount { get; set; }
    }
}
