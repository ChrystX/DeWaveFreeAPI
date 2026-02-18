using DeWaveFreeAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace DeWaveFreeAPI.Controllers
{
    public class BlockTypesController : ControllerBase
    {
        private readonly DeWaveAPIDbContext _dbContext;

        public BlockTypesController(DeWaveAPIDbContext context)
        {
            _dbContext = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetBlockTypes()
        {
            var types = await _dbContext.BlockTypes
                .OrderBy(t => t.Id)
                .ToListAsync();

            return Ok(types);
        }

    }
}
