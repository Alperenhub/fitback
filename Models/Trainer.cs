using fitback.Models;
using System.ComponentModel.DataAnnotations;

public class Trainer
{
    public int Id { get; set; }

    [Required]
    public string Email { get; set; }
    [Required]
    public string Username { get; set; }
    [Required]
    public string PasswordHash { get; set; }
    [Required]
    public string FirstName { get; set; }
    [Required]
    public string LastName { get; set; }

    public int Age { get; set; }
    public string Gender { get; set; }
    public string Experience { get; set; }
    public string Awards { get; set; }
    public string ProfileImagePath { get; set; }

    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }

    // 🔑 İlişki: 1 trainer → N trainercode
    public List<TrainerCode> TrainerCodes { get; set; } = new();

    // Öğrenciler
    public List<Student> Students { get; set; } = new();
}
