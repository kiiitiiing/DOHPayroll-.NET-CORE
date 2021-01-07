using DOHPayroll.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DOHPayroll.ViewModels
{
    public partial class JoPayrollModel : Employee
    {
        public int PayrollId { get; set; }
        [Required]
        public string Date { get; set; }
        [Required]
        public int Range { get; set; }
        public DateTime StartDate
        {
            get
            {
                if (string.IsNullOrEmpty(Date))
                    return DateTime.Now;
                else
                {
                    string monthname = Date.Split(" ")[0];
                    int month = Months.All.First(x => x.Key == monthname).Value;
                    int year = int.Parse(Date.Split(" ")[1]);
                    if (Range == 1)
                        return new DateTime(year, month, 1);
                    else
                        return new DateTime(year, month, 16);
                }
            }
        }
        public DateTime EndDate
        {
            get
            {
                if (string.IsNullOrEmpty(Date))
                    return DateTime.Now;
                else
                {
                    int month = Months.All.First(x => x.Key == Date.Split(" ")[0]).Value;
                    int year = int.Parse(Date.Split(" ")[1]);
                    if (Range == 1)
                        return new DateTime(year, month, 15);
                    else
                        return new DateTime(year, month, DateTime.DaysInMonth(year,month));
                }
            }
        }
        public double Disallowance { get; set; }
        public double Adjustment { get; set; }
        public double OtherAdjustments { get; set; }
        public int WorkingDays { get; set; }
        public int MinutesLate { get; set; }
        public string AbsentDays { get; set; }
        public double Deduction
        {
            get
            {
                var noDaysAbsent = AbsentDays.Split(",").Where(x => !string.IsNullOrEmpty(x)).ToList();
                return Math.Round((MinutesLate + (480 * noDaysAbsent.Count())) * (((Salary.CheckSalaryIfEmpty() / WorkingDays) / 8) / 60), 2);
            }
        }
        public double NetAmount
        {
            get
            {
                var halfSalary = Salary.CheckSalaryIfEmpty() == 0 ? 0 : Salary.CheckSalaryIfEmpty() / 2;
                var netAmt = Math.Round((halfSalary + Adjustment) - (Deduction), 2);
                return netAmt < 0 ? 0 : netAmt;
            }
        }
        public double EWT { get; set; }
        public double ProfessionalTax { get; set; }
        public LoanModel HWMPC { get; set; }
        public LoanModel Pagibig { get; set; }
        public LoanModel GSIS { get; set; }
        public LoanModel PHIC { get; set; }
        public LoanModel PagibigLoan { get; set; }
        public double TotalDeductions
        {
            get
            {
                return Deduction + HWMPC.Item + Pagibig.Item + GSIS.Item + PHIC.Item + PagibigLoan.Item + EWT + ProfessionalTax;
            }
        }
        public double NetPay
        {
            get
            {
                var halfSalary = Salary.CheckSalaryIfEmpty() == 0 ? 0 : Salary.CheckSalaryIfEmpty() / 2;
                var netPay = (halfSalary + Adjustment) - TotalDeductions;
                return netPay < 0 ? 0 : netPay;
            }
        }
        public string Remarks { get; set; }
    }
}
