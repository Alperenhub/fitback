using System.Text;
using fitback.Data;
using fitback.Dtos;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using fitback.Models;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace fitback.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public StudentController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] StudentLoginDto dto)
        {
            var student = _context.Students.FirstOrDefault(s => s.Username == dto.Username);
            if (student == null)
                return Unauthorized(new { message = "Kullanıcı adı veya şifre geçersiz" });

            var passwordHash = ComputeSha256Hash(dto.Password);
            if (student.PasswordHash != passwordHash)
                return Unauthorized(new { message = "Kullanıcı adı veya şifre geçersiz" });

            // Access token
            var accessToken = GenerateAccessToken(student);

            // Refresh token
            var refreshToken = GenerateRefreshToken();

            // Refresh token'ı DB'ye kaydet
            student.RefreshToken = refreshToken;
            student.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            _context.SaveChanges();

            // Refresh token'ı HttpOnly Cookie olarak setle
            Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            });

            return Ok(new
            {
                accessToken,
                studentId = student.Id,
                username = student.Username,
                role = "Student"
            });
        }

        [HttpPost("refresh")]
        public IActionResult Refresh()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (refreshToken == null)
                return Unauthorized(new { message = "Refresh token yok" });

            var student = _context.Students.FirstOrDefault(s => s.RefreshToken == refreshToken);
            if (student == null || student.RefreshTokenExpiry < DateTime.UtcNow)
                return Unauthorized(new { message = "Geçersiz refresh token" });

            var newAccessToken = GenerateAccessToken(student);
            var newRefreshToken = GenerateRefreshToken();

            student.RefreshToken = newRefreshToken;
            student.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            _context.SaveChanges();

            Response.Cookies.Append("refreshToken", newRefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            });

            return Ok(new { accessToken = newAccessToken });
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] StudentRegisterDto dto)
        {
            if (_context.Students.Any(s => s.Email == dto.Email))
                return BadRequest(new { message = "Email zaten var" });

            if (_context.Students.Any(s => s.Username == dto.Username))
                return BadRequest(new { message = $"{dto.Username} zaten var" });

            // Şifreyi hashle
            var passwordHash = ComputeSha256Hash(dto.Password);

            // Upload klasörü
            var uploadDir = Path.Combine("wwwroot", "uploads", "students");
            if (!Directory.Exists(uploadDir))
                Directory.CreateDirectory(uploadDir);

            // Profil fotoğrafı
            string? profilePhotoUrl = null;
            if (dto.ProfilePhotoUrl != null)
            {
                var fileName = $"{Guid.NewGuid()}_{dto.ProfilePhotoUrl.FileName}";
                var path = Path.Combine(uploadDir, fileName);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await dto.ProfilePhotoUrl.CopyToAsync(stream);
                }
                profilePhotoUrl = $"/uploads/students/{fileName}";
            }

            // Diğer fotoğraflar
            List<string> otherPhotoUrls = new List<string>();
            if (dto.OtherPhotos != null && dto.OtherPhotos.Count > 0)
            {
                foreach (var file in dto.OtherPhotos)
                {
                    var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                    var path = Path.Combine(uploadDir, fileName);
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    otherPhotoUrls.Add($"/uploads/students/{fileName}");
                }
            }

            // Tokenları üret (önceden!)
            var accessToken = GenerateAccessToken(new Student { Id = 0, Username = dto.Username });
            var refreshToken = GenerateRefreshToken();

            // Student objesini oluştur
            var student = new Student
            {
                Email = dto.Email,
                Username = dto.Username,
                PasswordHash = passwordHash,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Age = dto.Age,
                Weight = dto.Weight,
                Height = dto.Height,
                Gender = dto.Gender,
                Description = dto.Description,
                Arm = dto.Arm,
                Waist = dto.Waist,
                Leg = dto.Leg,
                Shoulder = dto.Shoulder,
                ProfilePhotoUrl = profilePhotoUrl,
                OtherPhotosUrls = otherPhotoUrls.Count > 0 ?
                    System.Text.Json.JsonSerializer.Serialize(otherPhotoUrls) : null,

                // direkt ekle → null hatasını önler
                RefreshToken = refreshToken,
                RefreshTokenExpiry = DateTime.UtcNow.AddDays(7)
            };

            // DB’ye kaydet
            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            // Access token yeniden oluştur (artık Id belli)
            accessToken = GenerateAccessToken(student);

            // Cookie ayarı
            var isDevelopment = _configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT") == "Development";
            Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = !isDevelopment, // localhost’ta false
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            });

            return Ok(new
            {
                accessToken,
                studentId = student.Id,
                username = student.Username,
                role = "Student",
                message = "Student registered successfully"
            });
        }

        private static string ComputeSha256Hash(string rawData)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                var builder = new StringBuilder();
                foreach (var b in bytes)
                    builder.Append(b.ToString("x2"));
                return builder.ToString();
            }
        }

        private string GenerateAccessToken(Student student)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
            new Claim(ClaimTypes.NameIdentifier, student.Id.ToString()),
            new Claim(ClaimTypes.Name, student.Username),
            new Claim(ClaimTypes.Role, "Student")
                }),
                Expires = DateTime.UtcNow.AddMinutes(15), // kısa ömürlü
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

    }
}
