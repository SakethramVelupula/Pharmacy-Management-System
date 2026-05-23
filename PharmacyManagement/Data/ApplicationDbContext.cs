using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Models;
namespace PharmacyManagement.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<Drug> Drugs { get; set; }
        public DbSet<Inventory> Inventory { get; set; }
        public DbSet<Sales> Sales { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<SpecialOrder> SpecialOrders { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
    }
}