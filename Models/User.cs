using System;
using System.Collections.Generic;

namespace gsa_api.Models;

public class User
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Role { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
}

public class InputtedUser
{
    public string FullName { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public User toUser()
    {
        return new User { Name = FullName, Username = Username, Email = Email, PasswordHash = Password, Role = "student"};
    }
}

public class Credential
{
    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;
}
