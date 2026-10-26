namespace AutoCare.Models;

public class Customer
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Address { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign key to User
    public int UserId { get; set; }

    // Navigation property
    public User User { get; set; } = null!;

    // Navigation property
    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}

// A customer can own multiple vehicles:
// Customer
//    │
//    ├── Vehicle 1
//    ├── Vehicle 2
//    └── Vehicle 3