using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DeWaveFreeAPI.Models;
using DeWaveFreeAPI.Data;

public class CourseDetailDto
{
    public int Id { get; set; }
    public int? CourseId { get; set; }
    public string? ShortDescription { get; set; }
    public string? FullDescriptionHtml { get; set; }
    public string? ToolsRequired { get; set; }
    public string? HeroImage { get; set; }
}

public class CourseDetailCreateDto
{
    public int Id { get; set; }
    public int? CourseId { get; set; }
    public string? ShortDescription { get; set; }
    public string? FullDescriptionHtml { get; set; }
    public string? ToolsRequired { get; set; }
    public string? HeroImage { get; set; }
}

namespace DeWaveFreeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseDetailsController : ControllerBase
    {
        private readonly DeWaveAPIDbContext _dbContext;

        public CourseDetailsController(DeWaveAPIDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourseDetailDto>>> GetCourseDetails()
        {
            var details = await _dbContext.CourseDetails
                .Select(cd => new CourseDetailDto
                {
                    Id = cd.Id,
                    CourseId = cd.CourseId,
                    ShortDescription = cd.ShortDescription,
                    FullDescriptionHtml = cd.FullDescriptionHtml,
                    ToolsRequired = cd.ToolsRequired,
                    HeroImage = cd.HeroImage
                })
                .ToListAsync();

            return Ok(details);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CourseDetailDto>> GetCourseDetail(int id)
        {
            var cd = await _dbContext.CourseDetails.FindAsync(id);

            if (cd == null) return NotFound();

            var dto = new CourseDetailDto
            {
                Id = cd.Id,
                CourseId = cd.CourseId,
                ShortDescription = cd.ShortDescription,
                FullDescriptionHtml = cd.FullDescriptionHtml,
                ToolsRequired = cd.ToolsRequired,
                HeroImage = cd.HeroImage
            };

            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult<CourseDetailDto>> PostCourseDetail([FromBody] CourseDetailCreateDto dto)
        {
            // Optionally check if the Id already exists to prevent conflicts
            if (await _dbContext.CourseDetails.AnyAsync(cd => cd.Id == dto.Id))
            {
                return Conflict($"CourseDetail with Id {dto.Id} already exists.");
            }

            var cd = new CourseDetail
            {
                Id = dto.Id,  // assign explicitly
                CourseId = dto.CourseId,
                ShortDescription = dto.ShortDescription,
                FullDescriptionHtml = dto.FullDescriptionHtml,
                ToolsRequired = dto.ToolsRequired,
                HeroImage = dto.HeroImage
            };

            _dbContext.CourseDetails.Add(cd);
            await _dbContext.SaveChangesAsync();

            var result = new CourseDetailDto
            {
                Id = cd.Id,
                CourseId = cd.CourseId,
                ShortDescription = cd.ShortDescription,
                FullDescriptionHtml = cd.FullDescriptionHtml,
                ToolsRequired = cd.ToolsRequired,
                HeroImage = dto.HeroImage
            };

            return CreatedAtAction(nameof(GetCourseDetail), new { id = cd.Id }, result);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> PutCourseDetail(int id, [FromBody] CourseDetailCreateDto dto)
        {
            var cd = await _dbContext.CourseDetails.FindAsync(id);
            if (cd == null) return NotFound();

            cd.CourseId = dto.CourseId;
            cd.ShortDescription = dto.ShortDescription;
            cd.FullDescriptionHtml = dto.FullDescriptionHtml;
            cd.ToolsRequired = dto.ToolsRequired;
            cd.HeroImage = dto.HeroImage;

            await _dbContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourseDetail(int id)
        {
            var cd = await _dbContext.CourseDetails.FindAsync(id);
            if (cd == null) return NotFound();

            _dbContext.CourseDetails.Remove(cd);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}
