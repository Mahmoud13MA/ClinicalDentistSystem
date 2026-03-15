using clinical.APIs.Modules.DentalClinic.Models;
using Microsoft.EntityFrameworkCore;

namespace clinical.APIs.Shared.Data
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
        public DbSet<EHRChangeLog> EHRChangeLogs { get; set; }
        public DbSet<MedicationRecord> MedicationRecords { get; set; }
        public DbSet<ProcedureRecord> ProcedureRecords { get; set; }
        public DbSet<ToothRecord> ToothRecords { get; set; }
        public DbSet<XRayRecord> XRayRecords { get; set; }
        public DbSet<Supply> Supplies { get; set; }
        public DbSet<Stock_Transaction> StockTransactions { get; set; }

        public DbSet<Admin> Admins { get; set; }

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

            // EHR Change Log relationships
            modelBuilder.Entity<EHRChangeLog>()
                .HasOne(cl => cl.EHR)
                .WithMany(e => e.ChangeLogs)
                .HasForeignKey(cl => cl.EHR_ID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EHRChangeLog>()
                .HasOne(cl => cl.Doctor)
                .WithMany()
                .HasForeignKey(cl => cl.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EHRChangeLog>()
                .HasOne(cl => cl.Appointment)
                .WithMany()
                .HasForeignKey(cl => cl.Appointment_ID)
                .OnDelete(DeleteBehavior.Restrict);

            // MedicationRecord relationships
            modelBuilder.Entity<MedicationRecord>()
                .HasOne(m => m.EHR)
                .WithMany(e => e.Medications)
                .HasForeignKey(m => m.EHR_ID)
                .OnDelete(DeleteBehavior.Cascade);

            // ProcedureRecord relationships
            modelBuilder.Entity<ProcedureRecord>()
                .HasOne(p => p.EHR)
                .WithMany(e => e.Procedures)
                .HasForeignKey(p => p.EHR_ID)
                .OnDelete(DeleteBehavior.Cascade);

            // ToothRecord relationships
            modelBuilder.Entity<ToothRecord>()
                .HasOne(t => t.EHR)
                .WithMany(e => e.Teeth)
                .HasForeignKey(t => t.EHR_ID)
                .OnDelete(DeleteBehavior.Cascade);

            // XRayRecord relationships
            modelBuilder.Entity<XRayRecord>()
                .HasOne(x => x.EHR)
                .WithMany(e => e.XRays)
                .HasForeignKey(x => x.EHR_ID)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
    }

