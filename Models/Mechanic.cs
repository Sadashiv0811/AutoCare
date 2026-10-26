namespace AutoCare.Models;

public class Mechanic
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Specialization { get; set; }

    public int ExperienceYears { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign key
    public int UserId { get; set; }

    // Navigation property
    public User User { get; set; } = null!;

    public ICollection<ServiceJob> ServiceJobs { get; set; } =
        new List<ServiceJob>();
}

// A mechanic can handle multiple service jobs:
// Mechanic
//    │
//    ├── ServiceJob 1001
//    ├── ServiceJob 1002
//    └── ServiceJob 1003