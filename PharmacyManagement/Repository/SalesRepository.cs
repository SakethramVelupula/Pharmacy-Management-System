using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PharmacyManagement.Interface;
using PharmacyManagement.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using PharmacyManagement.Data;

namespace PharmacyManagement.Repository
{
    public class SalesRepository : ISalesRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SalesRepository> _logger;

        public SalesRepository(ApplicationDbContext context, ILogger<SalesRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Sales>> GetAllAsync()
        {
            return await _context.Sales.AsNoTracking().ToListAsync();
        }

        public async Task<Sales?> GetByIdAsync(int id)
        {
            return await _context.Sales.AsNoTracking().FirstOrDefaultAsync(s => s.SalesId == id);
        }

        public async Task<Sales> CreateAsync(Sales sale)
        {
            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();
            return sale;
        }

        public async Task<Sales?> UpdateAsync(int id, Sales sale)
        {
            var existing = await _context.Sales.FindAsync(id);
            if (existing == null) return null;

            _context.Entry(existing).CurrentValues.SetValues(sale);
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var sale = await _context.Sales.FindAsync(id);
            if (sale == null) return false;

            _context.Sales.Remove(sale);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}