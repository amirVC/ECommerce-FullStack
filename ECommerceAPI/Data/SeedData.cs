using ECommerceAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAPI.Data
{
    public static class SeedData
    {
        public static async Task Initialize(AppDbContext context)
        {
            await context.Database.MigrateAsync();

            var adminEmail = "admin@shop.com";

            var adminExists = await context.Users
                .AnyAsync(u => u.Email == adminEmail);

            if (!adminExists)
            {
                var adminUser = new User
                {
                    FullName = "System Admin",
                    Email = adminEmail,
                    Role = "Admin",
                    CreatedAt = DateTime.UtcNow
                };

                adminUser.PasswordHash =
                    BCrypt.Net.BCrypt.HashPassword("Admin123!");

                context.Users.Add(adminUser);
                await context.SaveChangesAsync();
            }
        }
    }
}