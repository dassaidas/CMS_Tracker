using System;
using System.Collections.Generic;

namespace CMS_Tracker.Models;

public partial class RoleMenu
{
    public string Role { get; set; } = null!;

    public Guid MenuId { get; set; }

    public DateTime? AssignedAt { get; set; }

    public string? AssignedBy { get; set; }

    public virtual Menu Menu { get; set; } = null!;

    public virtual Role RoleNavigation { get; set; } = null!;
}
