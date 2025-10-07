using System.ComponentModel.DataAnnotations;

namespace fitback.Models

{
    public class Student
    {
        [Key]
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
        public float Weight { get; set; }
        public float Height { get; set; }
        public string Gender { get; set; }
        public string Description { get; set; }
        
        public float Arm { get; set; }
        public float Waist { get; set; }
        public float Leg {  get; set; }
        public float Shoulder { get; set; }

        public string? ProfilePhotoUrl { get; set; }
        public string? OtherPhotosUrls { get; set; }
        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpiry {  get; set; }

        //Trainer ile ilişki
        public int? TrainerId { get; set; }
        public Trainer? Trainer { get; set; }

        // 🔹 Öğrenci hangi kodla bağlandı
        public int? TrainerCodeId { get; set; }
        public TrainerCode? TrainerCode { get; set; }

        //Trainer onay durumu
        public bool IsApprovedByTrainer { get; set; } = false;
        //Student kaydolduğu tarih
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


    }
}
