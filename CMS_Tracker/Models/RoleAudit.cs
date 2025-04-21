using System;
using System.Collections.Generic;

namespace CMS_Tracker.Models;

public partial class RoleAudit
{
    public Guid AuditId { get; set; }

    public string? Role { get; set; }

    public string? Action { get; set; }

    public string? PerformedBy { get; set; }

    public DateTime? PerformedAt { get; set; }

    public string? Notes { get; set; }
}
