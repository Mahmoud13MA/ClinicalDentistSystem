using clinical.APIs.Modules.DentalClinic.Models;
using Microsoft.EntityFrameworkCore;
using Radiology.Models;

namespace clinical.APIs.Shared.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        { }

        public DbSet<clinical.APIs.Modules.DentalClinic.Models.Patient> Patients { get; set; }
        public DbSet<clinical.APIs.Modules.DentalClinic.Models.Doctor> Doctors { get; set; }
        public DbSet<clinical.APIs.Modules.DentalClinic.Models.Nurse> Nurses { get; set; }
        public DbSet<clinical.APIs.Modules.DentalClinic.Models.Appointment> Appointments { get; set; }
        public DbSet<clinical.APIs.Modules.DentalClinic.Models.EHR> EHRs { get; set; }
        public DbSet<clinical.APIs.Modules.DentalClinic.Models.EHRChangeLog> EHRChangeLogs { get; set; }
        public DbSet<clinical.APIs.Modules.DentalClinic.Models.MedicationRecord> MedicationRecords { get; set; }
        public DbSet<clinical.APIs.Modules.DentalClinic.Models.ProcedureRecord> ProcedureRecords { get; set; }
        public DbSet<clinical.APIs.Modules.DentalClinic.Models.ToothRecord> ToothRecords { get; set; }
        public DbSet<clinical.APIs.Modules.DentalClinic.Models.XRayRecord> XRayRecords { get; set; }
        public DbSet<clinical.APIs.Modules.DentalClinic.Models.Supply> Supplies { get; set; }
        public DbSet<clinical.APIs.Modules.DentalClinic.Models.Stock_Transaction> StockTransactions { get; set; }

        // Radiology Module DbSets
        public DbSet<Radiology.Models.Radiologist> Radiologists { get; set; }
        public DbSet<Radiology.Models.Patient> RadiologyPatients { get; set; }
        public DbSet<Radiology.Models.ImagingAppointment> ImagingAppointments { get; set; }
        public DbSet<Radiology.Models.Equipment> Equipment { get; set; }
        public DbSet<Radiology.Models.Report> Reports { get; set; }

        public DbSet<clinical.APIs.Shared.Models.ProcessedRequest> ProcessedRequests { get; set; }

        public DbSet<Admin> Admins { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<clinical.APIs.Modules.DentalClinic.Models.Appointment>()
                .HasOne(a => a.EHR)
                .WithOne(e => e.Appointment)
                .HasForeignKey<clinical.APIs.Modules.DentalClinic.Models.EHR>(e => e.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<clinical.APIs.Modules.DentalClinic.Models.Appointment>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.Patient_ID)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<clinical.APIs.Modules.DentalClinic.Models.Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.Doctor_ID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<clinical.APIs.Modules.DentalClinic.Models.Appointment>()
                .HasOne(a => a.Nurse)
                .WithMany(n => n.Appointments)
                .HasForeignKey(a => a.Nurse_ID)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<clinical.APIs.Modules.DentalClinic.Models.Stock_Transaction>()
                .HasOne(st => st.Supply)
                .WithMany(s => s.StockTransactions)
                .HasForeignKey(st => st.Supply_ID)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<clinical.APIs.Modules.DentalClinic.Models.Stock_Transaction>()
                .HasOne(st => st.Doctor)
                .WithMany(d => d.StockTransactions)
                .HasForeignKey(st => st.Doctor_ID)
                .OnDelete(DeleteBehavior.Restrict);

            // EHR Change Log relationships
            modelBuilder.Entity<clinical.APIs.Modules.DentalClinic.Models.EHRChangeLog>()
                .HasOne(cl => cl.EHR)
                .WithMany(e => e.ChangeLogs)
                .HasForeignKey(cl => cl.EHR_ID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<clinical.APIs.Modules.DentalClinic.Models.EHRChangeLog>()
                .HasOne(cl => cl.Doctor)
                .WithMany()
                .HasForeignKey(cl => cl.ChangedByDoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<clinical.APIs.Modules.DentalClinic.Models.EHRChangeLog>()
                .HasOne(cl => cl.Appointment)
                .WithMany()
                .HasForeignKey(cl => cl.AppointmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // MedicationRecord relationships
            modelBuilder.Entity<clinical.APIs.Modules.DentalClinic.Models.MedicationRecord>()
                .HasOne(m => m.EHR)
                .WithMany(e => e.Medications)
                .HasForeignKey(m => m.EHR_ID)
                .OnDelete(DeleteBehavior.Cascade);

            // ProcedureRecord relationships
            modelBuilder.Entity<clinical.APIs.Modules.DentalClinic.Models.ProcedureRecord>()
                .HasOne(p => p.EHR)
                .WithMany(e => e.Procedures)
                .HasForeignKey(p => p.EHR_ID)
                .OnDelete(DeleteBehavior.Cascade);

            // ToothRecord relationships
            modelBuilder.Entity<clinical.APIs.Modules.DentalClinic.Models.ToothRecord>()
                .HasOne(t => t.EHR)
                .WithMany(e => e.Teeth)
                .HasForeignKey(t => t.EHR_ID)
                .OnDelete(DeleteBehavior.Cascade);

            // XRayRecord relationships
            modelBuilder.Entity<clinical.APIs.Modules.DentalClinic.Models.XRayRecord>()
                .HasOne(x => x.EHR)
                .WithMany(e => e.XRays)
                .HasForeignKey(x => x.EHR_ID)
                .OnDelete(DeleteBehavior.Cascade);
                    // Radiology Module relationships
                    modelBuilder.Entity<Report>()
                        .HasOne(r => r.Radiologist)
                        .WithMany(rad => rad.Reports)
                        .HasForeignKey(r => r.RadiologistID)
                        .OnDelete(DeleteBehavior.Restrict);

                    modelBuilder.Entity<Report>()
                        .HasOne(r => r.Patient)
                        .WithMany(p => p.Reports)
                        .HasForeignKey(r => r.PatientID)
                        .OnDelete(DeleteBehavior.Restrict);

                    modelBuilder.Entity<Report>()
                        .HasOne(r => r.ImagingAppointment)
                        .WithMany(i => i.Reports)
                        .HasForeignKey(r => r.ImagingID)
                        .OnDelete(DeleteBehavior.Restrict);


                }
            }
        }
