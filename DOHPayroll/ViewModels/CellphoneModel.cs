using DOHPayroll.Resources;
using DOHPayroll.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DOHPayroll.ViewModels
{
    public partial class CellphoneModel : Employee
    {
        public int CellphoneId { get; set; }
        [Required]
        public string Date { get; set; }
        [Required]
        public double Amount { get; set; }
        [Required]
        public double MonthBillAmount { get; set; }
        public string Remarks { get; set; }
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
        public string MonthBill { get; set; }

        public double NetAmount
        {
            get
            {
                return Math.Round(Amount - MonthBillAmount, 2);
            }
        }
    }
}
