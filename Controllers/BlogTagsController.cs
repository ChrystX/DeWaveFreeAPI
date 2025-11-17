using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DeWaveFreeAPI.Data;
using DeWaveFreeAPI.Models;

namespace DeWaveFreeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogTagsController : ControllerBase
    {
        private readonly DeWaveAPIDbContext _context;

        public BlogTagsController(DeWaveAPIDbContext context)
        {
            _context = context;
        }

        // DTO
        public class BlogTagDto
        {
            public int BlogId { get; set; }
            public string Tag { get; set; } = null!;
        }

        // GET: api/BlogTags
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BlogTagDto>>> GetBlogTags()
        {
            return await _context.BlogTags
                .Select(bt => new BlogTagDto
                {
                    BlogId = bt.BlogId,
                    Tag = bt.Tag
                })
                .ToListAsync();
        }

        // GET: api/BlogTags/5/sample-tag
        [HttpGet("{blogId:int}/{tag}")]
        public async Task<ActionResult<BlogTagDto>> GetBlogTag(int blogId, string tag)
        {
            var blogTag = await _context.BlogTags.FindAsync(blogId, tag);

            if (blogTag == null)
                return NotFound();

            return new BlogTagDto
            {
                BlogId = blogTag.BlogId,
                Tag = blogTag.Tag
            };
        }

        // POST: api/BlogTags
        [HttpPost]
        public async Task<ActionResult<BlogTagDto>> CreateBlogTag(BlogTagDto dto)
        {
            var blogTag = new BlogTag
            {
                BlogId = dto.BlogId,
                Tag = dto.Tag
            };

            _context.BlogTags.Add(blogTag);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBlogTag), new { blogId = blogTag.BlogId, tag = blogTag.Tag }, dto);
        }

        // PUT: api/BlogTags/5/sample-tag
        [HttpPut("{blogId:int}/{tag}")]
        public async Task<IActionResult> UpdateBlogTag(int blogId, string tag, BlogTagDto dto)
        {
            if (blogId != dto.BlogId || tag != dto.Tag)
                return BadRequest();

            var blogTag = await _context.BlogTags.FindAsync(blogId, tag);
            if (blogTag == null)
                return NotFound();

            // If you only allow updating BlogId or Tag, adjust here
            blogTag.BlogId = dto.BlogId;
            blogTag.Tag = dto.Tag;

            _context.Entry(blogTag).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/BlogTags/5/sample-tag
        [HttpDelete("{blogId:int}/{tag}")]
        public async Task<IActionResult> DeleteBlogTag(int blogId, string tag)
        {
            var blogTag = await _context.BlogTags.FindAsync(blogId, tag);
            if (blogTag == null)
                return NotFound();

            _context.BlogTags.Remove(blogTag);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
