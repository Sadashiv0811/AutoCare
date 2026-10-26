namespace AutoCare.Models;

public class User
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// | Field          | Purpose |
// |---             |---|
// | `Id`           | Primary key |
// | `FullName`     | User's name |
// | `Email`        | Login/account email |
// | `PasswordHash` | Stores hashed password, never plaintext |
// | `Role`         | `Admin`, `ServiceAdvisor`, `Mechanic`, `Customer` |
// | `IsActive`     | Enable/disable account |
// | `CreatedAt`    | Account creation timestamp |

// We'll later use Role for JWT role-based authorization.