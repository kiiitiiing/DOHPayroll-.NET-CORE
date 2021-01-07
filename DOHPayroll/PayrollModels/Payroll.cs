using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DOHPayroll.PayrollModels
{
    [Table("payroll")]
    public partial class Payroll
    {
        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public int Id { get; set; }
        [Required]
        [Column("userid")]
        [StringLength(25)]
        public string Userid { get; set; }
        [Column("start_date", TypeName = "date")]
        public DateTime StartDate { get; set; }
        [Column("end_date", TypeName = "date")]
        public DateTime EndDate { get; set; }
        [Required]
        [Column("absent_days")]
        [StringLength(255)]
        public string AbsentDays { get; set; }
        [Column("working_days", TypeName = "int(11)")]
        public int WorkingDays { get; set; }
        [Column("month_salary", TypeName = "decimal(10,2)")]
        public decimal MonthSalary { get; set; }
        [Column("adjustment", TypeName = "decimal(10,2)")]
        public decimal Adjustment { get; set; }
        [Column("minutes_late", TypeName = "int(11)")]
        public int MinutesLate { get; set; }
        [Column("coop", TypeName = "decimal(10,2)")]
        public decimal Coop { get; set; }
        [Column("phic", TypeName = "decimal(10,2)")]
        public decimal Phic { get; set; }
        [Column("disallowance", TypeName = "decimal(10,2)")]
        public decimal Disallowance { get; set; }
        [Column("gsis", TypeName = "decimal(10,2)")]
        public decimal Gsis { get; set; }
        [Column("pagibig", TypeName = "decimal(10,2)")]
        public decimal Pagibig { get; set; }
        [Column("excess_mobile", TypeName = "decimal(10,2)")]
        public decimal ExcessMobile { get; set; }
        [Required]
        [Column("remarks")]
        [StringLength(255)]
        public string Remarks { get; set; }
        [Column("tax", TypeName = "decimal(10,2)")]
        public decimal? Tax { get; set; }
        [Column("other_adjustment", TypeName = "decimal(10,2)")]
        public decimal? OtherAdjustment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
