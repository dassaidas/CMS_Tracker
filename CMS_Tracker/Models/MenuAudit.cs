using System;
using System.Collections.Generic;

namespace CMS_Tracker.Models;

public partial class MenuAudit
{
    public Guid AuditId { get; set; }

    public Guid? MenuId { get; set; }

    public string? Action { get; set; }

    public string? PerformedBy { get; set; }

    public DateTime? PerformedAt { get; set; }

    public string? Snapshot { get; set; }

    public virtual Menu? Menu { get; set; }
}
