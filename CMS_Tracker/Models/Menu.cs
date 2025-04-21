using System;
using System.Collections.Generic;

namespace CMS_Tracker.Models;

public partial class Menu
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public string? Path { get; set; }

    public string? Icon { get; set; }

    public Guid? ParentId { get; set; }

    public int? OrderIndex { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsDeleted { get; set; }

    public string? Type { get; set; }

    public string? Description { get; set; }

    public string? UpdatedBy { get; set; }

    public virtual ICollection<Menu> InverseParent { get; set; } = new List<Menu>();

    public virtual ICollection<MenuAudit> MenuAudits { get; set; } = new List<MenuAudit>();

    public virtual Menu? Parent { get; set; }

    public virtual ICollection<RoleMenu> RoleMenus { get; set; } = new List<RoleMenu>();
}
