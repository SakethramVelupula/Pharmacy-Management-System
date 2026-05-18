using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using PharmacyManagement.Models;
using System;
using System.Threading.Tasks;

namespace PharmacyManagement.Seeder
{
    public static class DbSeeder
    {
        public static async Task SeedAdminAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            var adminEmail = configuration["Admin:Email"];
            var adminPassword = configuration["Admin:Password"];
            var adminUsername = configuration["Admin:Username"];

            if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
            {
                Console.WriteLine("Admin credentials not configured in environment variables.");
                return;
            }

            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);

            if (existingAdmin == null)
            {
                var admin = new User
                {
                    UserName = adminUsername ?? "Admin",
                    Email = adminEmail,
                    Role = "Admin"
                };

                var result = await userManager.CreateAsync(admin, adminPassword);
                if (result.Succeeded)
                    Console.WriteLine("Admin user seeded successfully.");
                else
                    Console.WriteLine("Failed to seed admin user: " + string.Join(", ", result.Errors));
            }
        }
    }
}