//using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Data;
using PharmacyManagement.Interface;
using PharmacyManagement.Models;
namespace PharmacyManagement.Repository
{
    public class DrugRepository : IDrugRepository
    {
        private readonly ApplicationDbContext _context;
        public DrugRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async  Task<IEnumerable<Drug>> GetAllAsync()
        {
            return await _context.Drugs.ToListAsync();
        }
        public async Task<Drug?> GetByIdAsync(int id)
        {
            return await _context.Drugs.FindAsync(id);
        }
        public async Task<Drug?> GetByNameAsync(string name)
        {
            return await _context.Drugs.FirstOrDefaultAsync(d => d.Name.ToLower() == name.ToLower());
        }
        public async Task<Drug> AddAsync(Drug drug)
        {
            _context.Drugs.Add(drug);
            await _context.SaveChangesAsync();
            return drug;
        }
        public async Task<Drug?> UpdateAsync(Drug drug)
        {
            _context.Drugs.Update(drug);
            await _context.SaveChangesAsync();
            return drug;
        }

        public async Task UpdateDrugStockAsync(int drugId)
        {
            var drug = await _context.Drugs.FindAsync(drugId);
            if (drug != null)
            {
                var totalQuantity = await _context.Inventory
                    .Where(i => i.Drug.Name.ToLower() == drug.Name.ToLower())
                    .SumAsync(i => i.Quantity);
                drug.Stock = totalQuantity;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Drug>> GetLowStockDrugsAsync()
        {
            return await _context.Drugs
                .Where(d => d.Stock <= d.LowStockThreshold)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}