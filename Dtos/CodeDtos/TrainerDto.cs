using static fitback.Dtos.CodeDtos.TrainerCodeDto;

namespace fitback.Dtos.CodeDtos
{
    public class TrainerDto
    {
        // Trainer listesi / response
        public class TrainerResponseDto
        {
            public int Id { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }

            // Artık birden fazla kod listesi
            public List<string> Codes { get; set; } = new();
        }

        // Trainer detay / single
        public class TrainerDetailDto
        {
            public int Id { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }

            // Kodların detaylı bilgileri
            public List<TrainerCodeResponseDto> Codes { get; set; } = new();

            public List<StudentDto.StudentResponseDto> Students { get; set; } = new();
        }
    }
}
