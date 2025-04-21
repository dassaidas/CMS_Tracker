using System;
using System.Collections.Generic;

namespace CMS_Tracker.Models;

public partial class TokenBlacklist
{
    public Guid Id { get; set; }

    public string Token { get; set; } = null!;

    public DateTime ExpiredAt { get; set; }

    public DateTime? CreatedAt { get; set; }
}
