using DeWaveFreeAPI.Data;
using DeWaveFreeAPI.DTOs.Blocks;
using DeWaveFreeAPI.DTOs.ContentObjects;
using DeWaveFreeAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace DeWaveFreeAPI.Controllers
{
    [ApiController]
    [Route("api/lessons/{lessonId}/blocks")]
    public class LessonBlocksController : ControllerBase
    {
        private readonly DeWaveAPIDbContext _dbContext;
        public LessonBlocksController(DeWaveAPIDbContext context)
        {
            _dbContext = context;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<LessonBlockDto>>> GetBlocks(int lessonId)
        {
            var lesson = await _dbContext.Lessons.FirstOrDefaultAsync(l => l.Id == lessonId);
            if (lesson == null) return NotFound();

            var effectiveLessonId = lesson.SourceLessonId ?? lessonId;

            var blocks = await _dbContext.LessonBlocks
                .Where(b => b.LessonId == effectiveLessonId)
                .OrderBy(b => b.OrderIndex)
                .Select(b => new LessonBlockDto
                {
                    Id = b.Id,
                    LessonId = b.LessonId,
                    BlockTypeId = b.BlockTypeId,
                    BlockTypeName = b.BlockType.Name,
                    OrderIndex = b.OrderIndex,
                    ContentObjectId = b.ContentObjectId,
                    ForkedFromContentObjectId = b.ForkedFromContentObjectId,
                    IsComposite = b.ContentObjectId != null && b.ContentObject!.BlockType.IsComposite,
                    DataJson = b.ContentObjectId != null ? b.ContentObject!.DataJson : b.DataJson
                })
                .ToListAsync();

            var compositeBlocks = blocks.Where(b => b.IsComposite && b.DataJson != null).ToList();
            if (compositeBlocks.Count > 0)
            {
                var parsed = new Dictionary<int, List<int>>();
                foreach (var b in compositeBlocks)
                    parsed[b.Id] = ExtractChildIds(b.DataJson) ?? new List<int>();

                var allChildIds = parsed.Values.SelectMany(x => x).ToHashSet();

                var children = await _dbContext.ContentObjects
                    .Include(c => c.BlockType)
                    .Where(c => allChildIds.Contains(c.Id))
                    .ToListAsync();

                var childLookup = children.ToDictionary(c => c.Id);

                foreach (var b in compositeBlocks)
                {
                    b.Children = parsed[b.Id]
                        .Where(childLookup.ContainsKey)
                        .Select(id => new ContentObjectDto
                        {
                            Id = childLookup[id].Id,
                            Title = childLookup[id].Title,
                            BlockTypeId = childLookup[id].BlockTypeId,
                            BlockTypeName = childLookup[id].BlockType.Name,
                            DataJson = childLookup[id].DataJson,
                            Version = childLookup[id].Version,
                            IsDraft = childLookup[id].IsDraft
                        })
                        .ToList();
                }
            }

            bool canSeeAnswers = User.IsInRole("Instructor") || User.IsInRole("Admin");

            if (!canSeeAnswers)
            {
                foreach (var block in blocks)
                {
                    if ((block.BlockTypeId == 7 || block.BlockTypeId == 4) && block.DataJson != null)
                    {
                        var json = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(block.DataJson);
                        json?.Remove("correct_answer");
                        json?.Remove("correct_answers");
                        block.DataJson = JsonSerializer.Serialize(json);
                    }

                    if (block.Children != null)
                    {
                        foreach (var child in block.Children.Where(c => c.BlockTypeId == 7 || c.BlockTypeId == 4))
                        {
                            if (child.DataJson == null) continue;
                            var json = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(child.DataJson);
                            json?.Remove("correct_answer");
                            json?.Remove("correct_answers");
                            child.DataJson = JsonSerializer.Serialize(json);
                        }
                    }
                }
            }

            return Ok(blocks);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> CreateBlock(int lessonId, CreateBlockDto dto)
        {
            Console.WriteLine($"blockTypeId={dto.BlockTypeId} contentObjectId={dto.ContentObjectId} dataJson={dto.DataJson}");
            // if linking an RLO, verify it exists and matches block type
            if (dto.ContentObjectId.HasValue)
            {
                var rlo = await _dbContext.ContentObjects
                    .Include(c => c.BlockType)
                    .FirstOrDefaultAsync(c => c.Id == dto.ContentObjectId.Value);

                if (rlo == null)
                    return NotFound("Content object not found");

                if (!rlo.BlockType.IsComposite && rlo.BlockTypeId != dto.BlockTypeId) // or block.BlockTypeId in UpdateBlock
                    return BadRequest("Content object block type does not match block type");
            }

            var block = new LessonBlock
            {
                LessonId = lessonId,
                BlockTypeId = dto.BlockTypeId,
                OrderIndex = dto.OrderIndex,
                DataJson = dto.ContentObjectId.HasValue ? null : dto.DataJson,  // don't store inline if RLO linked
                ContentObjectId = dto.ContentObjectId
            };

            _dbContext.LessonBlocks.Add(block);
            await _dbContext.SaveChangesAsync();
            return Ok(new LessonBlockDto
            {
                Id = block.Id,
                LessonId = block.LessonId,
                BlockTypeId = block.BlockTypeId,
                OrderIndex = block.OrderIndex,
                DataJson = block.DataJson,
                ContentObjectId = block.ContentObjectId
            });
        }

        [HttpPut("{blockId}")]
        [Authorize]
        public async Task<IActionResult> UpdateBlock(int lessonId, int blockId, UpdateBlockDto dto)
        {
            var block = await _dbContext.LessonBlocks
                .FirstOrDefaultAsync(b => b.Id == blockId && b.LessonId == lessonId);

            if (block == null)
                return NotFound();

            if (dto.ContentObjectId.HasValue)
            {
                var rlo = await _dbContext.ContentObjects
                    .Include(c => c.BlockType)
                    .FirstOrDefaultAsync(c => c.Id == dto.ContentObjectId.Value);

                if (rlo == null)
                    return NotFound("Content object not found");

                if (!rlo.BlockType.IsComposite && rlo.BlockTypeId != block.BlockTypeId)
                    return BadRequest("Content object block type does not match block type");

                block.ContentObjectId = dto.ContentObjectId;
                block.DataJson = null;
            }

            else
            {
                block.ForkedFromContentObjectId ??= block.ContentObjectId;
                block.ContentObjectId = null;
                block.DataJson = dto.DataJson;
            }

            block.OrderIndex = dto.OrderIndex;
            await _dbContext.SaveChangesAsync();
            return NoContent();
        }

        // POST /api/lessons/{lessonId}/blocks/{blockId}/update-to-latest
        [HttpPost("{blockId}/update-to-latest")]
        [Authorize]
        public async Task<IActionResult> UpdateToLatest(int lessonId, int blockId)
        {
            var block = await _dbContext.LessonBlocks
                .FirstOrDefaultAsync(b => b.Id == blockId && b.LessonId == lessonId);

            if (block?.ContentObjectId == null)
                return NotFound("Block is not linked to a content object");

            var current = await _dbContext.ContentObjects.FindAsync(block.ContentObjectId);
            if (current == null)
                return NotFound("Content object not found");

            var rootId = current.ParentId ?? current.Id;

            var latest = await _dbContext.ContentObjects
                .Where(c => !c.IsDraft && (c.Id == rootId || c.ParentId == rootId))
                .OrderByDescending(c => c.Version)
                .FirstOrDefaultAsync();

            if (latest == null)
                return NotFound("No published version found");

            block.ContentObjectId = latest.Id;
            await _dbContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{blockId}")]
        [Authorize]
        public async Task<IActionResult> DeleteBlock(int lessonId, int blockId)
        {
            var block = await _dbContext.LessonBlocks
                .FirstOrDefaultAsync(b => b.Id == blockId && b.LessonId == lessonId);

            if (block == null)
                return NotFound();

            _dbContext.LessonBlocks.Remove(block);
            await _dbContext.SaveChangesAsync();
            return NoContent();
        }

        // POST /api/lessons/{lessonId}/blocks/group
        [HttpPost("group")]
        [Authorize]
        public async Task<ActionResult<LessonBlockDto>> GroupBlocks(int lessonId, GroupBlocksDto dto)
        {
            if (dto.BlockIds == null || dto.BlockIds.Count < 2)
                return BadRequest("Select at least two blocks to group");

            var distinctBlockIds = dto.BlockIds.Distinct().ToList();

            var blocks = await _dbContext.LessonBlocks
                .Where(b => b.LessonId == lessonId && distinctBlockIds.Contains(b.Id))
                .OrderBy(b => b.OrderIndex)
                .ToListAsync();

            if (blocks.Count != distinctBlockIds.Count)
                return BadRequest("One or more blocks not found in this lesson");

            var groupType = await _dbContext.BlockTypes.FirstOrDefaultAsync(b => b.IsComposite);
            if (groupType == null)
                return StatusCode(500, "No composite block type configured");

            var childContentObjectIds = new List<int>();

            foreach (var block in blocks)
            {
                if (block.ContentObjectId.HasValue)
                {
                    childContentObjectIds.Add(block.ContentObjectId.Value);
                }
                else
                {
                    var promoted = new ContentObject
                    {
                        Title = $"{dto.Title} — part {childContentObjectIds.Count + 1}",
                        BlockTypeId = block.BlockTypeId,
                        DataJson = block.DataJson,
                        Version = 1,
                        CreatedAt = DateTime.UtcNow
                    };
                    _dbContext.ContentObjects.Add(promoted);
                    await _dbContext.SaveChangesAsync();
                    childContentObjectIds.Add(promoted.Id);
                }
            }

            var composite = new ContentObject
            {
                Title = dto.Title,
                BlockTypeId = groupType.Id,
                DataJson = JsonSerializer.Serialize(new { childContentObjectIds }),
                Version = 1,
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.ContentObjects.Add(composite);
            await _dbContext.SaveChangesAsync();

            var first = blocks[0];
            first.ContentObjectId = composite.Id;
            first.DataJson = null;

            _dbContext.LessonBlocks.RemoveRange(blocks.Skip(1));
            await _dbContext.SaveChangesAsync();

            return Ok(new LessonBlockDto
            {
                Id = first.Id,
                LessonId = first.LessonId,
                BlockTypeId = first.BlockTypeId,
                OrderIndex = first.OrderIndex,
                ContentObjectId = composite.Id,
                IsComposite = true
            });
        }

        private static List<int>? ExtractChildIds(string? dataJson)
        {
            if (string.IsNullOrWhiteSpace(dataJson))
                return null;

            try
            {
                using var doc = JsonDocument.Parse(dataJson);
                if (doc.RootElement.TryGetProperty("childContentObjectIds", out var arr))
                    return arr.EnumerateArray().Select(e => e.GetInt32()).ToList();
            }
            catch (JsonException) { }

            return null;
        }
    }
}