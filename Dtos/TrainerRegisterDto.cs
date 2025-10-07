namespace fitback.Dtos
{
    public class TrainerRegisterDto
    {
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Experience { get; set; } = string.Empty;
        public string Awards { get; set; } = string.Empty;
        public IFormFile? ProfileImage { get; set; }

    }
}
