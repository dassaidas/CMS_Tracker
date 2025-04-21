using System;
using System.Collections.Generic;

namespace CMS_Tracker.Models;

public partial class AuditLog
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public string? Action { get; set; }

    public DateTime? Timestamp { get; set; }

    public string? PerformedBy { get; set; }

    public string? Notes { get; set; }

    public virtual User? User { get; set; }
}
