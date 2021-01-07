using DOHPayroll.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DOHPayroll.ViewModels
{
    public partial class LongevityModel : Employee
    {
        public int LongevityId { get; set; }
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
        public string EntranceToDuty { get; set; }
        public List<SalaryModel> Salaries { get; set; }
        public double Disallowance { get; set; }
        public double TotalLPMO
        {
            get
            {
                return Salaries.Sum(x => x.LP);
            }
        }
        public double NetPay
        {
            get
            {
                return (TotalLPMO - Disallowance).ReturnIfZero();
            }
        }
    }
}
