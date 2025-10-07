using fitback.Data;
using fitback.Dtos.CodeDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using static fitback.Dtos.CodeDtos.StudentDto;
using static fitback.Dtos.CodeDtos.TrainerCodeDto;
using static fitback.Dtos.CodeDtos.TrainerDto;

namespace fitback.Controllers.CodeControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrainerController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TrainerController(AppDbContext context)
        {
            _context = context;
        }

        // 🔹 Trainer kodu aktive eder (inputtan girerek)
        [HttpPost("activate-code")]
        [Authorize(Roles = "Trainer")]
        public IActionResult ActivateCode([FromBody] ActivateCodeDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Code))
                return BadRequest(new { message = "Kod geçersiz." });

            var trainerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (trainerIdClaim == null)
                return Unauthorized();

            int trainerId = int.Parse(trainerIdClaim.Value);

            var trainer = _context.Trainers
                .Include(t => t.TrainerCodes)
                .FirstOrDefault(t => t.Id == trainerId);

            if (trainer == null)
                return NotFound(new { message = "Trainer bulunamadı." });

            var trainerCode = _context.TrainerCodes
                .FirstOrDefault(c => c.Code == dto.Code);

            if (trainerCode == null)
                return BadRequest(new { message = "Kod bulunamadı." });

            if (trainerCode.ExpiresAt < DateTime.UtcNow)
                return BadRequest(new { message = "Kodun süresi dolmuş." });

            if (trainerCode.Quota <= 0)
                return BadRequest(new { message = "Kodun kotası dolmuş." });

            if (trainerCode.TrainerId.HasValue)
                return BadRequest(new { message = "Kod zaten başka bir trainera atanmış." });

            // İlişkiyi kur
            trainer.TrainerCodes.Add(trainerCode);
            trainerCode.TrainerId = trainer.Id;
            trainerCode.IsUsed = true;

            _context.SaveChanges();

            // Aktif kodu dön
            var activeCode = _context.TrainerCodes
                .Where(c => c.TrainerId == trainerId)
                .Include(c => c.Students)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new TrainerCodeResponseDto
                {
                    Id = c.Id,
                    TrainerId = c.TrainerId,
                    Code = c.Code,
                    DurationInMonths = c.DurationInMonths,
                    Quota = c.Quota,
                    IsUsed = c.IsUsed,
                    CreatedAt = c.CreatedAt,
                    ExpiresAt = c.ExpiresAt,
                    Students = c.Students.Select(s => new StudentResponseDto
                    {
                        Id = s.Id,
                        Username = s.Username,
                        FirstName = s.FirstName,
                        LastName = s.LastName
                    }).ToList()
                })
                .FirstOrDefault();

            return Ok(activeCode);
        }


        // 🔹 Trainer listesi (Admin görebilir)
        [HttpGet("list")]
        //[Authorize(Roles = "Admin")]
        public IActionResult GetTrainers()
        {
            var trainers = _context.Trainers
                .Include(t => t.TrainerCodes)
                .Select(t => new TrainerResponseDto
                {
                    Id = t.Id,
                    Username = t.Username,
                    Email = t.Email,
                    FirstName = t.FirstName,
                    LastName = t.LastName,
                    Codes = t.TrainerCodes.Select(tc => tc.Code).ToList()
                })
                .ToList();

            return Ok(trainers);
        }

        [HttpGet("active-code")]
        //[Authorize(Roles = "Trainer")]
        public IActionResult GetActiveCode()
        {
            var trainerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (trainerIdClaim == null)
                return Unauthorized();

            int trainerId = int.Parse(trainerIdClaim.Value);

            var code = _context.TrainerCodes
                .Where(c => c.TrainerId == trainerId)
                .Include(c => c.Students) // 🔹 Bunu ekle
                .Select(c => new TrainerCodeResponseDto
                {
                    Id = c.Id,
                    TrainerId = c.TrainerId,
                    Code = c.Code,
                    DurationInMonths = c.DurationInMonths,
                    Quota = c.Quota,
                    IsUsed = c.IsUsed,
                    CreatedAt = c.CreatedAt,
                    ExpiresAt = c.ExpiresAt,
                    Students = c.Students.Select(s => new StudentResponseDto
                    {
                        Id = s.Id,
                        Username = s.Username,
                        FirstName = s.FirstName,
                        LastName = s.LastName
                    }).ToList()
                })
                .FirstOrDefault();

            if (code == null)
                return NotFound();

            return Ok(code);
        }

        // 🔹 Trainer detay (Admin görebilir)
        [HttpGet("detail/{id}")]
        //[Authorize(Roles = "Admin")]
        public IActionResult GetTrainerDetail(int id)
        {
            var trainer = _context.Trainers
                .Include(t => t.TrainerCodes)
                .Include(t => t.Students)
                .Where(t => t.Id == id)
                .Select(t => new TrainerDetailDto
                {
                    Id = t.Id,
                    Username = t.Username,
                    Email = t.Email,
                    FirstName = t.FirstName,
                    LastName = t.LastName,
                    Codes = t.TrainerCodes.Select(tc => new TrainerCodeResponseDto
                    {
                        Id = tc.Id,
                        Code = tc.Code,
                        DurationInMonths = tc.DurationInMonths,
                        Quota = tc.Quota,
                        IsUsed = tc.IsUsed,
                        CreatedAt = tc.CreatedAt,
                        ExpiresAt = tc.ExpiresAt
                    }).ToList(),
                    Students = t.Students.Select(s => new StudentDto.StudentResponseDto
                    {
                        Id = s.Id,
                        Username = s.Username,
                        Email = s.Email,
                        FirstName = s.FirstName,
                        LastName = s.LastName,
                        TrainerId = s.TrainerId,
                        IsApprovedByTrainer = s.IsApprovedByTrainer,
                        TrainerCodeId = s.TrainerCodeId
                    }).ToList()
                })
                .FirstOrDefault();

            if (trainer == null)
                return NotFound("Trainer bulunamadı.");

            return Ok(trainer);
        }
    }

    // Request DTO
    public class ActivateCodeDto
    {
        public string Code { get; set; }
    }
}
