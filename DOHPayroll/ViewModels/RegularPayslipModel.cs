using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DOHPayroll.ViewModels
{
    public partial class RegularPayslipModel
    {
        public string FullName
        {
            get
            {
                return FirstHalf.RegularPayroll.Fullname;
            }
        }
        public string UserID
        {
            get
            {
                return FirstHalf.RegularPayroll.ID;
            }
        }
        public string Salary
        {
            get
            {
                return FirstHalf.RegularPayroll.Salary;
            }
        }
        public string Designation
        {
            get
            {
                return FirstHalf.RegularPayroll.Designation;
            }
        }
        public FirstHalf FirstHalf { get; set; }
        public SecondHalf SecondHalf { get; set; }
        public double NetPayment
        {
            get
            {
                return FirstHalf.TotalPay + SecondHalf.TotalPay;
            }
        }
    }

    public partial class FirstHalf
    {
        public RegularPayrollModel RegularPayroll { get; set; }
        public SubsistenceModel Subsistence { get; set; }
        public LongevityModel Longevity { get; set; }
        public double NetSalary
        {
            get
            {
                return RegularPayroll.NetPay / 2;
            }
        }
        public double TotalPay
        {
            get
            {
                return NetSalary + Longevity.NetPay + Subsistence.NetAmount; 
            }
        }
    }

    public partial class SecondHalf
    {
        public HazardModel Hazard { get; set; }
        public CellphoneModel CommAll { get; set; }
        public RATAModel Rata { get; set; }
        public double MidYearBonus
        {
            get
            {
                if (Hazard.Month == 5)
                    return Hazard.Salary.CheckSalaryIfEmpty();
                else
                    return 0;
            }
        }
        public double YearEndBonus
        {
            get
            {
                if (Hazard.Month == 11)
                    return Hazard.Salary.CheckSalaryIfEmpty();
                else
                    return 0;
            }
        }
        public double TotalPay
        {
            get
            {
                return (Hazard.Salary.CheckSalaryIfEmpty() + Hazard.NetAmount + CommAll.NetAmount + MidYearBonus + YearEndBonus + Rata.Total);
            }
        }
    }
}
