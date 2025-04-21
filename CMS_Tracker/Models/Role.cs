using System;
using System.Collections.Generic;

namespace CMS_Tracker.Models;

public partial class Role
{
    public string Name { get; set; } = null!;

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public virtual ICollection<RoleMenu> RoleMenus { get; set; } = new List<RoleMenu>();
}
