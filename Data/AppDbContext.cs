using clinical.APIs.Models;
using Microsoft.EntityFrameworkCore;

namespace clinical.APIs.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        { }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Nurse> Nurses { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<EHR> EHRs { get; set; }
        public DbSet<Supply> Supplies { get; set; }
        public DbSet<Stock_Transaction> StockTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.EHR)
                .WithOne(e => e.Appointment)
                .HasForeignKey<EHR>(e => e.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.Patient_ID)
                .OnDelete(DeleteBehavior.Restrict);

         
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.Doctor_ID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Nurse)
                .WithMany(n => n.Appointments)
                .HasForeignKey(a => a.Nurse_ID)
                .OnDelete(DeleteBehavior.Restrict);

          
            modelBuilder.Entity<Stock_Transaction>()
                .HasOne(st => st.Supply)
                .WithMany(s => s.StockTransactions)
                .HasForeignKey(st => st.Supply_ID)
                .OnDelete(DeleteBehavior.Cascade);

          
            modelBuilder.Entity<Stock_Transaction>()
                .HasOne(st => st.Doctor)
                .WithMany()
                .HasForeignKey(st => st.Doctor_ID)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
    }

