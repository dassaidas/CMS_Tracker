using System;
using System.Collections.Generic;

namespace CMS_Tracker.Models;

public partial class UserToken
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public string? Token { get; set; }

    public string? Type { get; set; }

    public DateTime? Expiry { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User? User { get; set; }
}
