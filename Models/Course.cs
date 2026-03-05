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

    public void updateModules(List<string> inputs)
    {
        var i = 0;
        foreach(var inp in inputs)
        {
            if(i > Modules.Count - 1)
            {
                Modules.Add(new Module { Title = inp, Content = inp });
            }
            else if (Modules.ElementAt(i).Title != inp)
            {
                Modules.ElementAt(i).Title = inp;
            }
            i += 1;
        }
        if(inputs.Count < Modules.Count)
        {
            var removedItems = Modules.Skip(i);
            foreach(var item in removedItems)
            {
                Modules.Remove(item);
            }
        }
    }
}

public class CourseInput
{
    public string title { get; set; } = null!;
    public string description { get; set; } = null!;
    public Decimal price { get; set; }
    public int duration { get; set; }
    public List<string> modules { get; set; }

    public Course toCourse()
    {
        var moduleCollections = modules.Select(m => new Module { Title = m, Content = m }).ToList();
        return new Course { Title = title, Description = description, Price = price, Duration = duration, Modules = moduleCollections };
    }
    public bool isPriceValid => price > 0.000001m;
    public bool isDurationValid => duration > 0;
}