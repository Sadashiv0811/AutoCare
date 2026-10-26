using AutoCare.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoCare.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Mechanic> Mechanics => Set<Mechanic>();
    public DbSet<ServiceType> ServiceTypes => Set<ServiceType>();
    public DbSet<Part> Parts => Set<Part>();
    public DbSet<ServiceJob> ServiceJobs => Set<ServiceJob>();
    public DbSet<ServiceJobPart> ServiceJobParts => Set<ServiceJobPart>();
    public DbSet<ServiceNote> ServiceNotes => Set<ServiceNote>();
    public DbSet<MaintenanceRecord> MaintenanceRecords => Set<MaintenanceRecord>();
    public DbSet<Invoice> Invoices => Set<Invoice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // =========================
        // User
        // =========================

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);

            entity.Property(u => u.FullName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(u => u.Role)
                .IsRequired()
                .HasMaxLength(30);

            entity.HasIndex(u => u.Email)
                .IsUnique();
        });


        // =========================
        // Customer
        // =========================

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.Property(c => c.FullName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(c => c.Phone)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(c => c.Email)
                .HasMaxLength(150);

            entity.Property(c => c.Address)
                .HasMaxLength(300);

            // User → Customer
            entity.HasOne(c => c.User)
                .WithOne()
                .HasForeignKey<Customer>(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // =========================
        // Vehicle
        // =========================

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(v => v.Id);

            entity.Property(v => v.RegistrationNumber)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(v => v.VIN)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(v => v.Make)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(v => v.Model)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(v => v.FuelType)
                .IsRequired()
                .HasMaxLength(30);

            entity.Property(v => v.Color)
                .HasMaxLength(30);

            entity.Property(v => v.Mileage)
                .HasPrecision(10, 2);

            entity.HasIndex(v => v.RegistrationNumber)
                .IsUnique();

            entity.HasIndex(v => v.VIN)
                .IsUnique();

            entity.HasOne(v => v.Customer)
                .WithMany(c => c.Vehicles)
                .HasForeignKey(v => v.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });


        // =========================
        // Mechanic
        // =========================

        modelBuilder.Entity<Mechanic>(entity =>
        {
            entity.HasKey(m => m.Id);

            entity.Property(m => m.FullName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(m => m.Phone)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(m => m.Email)
                .HasMaxLength(150);

            entity.Property(m => m.Specialization)
                .HasMaxLength(100);

            // User → Mechanic
            entity.HasOne(m => m.User)
                .WithOne()
                .HasForeignKey<Mechanic>(m => m.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });


        // =========================
        // Service Type
        // =========================

        modelBuilder.Entity<ServiceType>(entity =>
        {
            entity.HasKey(s => s.Id);

            entity.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(s => s.Description)
                .HasMaxLength(500);

            entity.Property(s => s.BasePrice)
                .HasPrecision(12, 2);

            entity.HasIndex(s => s.Name)
                .IsUnique();
        });


        // =========================
        // Part
        // =========================

        modelBuilder.Entity<Part>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.Property(p => p.PartNumber)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(p => p.Description)
                .HasMaxLength(500);

            entity.Property(p => p.UnitPrice)
                .HasPrecision(12, 2);

            entity.Property(p => p.Supplier)
                .HasMaxLength(150);

            entity.HasIndex(p => p.PartNumber)
                .IsUnique();
        });


        // =========================
        // Service Job
        // =========================

        modelBuilder.Entity<ServiceJob>(entity =>
        {
            entity.HasKey(s => s.Id);

            entity.Property(s => s.JobNumber)
                .IsRequired()
                .HasMaxLength(30);

            entity.Property(s => s.Mileage)
                .HasPrecision(10, 2);

            entity.Property(s => s.ProblemDescription)
                .HasMaxLength(1000);

            entity.Property(s => s.InspectionNotes)
                .HasMaxLength(2000);

            entity.Property(s => s.Status)
                .HasConversion<string>()
                .HasMaxLength(30);

            entity.HasIndex(s => s.JobNumber)
                .IsUnique();

            entity.HasOne(s => s.Vehicle)
                .WithMany(v => v.ServiceJobs)
                .HasForeignKey(s => s.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(s => s.Mechanic)
                .WithMany(m => m.ServiceJobs)
                .HasForeignKey(s => s.MechanicId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(s => s.ServiceType)
                .WithMany(st => st.ServiceJobs)
                .HasForeignKey(s => s.ServiceTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });


        // =========================
        // Service Job Part
        // =========================

        modelBuilder.Entity<ServiceJobPart>(entity =>
        {
            entity.HasKey(sp => sp.Id);

            entity.Property(sp => sp.UnitPrice)
                .HasPrecision(12, 2);

            entity.Property(sp => sp.TotalPrice)
                .HasPrecision(12, 2);

            entity.HasOne(sp => sp.ServiceJob)
                .WithMany(s => s.ServiceJobParts)
                .HasForeignKey(sp => sp.ServiceJobId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(sp => sp.Part)
                .WithMany(p => p.ServiceJobParts)
                .HasForeignKey(sp => sp.PartId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(sp => new
            {
                sp.ServiceJobId,
                sp.PartId
            }).IsUnique();
        });


        // =========================
        // Service Note
        // =========================

        modelBuilder.Entity<ServiceNote>(entity =>
        {
            entity.HasKey(sn => sn.Id);

            entity.Property(sn => sn.Note)
                .IsRequired()
                .HasMaxLength(2000);

            entity.HasOne(sn => sn.ServiceJob)
                .WithMany(s => s.ServiceNotes)
                .HasForeignKey(sn => sn.ServiceJobId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(sn => sn.Mechanic)
                .WithMany()
                .HasForeignKey(sn => sn.MechanicId)
                .OnDelete(DeleteBehavior.SetNull);
        });


        // =========================
        // Maintenance Record
        // =========================

        modelBuilder.Entity<MaintenanceRecord>(entity =>
        {
            entity.HasKey(mr => mr.Id);

            entity.Property(mr => mr.ServiceName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(mr => mr.Mileage)
                .HasPrecision(10, 2);

            entity.Property(mr => mr.Cost)
                .HasPrecision(12, 2);

            entity.Property(mr => mr.Notes)
                .HasMaxLength(1000);

            entity.HasOne(mr => mr.Vehicle)
                .WithMany(v => v.MaintenanceRecords)
                .HasForeignKey(mr => mr.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(mr => mr.ServiceJob)
                .WithMany()
                .HasForeignKey(mr => mr.ServiceJobId)
                .OnDelete(DeleteBehavior.SetNull);
        });


        // =========================
        // Invoice
        // =========================

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(i => i.Id);

            entity.Property(i => i.InvoiceNumber)
                .IsRequired()
                .HasMaxLength(30);

            entity.Property(i => i.ServiceCharges)
                .HasPrecision(12, 2);

            entity.Property(i => i.PartsAmount)
                .HasPrecision(12, 2);

            entity.Property(i => i.Subtotal)
                .HasPrecision(12, 2);

            entity.Property(i => i.TaxAmount)
                .HasPrecision(12, 2);

            entity.Property(i => i.TotalAmount)
                .HasPrecision(12, 2);

            entity.HasIndex(i => i.InvoiceNumber)
                .IsUnique();

            entity.HasIndex(i => i.ServiceJobId)
                .IsUnique();

            entity.HasOne(i => i.ServiceJob)
                .WithOne(s => s.Invoice)
                .HasForeignKey<Invoice>(i => i.ServiceJobId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

// | Relationship                   | Type |
// |---                             |---|
// | Customer → Vehicle             | 1-to-many |
// | Vehicle → ServiceJob           | 1-to-many |
// | Mechanic → ServiceJob          | 1-to-many |
// | ServiceType → ServiceJob       | 1-to-many |
// | ServiceJob → ServiceJobPart    | 1-to-many |
// | Part → ServiceJobPart          | 1-to-many |
// | ServiceJob → ServiceNote       | 1-to-many |
// | Vehicle → MaintenanceRecord    | 1-to-many |
// | ServiceJob → Invoice           | 1-to-1 |