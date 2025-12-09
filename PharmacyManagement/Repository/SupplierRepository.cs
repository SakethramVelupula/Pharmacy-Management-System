using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Data;
using PharmacyManagement.Interface;
using PharmacyManagement.Models;

namespace PharmacyManagement.Repository
{
    public class SupplierRepository : ISupplierRepository
    {
        private readonly ApplicationDbContext _context;

        public SupplierRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Supplier>> GetAllAsync()
        {
            return await _context.Suppliers.ToListAsync();
        }

        public async Task<Supplier?> GetByIdAsync(int id)
        {
            return await _context.Suppliers.FindAsync(id);
        }

        public async Task<Supplier> AddAsync(Supplier supplier)
        {
            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();
            return supplier;
        }

        public async Task<Supplier> UpdateAsync(Supplier supplier)
        {
            _context.Suppliers.Update(supplier);
            await _context.SaveChangesAsync();
            return supplier;
        }

        public async Task<Supplier> EnsureDefaultSupplierAsync()
        {
            var defaultSupplier = await _context.Suppliers.FirstOrDefaultAsync();
            if (defaultSupplier == null)
            {
                defaultSupplier = new Supplier
                {
                    Name = "Default Supplier",
                    Email = "default@pharmacy.com",
                    PhoneNumber = "000-000-0000"
                };
                _context.Suppliers.Add(defaultSupplier);
                await _context.SaveChangesAsync();
            }
            return defaultSupplier;
        }
    }
}