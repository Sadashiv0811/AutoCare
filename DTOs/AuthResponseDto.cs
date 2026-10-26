namespace AutoCare.Api.DTOs;

public class AuthResponseDto
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public string Token { get; set; } = string.Empty;
}

// Returns JSON
// {
//   "id": 1,
//   "fullName": "John",
//   "email": "john@gmail.com",
//   "role": "Customer",
//   "token": "eyJhbGciOiJIUzI1NiIs..."
// }