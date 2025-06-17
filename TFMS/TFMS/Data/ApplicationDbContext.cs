using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion; // ADD THIS USING
using System.Reflection; // ADD THIS USING for Extension Methods
using System.ComponentModel; // ADD THIS USING for DescriptionAttribute
using TFMS.Models; // Make sure this using directive points to your Models folder
using System; // For Enum
using System.Linq; // For .FirstOrDefault() in ToEnumValue

namespace TFMS.Data // Your correct namespace
{
    // Change IdentityUser to ApplicationUser here
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Add DbSet for each of your new entities
        public DbSet<Vehicle> Vehicles { get; set; } = default!; // Added default! for non-nullable DbSet
        public DbSet<Trip> Trips { get; set; } = default!;
        public DbSet<FuelRecord> FuelRecords { get; set; } = default!;
        public DbSet<Maintenance> MaintenanceRecords { get; set; } = default!; // Renamed for clarity, Added default!
        public DbSet<PerformanceReport> PerformanceReports { get; set; } = default!;

        // Optional: Configure relationships or default behavior using Fluent API
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names to something different.
            // See the ASP.NET Identity documentation for more details.

            // Configure enum conversion for MaintenanceStatus with custom converter
            var converter = new ValueConverter<MaintenanceStatus, string>(
                v => v.GetDescription(), // Convert enum to string (using description or ToString)
                v => v.ToEnumValue<MaintenanceStatus>() // Convert string to enum
            );

            builder.Entity<Maintenance>()
                .Property(m => m.Status)
                .HasConversion(converter); // Use the custom converter

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

    // --- Extension Methods for Enum Description and Parsing (MUST be outside ApplicationDbContext class, but in the same namespace) ---
    public static class EnumExtensions
    {
        // Extension method to get the Description attribute value of an enum
       

        // Extension method to convert a string back to an enum value, using Description attribute or name
        public static TEnum ToEnumValue<TEnum>(this string stringValue) where TEnum : Enum
        {
            // Try to match by DescriptionAttribute first
            foreach (var field in typeof(TEnum).GetFields())
            {
                if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
                {
                    if (attribute.Description.Equals(stringValue, StringComparison.OrdinalIgnoreCase))
                    {
                        return (TEnum)field.GetValue(null);
                    }
                }
            }

            // If no match by Description, try to parse by name (case-insensitive)
            if (Enum.TryParse(typeof(TEnum), stringValue, true, out var enumResult))
            {
                return (TEnum)enumResult;
            }

            // Fallback: If parsing fails, you might want to throw an exception or return a default value.
            // For now, throwing an ArgumentException is a reasonable default.
            throw new ArgumentException($"'{stringValue}' is not a valid value for enum {typeof(TEnum).Name}.");
        }
    }
}