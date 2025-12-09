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
            var adminEmail = "sakethram@gmail.com";
            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);

            if (existingAdmin == null)
            {
                var admin = new User
                {
                    UserName = "SakethRam31",
                    Email = adminEmail,
                    Role = "Admin"
                };

                var result = await userManager.CreateAsync(admin, "Sakethram@03"); // Use a secure password
                if (result.Succeeded)
                {
                    Console.WriteLine("Admin user seeded successfully.");
                }
                else
                {
                    Console.WriteLine("Failed to seed admin user: " + string.Join(", ", result.Errors));
                }
            }
        }
    }
}