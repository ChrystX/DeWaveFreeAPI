using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DeWaveFreeAPI.Models;
using DeWaveFreeAPI.Data;

namespace DeWaveFreeAPI.Controllers
{
    public class CourseSectionDto
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string Title { get; set; } = null!;
        public string? ContentHtml { get; set; }
        public string? VideoUrl { get; set; }
        public string? ThumbnailUrl { get; set; }
        public int? DurationMinutes { get; set; }
        public int SortOrder { get; set; }
    }

    public class CourseSectionCreateDto
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = null!;
        public string? ContentHtml { get; set; }
        public string? VideoUrl { get; set; }
        public string? ThumbnailUrl { get; set; }
        public int? DurationMinutes { get; set; }
        public int SortOrder { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class CourseSectionsController : ControllerBase
    {
        private readonly DeWaveAPIDbContext _dbContext;

        public CourseSectionsController(DeWaveAPIDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourseSectionDto>>> GetCourseSections()
        {
            var sections = await _dbContext.CourseSections
                .Select(cs => new CourseSectionDto
                {
                    Id = cs.Id,
                    CourseId = cs.CourseId,
                    Title = cs.Title,
                    ContentHtml = cs.ContentHtml,
                    VideoUrl = cs.VideoUrl,
                    ThumbnailUrl = cs.ThumbnailUrl,
                    DurationMinutes = cs.DurationMinutes,
                    SortOrder = cs.SortOrder
                })
                .ToListAsync();

            return Ok(sections);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CourseSectionDto>> GetCourseSection(int id)
        {
            var cs = await _dbContext.CourseSections.FindAsync(id);

            if (cs == null) return NotFound();

            var dto = new CourseSectionDto
            {
                Id = cs.Id,
                CourseId = cs.CourseId,
                Title = cs.Title,
                ContentHtml = cs.ContentHtml,
                VideoUrl = cs.VideoUrl,
                ThumbnailUrl = cs.ThumbnailUrl,
                DurationMinutes = cs.DurationMinutes,
                SortOrder = cs.SortOrder
            };

            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult<CourseSectionDto>> PostCourseSection([FromBody] CourseSectionCreateDto dto)
        {
            var cs = new CourseSection
            {
                CourseId = dto.CourseId,
                Title = dto.Title,
                ContentHtml = dto.ContentHtml,
                VideoUrl = dto.VideoUrl,
                ThumbnailUrl = dto.ThumbnailUrl,
                DurationMinutes = dto.DurationMinutes,
                SortOrder = dto.SortOrder
            };

            _dbContext.CourseSections.Add(cs);
            await _dbContext.SaveChangesAsync();

            var result = new CourseSectionDto
            {
                Id = cs.Id,
                CourseId = cs.CourseId,
                Title = cs.Title,
                ContentHtml = cs.ContentHtml,
                VideoUrl = cs.VideoUrl,
                ThumbnailUrl = cs.ThumbnailUrl,
                DurationMinutes = cs.DurationMinutes,
                SortOrder = cs.SortOrder
            };

            return CreatedAtAction(nameof(GetCourseSection), new { id = cs.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCourseSection(int id, [FromBody] CourseSectionCreateDto dto)
        {
            var cs = await _dbContext.CourseSections.FindAsync(id);
            if (cs == null) return NotFound();

            cs.CourseId = dto.CourseId;
            cs.Title = dto.Title;
            cs.ContentHtml = dto.ContentHtml;
            cs.VideoUrl = dto.VideoUrl;
            cs.ThumbnailUrl = dto.ThumbnailUrl;
            cs.DurationMinutes = dto.DurationMinutes;
            cs.SortOrder = dto.SortOrder;

            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourseSection(int id)
        {
            var cs = await _dbContext.CourseSections.FindAsync(id);
            if (cs == null) return NotFound();

            _dbContext.CourseSections.Remove(cs);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}
