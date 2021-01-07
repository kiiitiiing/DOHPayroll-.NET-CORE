using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DOHPayroll.DTRModels
{
    [Table("users")]
    public partial class Users
    {
        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public int Id { get; set; }
        [Column("email")]
        [StringLength(255)]
        public string Email { get; set; }
        [Column("userid")]
        [StringLength(255)]
        public string Userid { get; set; }
        [Column("fname")]
        [StringLength(255)]
        public string Fname { get; set; }
        [Column("lname")]
        [StringLength(255)]
        public string Lname { get; set; }
        [Column("mname")]
        [StringLength(255)]
        public string Mname { get; set; }
        [Column("sched")]
        [StringLength(10)]
        public string Sched { get; set; }
        [Required]
        [Column("username")]
        [StringLength(255)]
        public string Username { get; set; }
        [Column("password")]
        [StringLength(255)]
        public string Password { get; set; }
        [Column("emptype")]
        [StringLength(255)]
        public string Emptype { get; set; }
        [Column("usertype")]
        public bool Usertype { get; set; }
        [Required]
        [Column("unique_row")]
        [StringLength(255)]
        public string UniqueRow { get; set; }
        [Column("remember_token")]
        [StringLength(100)]
        public string RememberToken { get; set; }
        [Column("pass_change")]
        [StringLength(45)]
        public string PassChange { get; set; }
        [Column("imei")]
        [StringLength(100)]
        public string Imei { get; set; }
        [Column("authority")]
        [StringLength(100)]
        public string Authority { get; set; }
        [Column("region")]
        [StringLength(100)]
        public string Region { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
