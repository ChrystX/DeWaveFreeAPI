using DeWaveFreeAPI.Data;
using DeWaveFreeAPI.DTOs;
using DeWaveFreeAPI.Extension;
using DeWaveFreeAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;

namespace DeWaveFreeAPI.Controllers
{
        // Controllers/StudentProfileController.cs
        [ApiController]
        [Route("api/student/profile")]
        [Authorize(Roles = "student")]
        public class StudentProfileController : ControllerBase
        {
            private readonly DeWaveAPIDbContext _dbContext;
            public StudentProfileController(DeWaveAPIDbContext context) => _dbContext = context;

            [HttpGet]
            public async Task<ActionResult<StudentProfileDto>> GetProfile()
            {
            var userId = User.GetUserId();
            if (userId is null) return Unauthorized();

            var student = await _dbContext.Students
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (student is null) return NotFound();

                return Ok(new StudentProfileDto
                {
                    FullName = student.FullName,
                    PhoneNumber = student.PhoneNumber,
                    DateOfBirth = student.DateOfBirth,
                    Address = student.Address,
                    EmergencyContact = student.EmergencyContact,
                    EmergencyPhone = student.EmergencyPhone,
                });
            }

            [HttpPut]
            public async Task<IActionResult> UpdateProfile([FromBody] StudentProfileDto dto)
            {
            var userId = User.GetUserId();
            if (userId is null) return Unauthorized();

            var student = await _dbContext.Students
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (student is null) return NotFound();

                student.FullName = dto.FullName;
                student.PhoneNumber = dto.PhoneNumber;
                student.DateOfBirth = dto.DateOfBirth;
                student.Address = dto.Address;
                student.EmergencyContact = dto.EmergencyContact;
                student.EmergencyPhone = dto.EmergencyPhone;

                await _dbContext.SaveChangesAsync();
                return NoContent();
            }
        }
}
