using DOHPayroll.Resources;
using Microsoft.AspNetCore.Routing.Constraints;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DOHPayroll.ViewModels
{
    public partial class RATAModel : Employee
    {
        public int RATAID { get; set; }
        [Required]
        public string Date { get; set; }
        public int Month
        {
            get
            {
                if (string.IsNullOrEmpty(Date))
                    return 0;
                else
                {
                    return Months.All.First(x => x.Key == Date.Split(" ")[0]).Value;
                }
            }
        }
        public int Year
        {
            get
            {
                if (string.IsNullOrEmpty(Date))
                    return 0;
                else
                {
                    return int.Parse(Date.Split(" ")[1]);
                }
            }
        }
        [Required]
        public double RA { get; set; }
        [Required]
        public double TA { get; set; }
        [Required]
        public double Deductions { get; set; }
        public double Total
        {
            get
            {
                return Math.Round((RA + TA) - Deductions, 2);
            }
        }
        public double Rata
        {
            get
            {
                return RA + TA;
            }
        }
        public string Remarks { get; set; }
    }
}
