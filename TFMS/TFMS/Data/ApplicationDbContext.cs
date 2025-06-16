// Data/ApplicationDbContext.cs
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion; // ADD THIS USING
using System.Reflection; // ADD THIS USING for Extension Methods
using System.ComponentModel; // ADD THIS USING for DescriptionAttribute
using TFMS.Models; // Make sure this using directive points to your Models folder
using System; // For Enum

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
        // Gets the Description attribute value of an enum member
        public static string GetDescription<TEnum>(this TEnum value)
            where TEnum : Enum
        {
            var fieldInfo = value.GetType().GetField(value.ToString());
            if (fieldInfo == null) return value.ToString(); // Fallback if fieldInfo is null (shouldn't happen for enum members)

            var attribute = (DescriptionAttribute?)Attribute.GetCustomAttribute(fieldInfo, typeof(DescriptionAttribute));
            return attribute?.Description ?? value.ToString(); // Return description or ToString() if no attribute
        }

        // Converts a string to an enum value, trying to match Description attribute first, then enum name
        public static TEnum ToEnumValue<TEnum>(this string str)
            where TEnum : Enum
        {
            foreach (var field in typeof(TEnum).GetFields())
            {
                if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
                {
                    if (attribute.Description.Equals(str, StringComparison.OrdinalIgnoreCase))
                        return (TEnum)field.GetValue(null)!; // Use ! for null-forgiving operator as we know it's not null here
                }

                if (field.Name.Equals(str, StringComparison.OrdinalIgnoreCase))
                    return (TEnum)field.GetValue(null)!; // Use ! for null-forgiving operator
            }
            // If no match, try parsing directly as a fallback (will throw if invalid string)
            return (TEnum)Enum.Parse(typeof(TEnum), str, true);
        }
    }
}
