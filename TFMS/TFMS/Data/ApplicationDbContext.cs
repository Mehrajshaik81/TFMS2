// Data/ApplicationDbContext.cs
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TFMS.Models; // Make sure this using directive points to your Models folder

namespace TFMS.Data
{
    // Change IdentityUser to ApplicationUser here
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Add DbSet for each of your new entities
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Trip> Trips { get; set; }
        public DbSet<FuelRecord> FuelRecords { get; set; }
        public DbSet<Maintenance> MaintenanceRecords { get; set; } // Renamed for clarity
        public DbSet<PerformanceReport> PerformanceReports { get; set; }

        // Optional: Configure relationships or default behavior using Fluent API
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names to something different.
            // See the ASP.NET Identity documentation for more details.

            // Example: Ensure onDelete behavior for foreign keys
            builder.Entity<Trip>()
                .HasOne(t => t.Vehicle)
                .WithMany(v => v.Trips)
                .HasForeignKey(t => t.VehicleId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascading delete of vehicle if trips exist

            builder.Entity<Trip>()
                .HasOne(t => t.Driver)
                .WithMany(d => d.Trips)
                .HasForeignKey(t => t.DriverId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascading delete of driver if trips exist

            builder.Entity<FuelRecord>()
                .HasOne(f => f.Vehicle)
                .WithMany(v => v.FuelRecords)
                .HasForeignKey(f => f.VehicleId)
                .OnDelete(DeleteBehavior.Cascade); // If vehicle deleted, related fuel records also deleted

            builder.Entity<Maintenance>()
                .HasOne(m => m.Vehicle)
                .WithMany(v => v.MaintenanceRecords)
                .HasForeignKey(m => m.VehicleId)
                .OnDelete(DeleteBehavior.Cascade); // If vehicle deleted, related maintenance records also deleted
        }
    }
}