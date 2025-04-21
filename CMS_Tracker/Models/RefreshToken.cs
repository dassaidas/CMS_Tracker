using System;
using System.Collections.Generic;

namespace CMS_Tracker.Models;

public partial class RefreshToken
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public string? Token { get; set; }

    public DateTime? Expiry { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? IsUsed { get; set; }

    public bool? IsRevoked { get; set; }

    public virtual User? User { get; set; }
}
