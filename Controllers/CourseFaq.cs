using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DeWaveFreeAPI.Models;
using DeWaveFreeAPI.Data;

namespace DeWaveFreeAPI.Controllers
{
    public class CourseFaqDto
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string Question { get; set; } = null!;
        public string? Answer { get; set; }
        public int? SortOrder { get; set; }
    }

    public class CourseFaqCreateDto
    {
        public int CourseId { get; set; }
        public string Question { get; set; } = null!;
        public string? Answer { get; set; }
        public int? SortOrder { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class CourseFaqsController : ControllerBase
    {
        private readonly DeWaveAPIDbContext _dbContext;

        public CourseFaqsController(DeWaveAPIDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourseFaqDto>>> GetCourseFaqs()
        {
            var faqs = await _dbContext.CourseFaqs
                .Select(faq => new CourseFaqDto
                {
                    Id = faq.Id,
                    CourseId = faq.CourseId,
                    Question = faq.Question,
                    Answer = faq.Answer,
                    SortOrder = faq.SortOrder
                })
                .ToListAsync();

            return Ok(faqs);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CourseFaqDto>> GetCourseFaq(int id)
        {
            var faq = await _dbContext.CourseFaqs.FindAsync(id);
            if (faq == null) return NotFound();

            var dto = new CourseFaqDto
            {
                Id = faq.Id,
                CourseId = faq.CourseId,
                Question = faq.Question,
                Answer = faq.Answer,
                SortOrder = faq.SortOrder
            };

            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult<CourseFaqDto>> PostCourseFaq([FromBody] CourseFaqCreateDto dto)
        {
            var faq = new CourseFaq
            {
                CourseId = dto.CourseId,
                Question = dto.Question,
                Answer = dto.Answer,
                SortOrder = dto.SortOrder
            };

            _dbContext.CourseFaqs.Add(faq);
            await _dbContext.SaveChangesAsync();

            var result = new CourseFaqDto
            {
                Id = faq.Id,
                CourseId = faq.CourseId,
                Question = faq.Question,
                Answer = faq.Answer,
                SortOrder = faq.SortOrder
            };

            return CreatedAtAction(nameof(GetCourseFaq), new { id = faq.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCourseFaq(int id, [FromBody] CourseFaqCreateDto dto)
        {
            var faq = await _dbContext.CourseFaqs.FindAsync(id);
            if (faq == null) return NotFound();

            faq.CourseId = dto.CourseId;
            faq.Question = dto.Question;
            faq.Answer = dto.Answer;
            faq.SortOrder = dto.SortOrder;

            await _dbContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourseFaq(int id)
        {
            var faq = await _dbContext.CourseFaqs.FindAsync(id);
            if (faq == null) return NotFound();

            _dbContext.CourseFaqs.Remove(faq);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}
