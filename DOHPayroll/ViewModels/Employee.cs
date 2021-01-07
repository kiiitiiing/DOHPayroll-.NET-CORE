using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DOHPayroll.ViewModels
{
    public partial class Employee
    {
        public string ID { get; set; }
        public string Fname { get; set; }
        public string Mname { get; set; }
        public string Lname { get; set; }
        public string NameExtension { get; set; }
        public string Designation { get; set; }
        public string Salary { get; set; }
        public string SalaryCharge { get; set; }
        public string SalaryGrade { get; set; }
        public string Division { get; set; }
        public string Section { get; set; }
        public string DisbursementType { get; set; }
        public string JobStatus { get; set; }
        public string Tin { get; set; }
        public bool CPRATA { get; set; }
        public string Fullname
        {
            get
            {
                return Fname + " " + (string.IsNullOrEmpty(Mname) ? "" : Mname + " ") + Lname;
            }
        }
        public string Tranch
        {
            get
            {
                if (!string.IsNullOrEmpty(SalaryGrade))
                    return SalaryGrade.Tranch();
                else
                    return "";
            }
        }
        public string Grade
        {
            get
            {
                if (!string.IsNullOrEmpty(SalaryGrade))
                    return SalaryGrade.Grade();
                else
                    return "";
            }
        }
        public string Step
        {
            get
            {
                if (!string.IsNullOrEmpty(SalaryGrade))
                    return SalaryGrade.Step();
                else
                    return "";
            }
        }
    }
}
