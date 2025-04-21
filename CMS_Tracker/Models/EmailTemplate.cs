using System;
using System.Collections.Generic;

namespace CMS_Tracker.Models;

public partial class EmailTemplate
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Subject { get; set; }

    public string? Body { get; set; }

    public string? Type { get; set; }

    public bool? IsActive { get; set; }
}
