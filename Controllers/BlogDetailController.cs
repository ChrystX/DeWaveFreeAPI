using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DeWaveFreeAPI.Data;
using DeWaveFreeAPI.Models;

namespace DeWaveFreeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogDetailsController : ControllerBase
    {
        private readonly DeWaveAPIDbContext _context;

        public BlogDetailsController(DeWaveAPIDbContext context)
        {
            _context = context;
        }

        // DTO
        public class BlogDetailDto
        {
            public int BlogId { get; set; }
            public string Content { get; set; } = null!;
            public string? SeoTitle { get; set; }
            public string? SeoDescription { get; set; }
            public string? SeoKeywords { get; set; }
            public string? Tags { get; set; }
            public string? ExtraJson { get; set; }
        }

        // GET: api/BlogDetails
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BlogDetailDto>>> GetBlogDetails()
        {
            return await _context.BlogDetails
                .Select(bd => new BlogDetailDto
                {
                    BlogId = bd.BlogId,
                    Content = bd.Content,
                    SeoTitle = bd.SeoTitle,
                    SeoDescription = bd.SeoDescription,
                    SeoKeywords = bd.SeoKeywords,
                    Tags = bd.Tags,
                    ExtraJson = bd.ExtraJson
                })
                .ToListAsync();
        }

        // GET: api/BlogDetails/5
        [HttpGet("{blogId:int}")]
        public async Task<ActionResult<BlogDetailDto>> GetBlogDetail(int blogId)
        {
            var bd = await _context.BlogDetails.FindAsync(blogId);
            if (bd == null) return NotFound();

            return new BlogDetailDto
            {
                BlogId = bd.BlogId,
                Content = bd.Content,
                SeoTitle = bd.SeoTitle,
                SeoDescription = bd.SeoDescription,
                SeoKeywords = bd.SeoKeywords,
                Tags = bd.Tags,
                ExtraJson = bd.ExtraJson
            };
        }

        // POST: api/BlogDetails
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<BlogDetailDto>> CreateBlogDetail(BlogDetailDto dto)
        {
            var bd = new BlogDetail
            {
                BlogId = dto.BlogId,
                Content = dto.Content,
                SeoTitle = dto.SeoTitle,
                SeoDescription = dto.SeoDescription,
                SeoKeywords = dto.SeoKeywords,
                Tags = dto.Tags,
                ExtraJson = dto.ExtraJson
            };

            _context.BlogDetails.Add(bd);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBlogDetail), new { blogId = bd.BlogId }, dto);
        }

        // PUT: api/BlogDetails/5
        [HttpPut("{blogId:int}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateBlogDetail(int blogId, BlogDetailDto dto)
        {
            if (blogId != dto.BlogId) return BadRequest();

            var bd = await _context.BlogDetails.FindAsync(blogId);
            if (bd == null) return NotFound();

            bd.Content = dto.Content;
            bd.SeoTitle = dto.SeoTitle;
            bd.SeoDescription = dto.SeoDescription;
            bd.SeoKeywords = dto.SeoKeywords;
            bd.Tags = dto.Tags;
            bd.ExtraJson = dto.ExtraJson;

            _context.Entry(bd).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/BlogDetails/5
        [HttpDelete("{blogId:int}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteBlogDetail(int blogId)
        {
            var bd = await _context.BlogDetails.FindAsync(blogId);
            if (bd == null) return NotFound();

            _context.BlogDetails.Remove(bd);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
