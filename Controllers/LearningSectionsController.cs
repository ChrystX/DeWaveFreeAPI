using DeWaveFreeAPI.Data;
using DeWaveFreeAPI.DTOs;
using DeWaveFreeAPI.DTOs.LearningSections;
using DeWaveFreeAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeWaveFreeAPI.Controllers;

[ApiController]
[Route("api/courses/{courseId}/learning-sections")]
public class LearningSectionsController : ControllerBase
{
    private readonly DeWaveAPIDbContext _dbContext;

    public LearningSectionsController(DeWaveAPIDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<LearningSectionDto>>> GetSections(int courseId)
    {
        var sections = await _dbContext.CourseLearningSections
            .Where(s => s.CourseId == courseId && s.IsActive)
            .OrderBy(s => s.SortOrder)
            .Select(s => new LearningSectionDto
            {
                Id = s.Id,
                CourseId = s.CourseId,
                Title = s.Title,
                SortOrder = s.SortOrder,
                IsActive = s.IsActive
            })
            .ToListAsync();

        return Ok(sections);
    }

    [HttpGet("admin")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<LearningSectionDto>>> GetSectionsAdmin(int courseId)
    {
        var sections = await _dbContext.CourseLearningSections
            .Where(s => s.CourseId == courseId)
            .OrderBy(s => s.SortOrder)
            .Select(s => new LearningSectionDto
            {
                Id = s.Id,
                CourseId = s.CourseId,
                Title = s.Title,
                SortOrder = s.SortOrder,
                IsActive = s.IsActive
            })
            .ToListAsync();

        return Ok(sections);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<LearningSectionDto>> CreateSection(
       int courseId,
       CreateLearningSectionDto dto)
    {
        var section = new CourseLearningSection
        {
            CourseId = courseId,
            Title = dto.Title,
            SortOrder = dto.SortOrder,
            IsActive = dto.IsActive
        };

        _dbContext.CourseLearningSections.Add(section);
        await _dbContext.SaveChangesAsync();

        return Ok(new LearningSectionDto
        {
            Id = section.Id,
            CourseId = section.CourseId,
            Title = section.Title,
            SortOrder = section.SortOrder
        });
    }

    [HttpPut("{sectionId}")]
    [Authorize]
    public async Task<IActionResult> UpdateSection(
       int courseId,
       int sectionId,
       UpdateLearningSectionDto dto)
    {
        var section = await _dbContext.CourseLearningSections
            .FirstOrDefaultAsync(s => s.Id == sectionId && s.CourseId == courseId);

        if (section == null)
            return NotFound();

        section.Title = dto.Title;
        section.SortOrder = dto.SortOrder;
        section.IsActive = dto.IsActive;

        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("syllabus")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<SyllabusSectionDto>>> GetSyllabus(int courseId)
    {
        var sections = await _dbContext.CourseLearningSections
            .Where(s => s.CourseId == courseId && s.IsActive)
            .OrderBy(s => s.SortOrder)
            .Select(s => new SyllabusSectionDto
            {
                Id = s.Id,
                Title = s.Title,
                SortOrder = s.SortOrder,
                Lessons = s.Lessons
                    .OrderBy(l => l.SortOrder)
                    .Select(l => new SyllabusLessonDto
                    {
                        Id = l.Id,
                        Title = l.Title,
                        SortOrder = l.SortOrder
                    })
                    .ToList()
            })
            .ToListAsync();

        return Ok(sections);
    }

    [HttpDelete("{sectionId}")]
    [Authorize]
    public async Task<IActionResult> DeleteSection(int courseId, int sectionId)
    {
        var section = await _dbContext.CourseLearningSections
            .FirstOrDefaultAsync(s => s.Id == sectionId && s.CourseId == courseId);

        if (section == null)
            return NotFound();

        _dbContext.CourseLearningSections.Remove(section);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

}
