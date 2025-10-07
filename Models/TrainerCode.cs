using fitback.Models;

public class TrainerCode
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public int DurationInMonths { get; set; }
    public int Quota { get; set; }
    public bool IsUsed { get; set; } = false;

    // 🔑 Trainer ilişkisi
    public int? TrainerId { get; set; }
    public Trainer? Trainer { get; set; }

    // 🔹 Bu kodu kullanan öğrenciler
    public List<Student> Students { get; set; } = new();

    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
