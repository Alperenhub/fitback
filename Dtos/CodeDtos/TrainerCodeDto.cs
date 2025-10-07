// TrainerCodeDto.cs
using fitback.Models;

namespace fitback.Dtos.CodeDtos
{
    public class TrainerCodeDto
    {
        public class GenerateCodeDto
        {
            public int DurationInMonths { get; set; }
            public int Quota { get; set; }
        }

        public class UpdateCodeDto
        {
            public int DurationInMonths { get; set; }
            public int Quota { get; set; }
        }

        public class TrainerCodeResponseDto
        {
            public int Id { get; set; }
            public int? TrainerId { get; set; }
            public string Code { get; set; }
            public int DurationInMonths { get; set; }
            public int Quota { get; set; }
            public bool IsUsed { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? ExpiresAt { get; set; }

            // 🔹 Yeni ekleme
            public TrainerInfoDto Trainer { get; set; }

            public List<StudentDto.StudentResponseDto> Students { get; set; } = new();
        }

        // Trainer detay DTO
        public class TrainerInfoDto
        {
            public int Id { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }
    }
}
