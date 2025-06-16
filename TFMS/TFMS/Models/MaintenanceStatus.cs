// Models/MaintenanceStatus.cs
using System.ComponentModel; // ADD THIS USING DIRECTIVE

namespace TFMS.Models
{
    public enum MaintenanceStatus
    {
        [Description("Scheduled")]
        Scheduled,
        [Description("In Progress")] // Add Description attribute for mapping
        InProgress,
        [Description("Completed")]
        Completed,
        [Description("Overdue")]
        Overdue,
        [Description("Delayed")]
        Delayed,
        [Description("Cancelled")] // Add Description attribute
        Cancelled
    }
}