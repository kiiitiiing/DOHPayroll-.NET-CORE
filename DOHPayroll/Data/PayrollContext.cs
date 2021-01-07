using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using DOHPayroll.PayrollModels;

namespace DOHPayroll.Data
{
    public partial class PayrollContext : DbContext
    {
        public PayrollContext()
        {
        }

        public PayrollContext(DbContextOptions<PayrollContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Payroll> Payroll { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseMySQL("server=localhost;port=3306;user=root;password=admin;database=payroll");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Payroll>(entity =>
            {
                entity.HasIndex(e => e.EndDate)
                    .HasName("end_date");

                entity.HasIndex(e => e.StartDate)
                    .HasName("start_date");

                entity.HasIndex(e => e.Userid)
                    .HasName("userid");

                entity.Property(e => e.OtherAdjustment).HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Tax).HasDefaultValueSql("'NULL'");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
