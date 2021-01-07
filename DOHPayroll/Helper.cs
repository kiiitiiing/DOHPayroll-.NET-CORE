using DOHPayroll.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace DOHPayroll
{
    public static class Helper
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string GetCurrentMethod()
        {
            var st = new StackTrace();
            var sf = st.GetFrame(0);

            return sf.GetMethod().Name;
        }

        public static string WhtSpc(this string digit)
        {
            if (digit == "0.00")
                return "<span class='font-white'>1,00</span>0.00";
            else
                return digit;
        }

        public static double CheckSalaryIfEmpty(this string salary)
        {
            if (string.IsNullOrEmpty(salary))
                return 0;
            else
                return double.Parse(salary);    
        }
        public static string RemoveLastChar(this string text)
        {
            return string.IsNullOrEmpty(text) ? "" : text.Remove(text.Length - 1, 1);
        }

        public static double MinsFromAbsences(this string absences)
        {
            var days = string.IsNullOrEmpty(absences) ? 0 : absences.Split(",").Length;

            return days * 480;
        }

        public static string FormalDigit(this double number)
        {
            if(number == 0.00)
                return "<span class='font-white'>1,00</span>0.00";
            else 
                return string.Format("{0:n}", number);
        }

        public static string ConvertStr(this object reader)
        {
            return reader.ToString();
        }
        public static int ConvertInt(this object reader)
        {
            var val = reader.ToString();
            int retVal = 0;
            if(int.TryParse(val, out retVal))
            {
                return retVal;
            }

            return 0;
        }

        public static double ConvertDouble(this object reader)
        {
            var val = reader.ToString();
            double retVal = 0.00;
            if (double.TryParse(val, out retVal))
            {
                return retVal;
            }

            return 0;
        }

        public static string Enclose(this object text)
        {
            return "(" + text.ToString() + ")";
        }

        public static string FixDigit(this double number)
        {
            return string.Format("{0:n}", number);
        }

        public static double ReturnIfZero(this double number)
        {
            if (number < 0)
                return 0;
            else
                return number;
        }

        public static string FormalDigit(this string number)
        {
            return string.Format("{0:n}", double.Parse(number));
        }

        public static List<DropDownModel> AsDropDown(this List<Employee> employees)
        {
            var dropdown = new List<DropDownModel>();
            foreach(var employee in employees)
            {
                dropdown.Add(new DropDownModel
                {
                    id = employee.ID,
                    text = employee.Fullname
                });
            }
            return dropdown;
        }

        public static string Initial(this string text)
        {
            if (!string.IsNullOrEmpty(text))
                return text[0].ToString();
            else
                return "";
        }

        public static string Tranch(this string salary_grade)
        {
            if(salary_grade.Contains("|"))
            {
                var tranch = salary_grade.Split('|')[0].Trim();
                switch (tranch)
                {
                    case "First":
                        {
                            return 1 + "";
                        }
                    case "Second":
                        {
                            return 2 + "";
                        }

                    case "Third":
                        {
                            return 3 + "";
                        }
                    case "Fourth":
                        {
                            return 4 + "";
                        }
                    default:
                        {
                            return tranch;
                        }
                }
            }
            else
            {
                return salary_grade;
            }
        }

        public static string Grade(this string salary_grade)
        {
            if(string.IsNullOrEmpty(salary_grade) && salary_grade.Contains("|"))
            {
                var split = salary_grade.Split('|');
                var grade_step = split[1].Trim();
                return grade_step.Split('-')[0].Trim();
            }
            else
            {
                return salary_grade;
            }
        }

        public static string SalCharge(this string salary_charge)
        {
            switch(salary_charge)
            {
                case "MSD":
                    {
                        return "OFFICE OF THE MANAGEMENT SUPPORT DIVISION";
                    }
                case "RD_ARD":
                    {
                        return "OFFICE OF THE DIRECTOR/ASSISTANT DIRECTOR";
                    }
                case "RLED":
                    {
                        return "REGULATIONS, LICENSING AND ENFORCEMENT DIVISION";
                    }
                case "LHSD":
                    {
                        return "LOCAL HEALTH SUPPORT DIVISION";
                    }
                default:
                    {
                        return salary_charge;
                    }
            }
        }

        public static string Step(this string salary_grade)
        {
            if (string.IsNullOrEmpty(salary_grade) && salary_grade.Contains("|"))
            {
                var split = salary_grade.Split('|');
                var grade_step = split[1].Trim();
                return grade_step.Split('-')[1].Trim();
            }
            else
            {
                return salary_grade;
            }
        }
    }
}
