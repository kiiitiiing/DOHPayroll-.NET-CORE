using DOHPayroll.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DOHPayroll.ViewModels
{
    public partial class RegularPayrollModel : Employee
    {
        public int PayrollId { get; set; }
        public double RATA { get; set; }
        public double Cellphone { get; set; }
        public double TotalOthers
        {
            get
            {
                return CFI.Item + SIMC.Item + HWMPC.Item + Disallowances.Item + DBP.Item;
            }
        }
        public double TotalPagibig
        {
            get
            {
                return PagibigPremium + PagibigLoan.Item + PagibigMP2.Item + PagibigCalamity.Item;
            }
        }
        public double TotalGsis
        {
            get
            {
                return GSIS_COMP.Item + GSIS_GFAL.Item + GSIS_Premium + GSIS_Consoloan.Item + GSIS_Help.Item + GSIS_PolicyLoan.Item + GSIS_EML.Item + GSIS_Rel.Item + GSIS_EDU.Item;
            }
        }
        public double GrossIncome
        {
            get
            {
                return (double.Parse(Salary) + PERA) - Deduction;
            }
        }
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
        [Required]
        public double PERA { get; set; }
        [Required]
        public int WorkingDays { get; set; }
        [Required]
        public int MinutesLate { get; set; }
        public string AbsentDays { get; set; }
        public double DeductionPerDay
        {
            get
            {
                return ((Salary.CheckSalaryIfEmpty() / WorkingDays) / 8) / 60;
            }
        }
        public double DeductionLate
        {
            get
            {
                return Math.Round(MinutesLate * DeductionPerDay, 2);
            }
        }
        public double DeductionAbsent
        {
            get
            {
                var noDaysAbsent = !string.IsNullOrEmpty(AbsentDays) ? AbsentDays.Split(",").Where(x => !string.IsNullOrEmpty(x)).ToList().Count() : 0;
                return Math.Round((480 * noDaysAbsent) * DeductionPerDay, 2);
            }
        }
        public double Deduction
        {
            get
            {
                return DeductionLate + DeductionAbsent;   
            }
        }
        public double NetAmount
        {
            get
            {
                var netAmt = Math.Round(Salary.CheckSalaryIfEmpty() + PERA - Deduction, 2);
                return netAmt < 0 ? 0 : netAmt;
            }
        }
        public LoanModel GSIS_GFAL { get; set; }
        public LoanModel GSIS_COMP { get; set; }
        [Required]
        public double ProfessionalTax { get; set; }
        public LoanModel CFI { get; set; }
        [Required]
        public double GSIS_Premium
        {
            get
            {
                return Math.Round(Salary.CheckSalaryIfEmpty() * 0.09, 2);
            }
        }
        public LoanModel GSIS_Consoloan { get; set; }
        public LoanModel GSIS_PolicyLoan { get; set; }
        public LoanModel GSIS_EML { get; set; }
        public LoanModel GSIS_UOLI { get; set; }
        public LoanModel GSIS_EDU { get; set; }
        public LoanModel GSIS_Help { get; set; }
        public LoanModel GSIS_Rel { get; set; }
        [Required]
        public double PagibigPremium { get; set; }
        public LoanModel PagibigLoan { get; set; }
        public LoanModel PagibigMP2 { get; set; }
        public LoanModel PagibigCalamity { get; set; }
        [Required]
        public double Phic
        {
            get
            {
                return Salary.CheckSalaryIfEmpty() >= 60000 ? 900 : Salary.CheckSalaryIfEmpty() * 0.015;
            }
        }
        public LoanModel SIMC { get; set; }
        public LoanModel HWMPC { get; set; }
        public LoanModel DBP { get; set; }
        public LoanModel Disallowances { get; set; }
        public double TotalDeductions
        {
            get
            {
                return Deduction + TotalGsis + TotalPagibig + TotalOthers + ProfessionalTax + Phic;
            }
        }
        public double TotalSalary
        {
            get
            {
                return Salary.CheckSalaryIfEmpty() + PERA;
            }
        }
        public double NetPay
        {
            get
            {
                double netPay = TotalSalary - TotalDeductions;
                if (netPay < 0)
                    return 0;
                else
                    return netPay;
            }
        }
    }
}
