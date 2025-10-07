using System.Security.Claims;
using fitback.Data;
using fitback.Dtos.CodeDtos;
using fitback.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static fitback.Dtos.CodeDtos.StudentDto;
using static fitback.Dtos.CodeDtos.TrainerCodeDto;

namespace fitback.Controllers.CodeControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrainerCodeController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TrainerCodeController(AppDbContext context)
        {
            _context = context;
        }

        // Kod oluşturma (Admin)
        [HttpPost("generate")]
        public IActionResult GenerateCode([FromBody] GenerateCodeDto dto)
        {
            var code = new Random().Next(100000, 999999).ToString();
            var now = DateTime.UtcNow;

            var trainerCode = new TrainerCode
            {
                Code = code,
                DurationInMonths = dto.DurationInMonths,
                Quota = dto.Quota,
                CreatedAt = now,
                ExpiresAt = now.AddMonths(dto.DurationInMonths),
                IsUsed = false
            };

            _context.TrainerCodes.Add(trainerCode);
            _context.SaveChanges();

            return Ok(new TrainerCodeResponseDto
            {
                Id = trainerCode.Id,
                TrainerId = trainerCode.TrainerId,
                Code = trainerCode.Code,
                DurationInMonths = trainerCode.DurationInMonths,
                Quota = trainerCode.Quota,
                IsUsed = trainerCode.IsUsed,
                CreatedAt = trainerCode.CreatedAt,
                ExpiresAt = trainerCode.ExpiresAt,
            });
        }

        [Authorize(Roles = "Trainer")]
        [HttpPost("validate")]
        public async Task<IActionResult> ValidateCode([FromBody] ValidateCodeDto request)
        {
            // Token'dan trainer id al
            var trainerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (trainerIdClaim == null)
                return Unauthorized();

            int trainerId = int.Parse(trainerIdClaim);

            var trainerCode = await _context.TrainerCodes.FirstOrDefaultAsync(c => c.Code == request.Code);

            if (trainerCode == null)
                return NotFound(new { message = "Kod bulunamadı" });

            if (trainerCode.ExpiresAt < DateTime.UtcNow)
                return BadRequest(new { message = "Kodun süresi dolmuş" });

            if (trainerCode.Quota <= 0)
                return BadRequest(new { message = "Kodun kotası dolmuş" });

            // Buraya ekle:
            if (!trainerCode.IsUsed && trainerCode.TrainerId == null)
            {
                trainerCode.IsUsed = true;           // Trainer doğruladı → kullanıldı
                //trainerCode.TrainerId = request.TrainerId; // TrainerId gönderiyorsan ata
                await _context.SaveChangesAsync();
            }

            // Trainer ile ilişkilendir
            trainerCode.TrainerId = trainerId;

            // Quota azalt (istersen)
            trainerCode.Quota--;

            await _context.SaveChangesAsync();

            return Ok(trainerCode);
        }

        // Kod listesi (Admin)
        [HttpGet("list")]
        public IActionResult GetTrainerCodes()
        {
            var codes = _context.TrainerCodes
                .Include(c => c.Students)
                .Include(c => c.Trainer)
                .Select(c => new TrainerCodeResponseDto
                {
                    Id = c.Id,
                    Code = c.Code,
                    DurationInMonths = c.DurationInMonths,
                    Quota = c.Quota,
                    IsUsed = c.IsUsed,
                    CreatedAt = c.CreatedAt,
                    ExpiresAt = c.ExpiresAt,
                    TrainerId = c.TrainerId,

                    // 🔹 Trainer objesini doldur
                    Trainer = c.Trainer == null ? null : new TrainerInfoDto
                    {
                        Id = c.Trainer.Id,
                        Username = c.Trainer.Username,
                        Email = c.Trainer.Email,
                        FirstName = c.Trainer.FirstName,
                        LastName = c.Trainer.LastName
                    },

                    Students = c.Students.Select(s => new StudentDto.StudentResponseDto
                    {
                        Id = s.Id,
                        Username = s.Username,
                        FirstName = s.FirstName,
                        LastName = s.LastName,
                        TrainerId = s.TrainerId,
                        TrainerCodeId = s.TrainerCodeId,
                        IsApprovedByTrainer = s.IsApprovedByTrainer
                    }).ToList()
                })
                .ToList();

            return Ok(codes);
        }

        // Kod güncelleme (Admin)
        [HttpPut("update/{id}")]
        public IActionResult UpdateCode(int id, [FromBody] UpdateCodeDto dto)
        {
            var code = _context.TrainerCodes.FirstOrDefault(c => c.Id == id);
            if (code == null)
                return NotFound("Kod bulunamadı.");

            code.DurationInMonths = dto.DurationInMonths;
            code.Quota = dto.Quota;

            if (!code.IsUsed)
                code.ExpiresAt = code.CreatedAt.AddMonths(dto.DurationInMonths);

            _context.SaveChanges();

            return Ok(new TrainerCodeResponseDto
            {
                Id = code.Id,
                Code = code.Code,
                DurationInMonths = code.DurationInMonths,
                Quota = code.Quota,
                IsUsed = code.IsUsed,
                CreatedAt = code.CreatedAt,
                ExpiresAt = code.ExpiresAt,
                TrainerId = code.TrainerId
            });
        }

        // Kod silme (Admin)
        [HttpDelete("delete/{id}")]
        public IActionResult DeleteCode(int id)
        {
            var code = _context.TrainerCodes.FirstOrDefault(c => c.Id == id);
            if (code == null)
                return NotFound("Kod bulunamadı.");

            _context.TrainerCodes.Remove(code);
            _context.SaveChanges();

            return Ok(new { message = "Kod başarıyla silindi." });
        }

        public class ValidateCodeDto
        {
            public string Code { get; set; }
        }
    }
}
