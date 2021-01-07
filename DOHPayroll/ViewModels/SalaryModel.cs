using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DOHPayroll.ViewModels
{
    public partial class SalaryModel
    {
        public int Index { get; set; }
        public double Salary { get; set; }
        public double LP
        {
            get
            {
                return Math.Round(Salary * 0.05, 2);
            }
        }
    }
}
