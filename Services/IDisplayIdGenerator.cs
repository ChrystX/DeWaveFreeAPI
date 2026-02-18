using Microsoft.EntityFrameworkCore;
using DeWaveFreeAPI.Data;
using DeWaveFreeAPI.Models;

namespace DeWaveFreeAPI.Services
{
    public interface IDisplayIdGenerator
    {
        Task<string> GenerateAsync(string roleName);
    }

    public class DisplayIdGenerator : IDisplayIdGenerator
    {
        private readonly DeWaveAPIDbContext _context;
        private static readonly Dictionary<string, string> RolePrefixes = new()
    {
        { "student", "ST" },
        { "instructor", "IN" },
        { "admin", "AD" }
    };

        public DisplayIdGenerator(DeWaveAPIDbContext context)
        {
            _context = context;
        }

        public async Task<string> GenerateAsync(string roleName)
        {
            var now = DateTime.UtcNow;
            var month = (byte)now.Month;
            var year = (short)now.Year;
            var prefix = RolePrefixes.GetValueOrDefault(roleName, "US");

            // Get or create sequence
            var sequence = await _context.UserSequences
                .FirstOrDefaultAsync(s => s.Role == roleName && s.Month == month && s.Year == year);

            if (sequence == null)
            {
                sequence = new UserSequence
                {
                    Role = roleName,
                    RolePrefix = prefix,
                    Month = month,
                    Year = year,
                    LastSequence = 0,
                    CreatedAt = DateTime.UtcNow
                };
                _context.UserSequences.Add(sequence);
            }

            sequence.LastSequence = (sequence.LastSequence ?? 0) + 1;
            await _context.SaveChangesAsync();

            return $"{prefix}{month:D2}{year % 100:D2}{sequence.LastSequence:D4}";
        }
    }
}
