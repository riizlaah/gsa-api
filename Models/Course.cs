using System;
using System.Collections.Generic;

namespace gsa_api.Models;

public partial class Course
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public decimal Price { get; set; }

    public int Duration { get; set; }
    public string DurationStr => $"{Duration} minutes";

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Module> Modules { get; set; } = new List<Module>();

    public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
}

public class CourseInput
{
    public string title { get; set; } = null!;
    public string description { get; set; } = null!;
    public decimal price { get; set; }
    public int duration { get; set; }
    public string[] modules { get; set; }

    public Course toCourse()
    {
        return new Course { Title = title, Description = description, Price = price, Duration = duration };
    }
}