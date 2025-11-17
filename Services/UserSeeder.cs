using DeWaveFreeAPI.Data;
using DeWaveFreeAPI.Models;

namespace DeWaveFreeAPI.Services
{
    public class UserSeeder
    {
        public static void SeedAdminUser(DeWaveAPIDbContext context)
        {
            // Check if admin user already exists
            if (!context.Users.Any(u => u.Role == "Admin"))
            {
                var adminUser = new User
                {
                    Username = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    Role = "Admin"
                };

                context.Users.Add(adminUser);
                context.SaveChanges();

                Console.WriteLine("Admin user created with username: admin, password: admin123");
                Console.WriteLine("PLEASE CHANGE THE DEFAULT PASSWORD!");
            }

            if (!context.Users.Any(u => u.Username == "teknik"))
            {
                var teknikUser = new User
                {
                    Username = "teknik",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("teknik"),
                    Role = "Admin"
                };

                context.Users.Add(teknikUser);
                context.SaveChanges();

                Console.WriteLine("Teknik admin created with username: teknik, password: teknik");
            }
        }
    }
}