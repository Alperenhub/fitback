namespace fitback.Dtos
{
    public class StudentRegisterDto
    {
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public int Age { get; set; }
        public float Weight { get; set; }
        public float Height { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string? Description { get; set; } = null;

        public float Arm { get; set; }
        public float Waist { get; set; }
        public float Leg { get; set; }
        public float Shoulder { get; set; }

        public IFormFile? ProfilePhotoUrl { get; set; }
        public List<IFormFile> OtherPhotos { get; set; } = new List<IFormFile>(); 
    }
}
