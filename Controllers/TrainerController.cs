using fitback.Data;
using fitback.Dtos;
using fitback.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[ApiController]
[Route("api/[controller]")]
public class TrainerController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _env;

    public TrainerController(AppDbContext context, IConfiguration configuration, IWebHostEnvironment env)
    {
        _context = context;
        _configuration = configuration;
        _env = env;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromForm] TrainerRegisterDto dto)
    {
        if (_context.Trainers.Any(t => t.Username == dto.Username))
            return BadRequest(new { message = "Bu kullanıcı adı zaten alınmış" });

        string filePath = "";
        if (dto.ProfileImage != null)
        {
            var uploads = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "trainers");
            if (!Directory.Exists(uploads))
                Directory.CreateDirectory(uploads);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.ProfileImage.FileName);
            filePath = Path.Combine(uploads, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.ProfileImage.CopyToAsync(stream);
            }

            filePath = "/uploads/trainers/" + fileName;
        }

        var trainer = new Trainer
        {
            Email = dto.Email,
            Username = dto.Username,
            PasswordHash = ComputeSha256Hash(dto.Password),
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Age = dto.Age,
            Gender = dto.Gender,
            Experience = dto.Experience,
            Awards = dto.Awards,
            ProfileImagePath = filePath
        };

        _context.Trainers.Add(trainer);
        await _context.SaveChangesAsync();

        // Token üret
        var accessToken = GenerateAccessToken(trainer);
        var refreshToken = GenerateRefreshToken();

        trainer.RefreshToken = refreshToken;
        trainer.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        _context.SaveChanges();

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
            trainerId = trainer.Id,
            username = trainer.Username,
            role = "Trainer",
            message = "Trainer kaydı başarılı"
        });
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] TrainerLoginDto dto)
    {
        var trainer = _context.Trainers.FirstOrDefault(t => t.Username == dto.Username);
        if (trainer == null)
            return Unauthorized(new { message = "Kullanıcı adı veya şifre geçersiz" });

        var passwordHash = ComputeSha256Hash(dto.Password);
        if (trainer.PasswordHash != passwordHash)
            return Unauthorized(new { message = "Kullanıcı adı veya şifre geçersiz" });

        var accessToken = GenerateAccessToken(trainer);
        var refreshToken = GenerateRefreshToken();

        trainer.RefreshToken = refreshToken;
        trainer.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        _context.SaveChanges();

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
            trainerId = trainer.Id,
            username = trainer.Username,
            role = "Trainer"
        });
    }

    private string GenerateAccessToken(Trainer trainer)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, trainer.Id.ToString()),
                new Claim(ClaimTypes.Name, trainer.Username),
                new Claim(ClaimTypes.Role, "Trainer")
            }),
            Expires = DateTime.UtcNow.AddMinutes(15), // kısa süreli token
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    }

    private static string ComputeSha256Hash(string rawData)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
        var builder = new StringBuilder();
        foreach (var b in bytes)
            builder.Append(b.ToString("x2"));
        return builder.ToString();
    }
}
