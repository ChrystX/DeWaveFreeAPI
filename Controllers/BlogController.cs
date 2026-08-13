using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DeWaveFreeAPI.Data;
using DeWaveFreeAPI.Models;

namespace DeWaveFreeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogsController : ControllerBase
    {
        private readonly DeWaveAPIDbContext _context;

        public BlogsController(DeWaveAPIDbContext context)
        {
            _context = context;
        }

        // DTO for Blog operations (excluding related entities)
        public class BlogDto
        {
            public int Id { get; set; }
            public string Title { get; set; } = null!;
            public string Slug { get; set; } = null!;
            public int? AuthorId { get; set; }
            public string? Summary { get; set; }
            public string? ThumbnailUrl { get; set; }
            public int? CategoryId { get; set; }
            public string Status { get; set; } = null!;
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
            public DateTime? PublishedAt { get; set; }
            public int ViewCount { get; set; } = 0;

        }

        // GET: api/Blogs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BlogDto>>> GetBlogs([FromQuery] int? limit = null)
        {
            var query = _context.Blogs
                .Where(b => b.Status == "published") // Only published blogs
                .OrderByDescending(b => b.PublishedAt ?? b.CreatedAt) // Sort by published date, fallback to created date
                .Select(b => new BlogDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Slug = b.Slug,
                    AuthorId = b.AuthorId,
                    Summary = b.Summary,
                    ThumbnailUrl = b.ThumbnailUrl,
                    CategoryId = b.CategoryId,
                    Status = b.Status,
                    CreatedAt = b.CreatedAt,
                    UpdatedAt = b.UpdatedAt,
                    PublishedAt = b.PublishedAt,
                    ViewCount = b.ViewCount
                });

            // Apply limit if specified
            if (limit.HasValue && limit.Value > 0)
            {
                query = query.Take(limit.Value);
            }

            return await query.ToListAsync();
        }

        // GET: api/Blogs/All
        [HttpGet("All")]
        public async Task<ActionResult<IEnumerable<BlogDto>>> GetAllBlogs()
        {
            var blogs = await _context.Blogs
                .OrderByDescending(b => b.Id) // newest first
                .Select(b => new BlogDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Slug = b.Slug,
                    AuthorId = b.AuthorId,
                    Summary = b.Summary,
                    ThumbnailUrl = b.ThumbnailUrl,
                    CategoryId = b.CategoryId,
                    Status = b.Status,
                    CreatedAt = b.CreatedAt,
                    UpdatedAt = b.UpdatedAt,
                    PublishedAt = b.PublishedAt,
                    ViewCount = b.ViewCount
                })
                .ToListAsync();

            return blogs;
        }

        // GET: api/Blogs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BlogDto>> GetBlog(int id)
        {
            var blog = await _context.Blogs
                .Where(b => b.Id == id)
                .Select(b => new BlogDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Slug = b.Slug,
                    AuthorId = b.AuthorId,
                    Summary = b.Summary,
                    ThumbnailUrl = b.ThumbnailUrl,
                    CategoryId = b.CategoryId,
                    Status = b.Status,
                    CreatedAt = b.CreatedAt,
                    UpdatedAt = b.UpdatedAt,
                    PublishedAt = b.PublishedAt,
                    ViewCount = b.ViewCount
                })
                .FirstOrDefaultAsync();

            if (blog == null)
            {
                return NotFound();
            }

            return blog;
        }

        // POST: api/Blogs
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<BlogDto>> CreateBlog(BlogDto blogDto)
        {
            var blog = new Blog
            {
                Title = blogDto.Title,
                Slug = blogDto.Slug,
                AuthorId = blogDto.AuthorId,
                Summary = blogDto.Summary,
                ThumbnailUrl = blogDto.ThumbnailUrl,
                CategoryId = blogDto.CategoryId,
                Status = blogDto.Status,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                PublishedAt = blogDto.PublishedAt,
            };

            _context.Blogs.Add(blog);
            await _context.SaveChangesAsync();

            // Map back to DTO with the generated ID
            blogDto.Id = blog.Id;
            blogDto.CreatedAt = blog.CreatedAt;
            blogDto.UpdatedAt = blog.UpdatedAt;

            return CreatedAtAction(nameof(GetBlog), new { id = blog.Id }, blogDto);
        }

        [HttpPost("{id}/increment-view")]
        public async Task<IActionResult> IncrementViewCount(int id)
        {
            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null)
            {
                return NotFound();
            }

            blog.ViewCount++;
            await _context.SaveChangesAsync();

            return Ok(new { viewCount = blog.ViewCount });
        }

        // PUT: api/Blogs/5
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateBlog(int id, BlogDto blogDto)
        {
            if (id != blogDto.Id)
            {
                return BadRequest();
            }

            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null)
            {
                return NotFound();
            }

            // Update only Blog properties
            blog.Title = blogDto.Title;
            blog.Slug = blogDto.Slug;
            blog.AuthorId = blogDto.AuthorId;
            blog.Summary = blogDto.Summary;
            blog.ThumbnailUrl = blogDto.ThumbnailUrl;
            blog.CategoryId = blogDto.CategoryId;
            blog.Status = blogDto.Status;
            blog.UpdatedAt = DateTime.UtcNow;
            blog.PublishedAt = blogDto.PublishedAt;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BlogExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }



        // DELETE: api/Blogs/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteBlog(int id)
        {
            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null)
            {
                return NotFound();
            }

            _context.Blogs.Remove(blog);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BlogExists(int id)
        {
            return _context.Blogs.Any(e => e.Id == id);
        }
    }
}