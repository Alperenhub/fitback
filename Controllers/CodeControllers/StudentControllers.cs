using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using fitback.Data;
using fitback.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace fitback.Controllers.CodeControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StudentController(AppDbContext context)
        {
            _context = context;
        }

        // Öğrenci Trainer’a katılır
        [HttpPost("join")]
        [Authorize(Roles = "Student")]
        public IActionResult JoinTrainer([FromBody] JoinTrainerRequest dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Code))
                return BadRequest(new { message = "Geçersiz istek." });

            // Token'dan studentId al
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized(new { message = "Token geçersiz." });

            if (!int.TryParse(userIdClaim.Value, out int studentId))
                return Unauthorized(new { message = "Token geçersiz." });

            var student = _context.Students.FirstOrDefault(s => s.Id == studentId);
            if (student == null)
                return NotFound(new { message = "Öğrenci bulunamadı." });

            if (student.TrainerId.HasValue)
                return BadRequest(new { message = "Öğrenci zaten bir trainer'a bağlı." });

            var trainerCode = _context.TrainerCodes.FirstOrDefault(c => c.Code == dto.Code);
            if (trainerCode == null)
                return BadRequest(new { message = "Geçersiz kod." });

            if (trainerCode.ExpiresAt.HasValue && trainerCode.ExpiresAt.Value < DateTime.UtcNow)
                return BadRequest(new { message = "Kodun süresi dolmuş." });

            if (trainerCode.Quota <= 0)
                return BadRequest(new { message = "Kodun kotası dolmuş." });

            if (!trainerCode.TrainerId.HasValue)
                return BadRequest(new { message = "Koda ait trainer henüz atanmamış." });

            var trainerId = trainerCode.TrainerId.Value;
            var trainer = _context.Trainers.FirstOrDefault(t => t.Id == trainerId);
            if (trainer == null)
                return BadRequest(new { message = "Koda ait trainer bulunamadı." });

            // 🔹 Atama ve quota azaltma
            student.TrainerId = trainer.Id;
            student.TrainerCodeId = trainerCode.Id; // öğrenci hangi kodla bağlandığını kaydettik
            trainerCode.Quota -= 1;

            _context.SaveChanges();

            return Ok(new
            {
                message = "Trainer ile başarıyla eşleştirildiniz.",
                trainerId = trainer.Id,
                trainerCodeId = trainerCode.Id,
                remainingQuota = trainerCode.Quota
            });
        }
    }

    public class JoinTrainerRequest
    {
        public string Code { get; set; }
    }
}
