using DOHPayroll.Resources;
using Microsoft.AspNetCore.Routing.Constraints;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DOHPayroll.ViewModels
{
    public partial class SubsistenceModel : Employee
    {
        public double DeductionPerDaySub = 50.0;
        public double DeductionPerDayLau = 6.82;
        public int SubsistenceId { get; set; }
        [Required]
        public string Date { get; set; }
        [Required]
        public double Subsistence { get; set; }
        [Required]
        public double Laundry { get; set; }
        [Required]
        public double NoDaysAbsent { get; set; }
        [Required]
        public double HWMPC { get; set; }
        public double NetAmount
        {
            get
            {
                return (Subsistence + Laundry) - (HWMPC + LessTotal);
            }
        }
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

        public double LessSubsistenceAllowance
        {
            get
            {
                return NoDaysAbsent * DeductionPerDaySub;
            }
        }

        public double LessLaundryAllowance
        {
            get
            {
                return NoDaysAbsent * DeductionPerDayLau;
            }
        }

        public double LessTotal
        {
            get
            {
                return LessSubsistenceAllowance + LessLaundryAllowance;
            }
        }
    }
}
