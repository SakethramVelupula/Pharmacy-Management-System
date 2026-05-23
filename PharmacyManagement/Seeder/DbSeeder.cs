using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
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
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            var adminEmail = configuration["Admin:Email"];
            var adminPassword = configuration["Admin:Password"];
            var adminUsername = configuration["Admin:Username"];

            if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
            {
                Console.WriteLine("Admin credentials not configured in environment variables.");
                return;
            }

            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole("Admin"));

            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);

            if (existingAdmin == null)
            {
                var admin = new User
                {
                    UserName = adminUsername ?? "Admin",
                    Email = adminEmail,
                    Role = "Admin",
                    IsApproved = true,
                    IsActive = true
                };

                var result = await userManager.CreateAsync(admin, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                    Console.WriteLine("Admin user seeded successfully.");
                }
                else
                    Console.WriteLine("Failed to seed admin user: " + string.Join(", ", result.Errors));
            }
            else
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(existingAdmin);
                await userManager.ResetPasswordAsync(existingAdmin, token, adminPassword);

                if (!await userManager.IsInRoleAsync(existingAdmin, "Admin"))
                    await userManager.AddToRoleAsync(existingAdmin, "Admin");

                Console.WriteLine("Admin password synced from configuration.");
            }
        }
    }
}