using DOHPayroll.Resources;
using Microsoft.AspNetCore.Routing.Constraints;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DOHPayroll.ViewModels
{
    public partial class HazardModel : Employee
    {
        public int HazardID { get; set; }
        [Required]
        public double HazardPay { get; set; }
        [Required]
        public double HWMPCLoan { get; set; }
        [Required]
        public double GlobeBill { get; set; }
        [Required]
        public double SunBill { get; set; }
        [Required]
        public double Mortuary { get; set; }
        public double NetAmount
        {
            get
            {
                return Math.Round(HazardPay - (HWMPCLoan + Mortuary + GlobeBill + SunBill), 2);
            }
        }
        public double NetAmountNoMortuary
        {
            get
            {
                return Math.Round(HazardPay - (HWMPCLoan + GlobeBill + SunBill), 2);
            }
        }
        [Required]
        public int DaysWithOO { get; set; }
        [Required]
        public int DaysWithLeave { get; set; }
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
        public string Date { get; set; }
        public double DigitelBilling
        {
            get
            {
                return SunBill + GlobeBill;
            }
        }
        public string HazardSalaryPercentage
        {
            get
            {
                if (!string.IsNullOrEmpty(Salary))
                    return Math.Round((HazardPay / double.Parse(Salary)) * 100, 2) + "%";
                else
                    return "";
            }
        }

    }
}
