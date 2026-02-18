using DeWaveFreeAPI.Data;
using DeWaveFreeAPI.DTOs.ContentObjects;
using DeWaveFreeAPI.DTOs.ExternalContent;
using DeWaveFreeAPI.Models;
using DeWaveFreeAPI.Services.ExternalContent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace DeWaveFreeAPI.Controllers
{
    [ApiController]
    [Route("api/content-objects")]
    public class ContentObjectsController : ControllerBase
    {
        private readonly DeWaveAPIDbContext _dbContext;
        public ContentObjectsController(DeWaveAPIDbContext context)
        {
            _dbContext = context;
        }

        // Search RLOs — filter by block type, optional keyword
        // GET /api/content-objects?blockTypeId=2&q=wave
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ContentObjectDto>>> Search(
            [FromQuery] int? blockTypeId,
            [FromQuery] string? q)
        {
            var published = _dbContext.ContentObjects
                .Include(c => c.BlockType)
                .Where(c => !c.IsDraft);

            if (blockTypeId.HasValue)
                published = published.Where(c => c.BlockTypeId == blockTypeId.Value);

            if (!string.IsNullOrWhiteSpace(q))
                published = published.Where(c => c.Title.Contains(q));

            var all = await published.ToListAsync();

            var latestPerRoot = all
                .GroupBy(c => c.ParentId ?? c.Id)
                .Select(g => g.OrderByDescending(c => c.Version).First())
                .ToList();

            var results = latestPerRoot.Select(c => new ContentObjectDto
            {
                Id = c.Id,
                Title = c.Title,
                BlockTypeId = c.BlockTypeId,
                BlockTypeName = c.BlockType.Name,
                DataJson = c.DataJson,
                Version = c.Version,
                IsDraft = c.IsDraft,
                CreatedAt = c.CreatedAt
            }).ToList();

            return Ok(results);
        }

        // Create a new RLO from scratch
        // POST /api/content-objects
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ContentObjectDto>> Create(CreateContentObjectDto dto)
        {
            var blockTypeExists = await _dbContext.BlockTypes
                .AnyAsync(b => b.Id == dto.BlockTypeId);

            if (!blockTypeExists)
                return NotFound("Block type not found");

            var contentObject = new ContentObject
            {
                Title = dto.Title,
                BlockTypeId = dto.BlockTypeId,
                DataJson = dto.DataJson,
                Version = 1,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.ContentObjects.Add(contentObject);
            await _dbContext.SaveChangesAsync();

            return Ok(new ContentObjectDto
            {
                Id = contentObject.Id,
                Title = contentObject.Title,
                BlockTypeId = contentObject.BlockTypeId,
                DataJson = contentObject.DataJson,
                Version = contentObject.Version,
                CreatedAt = contentObject.CreatedAt
            });
        }

        // GET /api/content-objects/external-search?source=youtube&q=skincare
        [HttpGet("external-search")]
        [Authorize]
        public async Task<ActionResult<List<ExternalContentSearchResult>>> ExternalSearch(
            [FromQuery] string source,
            [FromQuery] string q,
            [FromServices] ExternalContentSearchFactory factory,
            [FromQuery] int limit = 10)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest("Query is required");

            var service = factory.Get(source);
            if (service == null)
                return BadRequest($"Unknown source '{source}'. Available: {string.Join(", ", factory.AvailableSources)}");

            var results = await service.SearchAsync(q, limit);
            return Ok(results);
        }

        // PUT /api/content-objects/{id}
        [HttpPatch("{id}/draft")]
        [Authorize]
        public async Task<ActionResult<ContentObjectDto>> SaveDraft(int id, UpdateContentObjectDto dto)
        {
            var existing = await _dbContext.ContentObjects.FirstOrDefaultAsync(c => c.Id == id);
            if (existing == null)
                return NotFound("Content object not found");

            var rootId = existing.ParentId ?? existing.Id;

            var draft = await _dbContext.ContentObjects
                .FirstOrDefaultAsync(c => (c.Id == rootId || c.ParentId == rootId) && c.IsDraft);

            if (draft == null)
            {
                var latestPublished = await _dbContext.ContentObjects
                    .Where(c => (c.Id == rootId || c.ParentId == rootId) && !c.IsDraft)
                    .OrderByDescending(c => c.Version)
                    .FirstOrDefaultAsync();

                if (latestPublished == null)
                    return BadRequest("No published version exists to draft from");

                draft = new ContentObject
                {
                    ParentId = rootId,
                    Version = 0,
                    IsDraft = true,
                    Title = latestPublished.Title,
                    BlockTypeId = latestPublished.BlockTypeId,
                    DataJson = latestPublished.DataJson,
                    CreatedAt = DateTime.UtcNow
                };
                _dbContext.ContentObjects.Add(draft);
            }

            draft.DataJson = dto.DataJson ?? draft.DataJson;
            await _dbContext.SaveChangesAsync();

            return Ok(new ContentObjectDto
            {
                Id = draft.Id,
                Title = draft.Title,
                BlockTypeId = draft.BlockTypeId,
                DataJson = draft.DataJson,
                Version = draft.Version,
                IsDraft = draft.IsDraft,
                CreatedAt = draft.CreatedAt,
                ParentId = draft.ParentId
            });
        }

        // POST /api/content-objects/{id}/publish
        // The ONLY place a new version row is created. Explicit action only.
        [HttpPost("{id}/publish")]
        [Authorize]
        public async Task<ActionResult<ContentObjectDto>> Publish(int id)
        {
            var existing = await _dbContext.ContentObjects.FirstOrDefaultAsync(c => c.Id == id);
            if (existing == null)
                return NotFound("Content object not found");

            var rootId = existing.ParentId ?? existing.Id;

            var draft = await _dbContext.ContentObjects
                .FirstOrDefaultAsync(c => (c.Id == rootId || c.ParentId == rootId) && c.IsDraft);

            if (draft == null)
                return BadRequest("No draft to publish");

            var maxVersion = await _dbContext.ContentObjects
                .Where(c => c.Id == rootId || c.ParentId == rootId)
                .MaxAsync(c => (int?)c.Version) ?? 0;

            var published = new ContentObject
            {
                ParentId = rootId,
                Version = maxVersion + 1,
                IsDraft = false,
                Title = draft.Title,
                BlockTypeId = draft.BlockTypeId,
                DataJson = draft.DataJson,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.ContentObjects.Add(published);
            // draft row stays as-is — becomes the base for the next round of edits
            await _dbContext.SaveChangesAsync();

            return Ok(new ContentObjectDto
            {
                Id = published.Id,
                Title = published.Title,
                BlockTypeId = published.BlockTypeId,
                DataJson = published.DataJson,
                Version = published.Version,
                IsDraft = published.IsDraft,
                CreatedAt = published.CreatedAt,
                ParentId = published.ParentId
            });
        }

        // GET /api/content-objects/{id}/versions
        [HttpGet("{id}/versions")]
        [Authorize]
        public async Task<ActionResult<List<ContentObjectDto>>> GetVersions(int id)
        {
            var root = await _dbContext.ContentObjects
                .FirstOrDefaultAsync(c => c.Id == id);

            if (root == null)
                return NotFound();

            var rootId = root.ParentId ?? root.Id;

            var versions = await _dbContext.ContentObjects
                .Where(c => c.Id == rootId || c.ParentId == rootId)
                .OrderBy(c => c.Version)
                .Select(c => new ContentObjectDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    BlockTypeId = c.BlockTypeId,
                    DataJson = c.DataJson,
                    Version = c.Version,
                    IsDraft = c.IsDraft,
                    CreatedAt = c.CreatedAt,
                    ParentId = c.ParentId
                })
                .ToListAsync();

            return Ok(versions);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id, [FromQuery] bool force = false)
        {
            var contentObject = await _dbContext.ContentObjects
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contentObject == null)
                return NotFound();

            var rootId = contentObject.ParentId ?? contentObject.Id;

            if (force)
            {
              
                var allVersionIds = await _dbContext.ContentObjects
                    .Where(c => c.Id == rootId || c.ParentId == rootId)
                    .Select(c => c.Id)
                    .ToListAsync();

                var linkedBlocks = await _dbContext.LessonBlocks
                    .Where(b => b.ContentObjectId.HasValue && allVersionIds.Contains(b.ContentObjectId.Value))
                    .ToListAsync();

                foreach (var block in linkedBlocks)
                {
                    var version = await _dbContext.ContentObjects
                        .FirstOrDefaultAsync(c => c.Id == block.ContentObjectId);
                    block.DataJson = version?.DataJson;
                    block.ContentObjectId = null;
                }

                var versions = await _dbContext.ContentObjects
                    .Where(c => c.ParentId == rootId)
                    .ToListAsync();

                _dbContext.ContentObjects.RemoveRange(versions);

                var root = await _dbContext.ContentObjects
                    .FirstOrDefaultAsync(c => c.Id == rootId);

                if (root != null)
                    _dbContext.ContentObjects.Remove(root);

                await _dbContext.SaveChangesAsync();
                return NoContent();
            }

            var hasLinkedBlocks = await _dbContext.LessonBlocks
                .AnyAsync(b => b.ContentObjectId == id);

            if (hasLinkedBlocks)
                return BadRequest("Cannot delete — still linked to lesson blocks. Use ?force=true to force.");

            var hasVersions = await _dbContext.ContentObjects
                .AnyAsync(c => c.ParentId == rootId && c.Id != contentObject.Id);

            if (hasVersions)
                return BadRequest("Cannot delete — has versions. Use ?force=true to delete all versions.");

            _dbContext.ContentObjects.Remove(contentObject);
            await _dbContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("group")]
        [Authorize]
        public async Task<ActionResult<ContentObjectDto>> Group(GroupContentObjectsDto dto)
        {
            if (dto.ChildContentObjectIds == null || dto.ChildContentObjectIds.Count == 0)
                return BadRequest("At least one content object is required");

            var distinctIds = dto.ChildContentObjectIds.Distinct().ToList();

            var groupType = await _dbContext.BlockTypes.FirstOrDefaultAsync(b => b.IsComposite);
            if (groupType == null)
                return StatusCode(500, "No composite block type configured");

            var foundCount = await _dbContext.ContentObjects
                .Where(c => distinctIds.Contains(c.Id))
                .CountAsync();

            if (foundCount != distinctIds.Count)
                return BadRequest("One or more content objects not found");

            var composite = new ContentObject
            {
                Title = dto.Title,
                BlockTypeId = groupType.Id,
                DataJson = JsonSerializer.Serialize(new { childContentObjectIds = distinctIds }),
                Version = 1,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.ContentObjects.Add(composite);
            await _dbContext.SaveChangesAsync();

            return Ok(new ContentObjectDto
            {
                Id = composite.Id,
                Title = composite.Title,
                BlockTypeId = composite.BlockTypeId,
                BlockTypeName = groupType.Name,
                DataJson = composite.DataJson,
                Version = composite.Version,
                CreatedAt = composite.CreatedAt,
                ChildContentObjectIds = distinctIds
            });
        }

        // Promote an existing inline block to an RLO
        // POST /api/content-objects/promote/{blockId}
        [HttpPost("promote/{blockId}")]
        [Authorize]
        public async Task<ActionResult<ContentObjectDto>> Promote(int blockId, PromoteBlockDto dto)
        {
            var block = await _dbContext.LessonBlocks
                .FirstOrDefaultAsync(b => b.Id == blockId);

            if (block == null)
                return NotFound("Block not found");

            if (block.ContentObjectId.HasValue)
                return BadRequest("Block is already linked to an RLO");

            var contentObject = new ContentObject
            {
                Title = dto.Title,
                BlockTypeId = block.BlockTypeId,
                DataJson = block.DataJson,
                Version = 1,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.ContentObjects.Add(contentObject);
            await _dbContext.SaveChangesAsync();

            // link the block to the new RLO and clear inline
            block.ContentObjectId = contentObject.Id;
            block.DataJson = null;
            await _dbContext.SaveChangesAsync();

            return Ok(new ContentObjectDto
            {
                Id = contentObject.Id,
                Title = contentObject.Title,
                BlockTypeId = contentObject.BlockTypeId,
                DataJson = contentObject.DataJson,
                Version = contentObject.Version,
                CreatedAt = contentObject.CreatedAt
            });
        }
    }
}