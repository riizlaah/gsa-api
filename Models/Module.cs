using System;
using System.Collections.Generic;

namespace gsa_api.Models;

public partial class Module
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public string Title { get; set; } = null!;

    public string? Content { get; set; }

    public virtual Course Course { get; set; } = null!;
}
