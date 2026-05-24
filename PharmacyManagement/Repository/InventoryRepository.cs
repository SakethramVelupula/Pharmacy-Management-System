using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PharmacyManagement.Interface;
using PharmacyManagement.Models;
using PharmacyManagement.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PharmacyManagement.Repository

{

    public class InventoryRepository : IInventoryRepository

    {

        private readonly ApplicationDbContext _context;
        private readonly ILogger<InventoryRepository> _logger;
        private readonly ISupplierRepository _supplierRepo;

        public InventoryRepository(ApplicationDbContext context, ILogger<InventoryRepository> logger, ISupplierRepository supplierRepo)
        {
            _context = context;
            _logger = logger;
            _supplierRepo = supplierRepo;
        }

        public async Task<IEnumerable<Inventory>> GetAllInventoryAsync()

        {

            _logger.LogInformation("Fetching all inventory items from DB.");

            return await _context.Inventory

                .Include(i => i.Drug)

                .Include(i => i.Supplier)

                .AsNoTracking()

                .ToListAsync();

        }

        public async Task<Inventory?> GetInventoryByDrugIdAsync(int drugId)

        {

            _logger.LogInformation("Fetching inventory for Drug ID: {DrugId}", drugId);

            return await _context.Inventory

                .Include(i => i.Drug)

                .Include(i => i.Supplier)

                .AsNoTracking()

                .FirstOrDefaultAsync(i => i.DrugId == drugId);

        }

        public async Task<Inventory?> GetInventoryByDrugNameAsync(string drugName)
 
        {

            _logger.LogInformation("Fetching inventory for Drug Name: {DrugName}", drugName);

            return await _context.Inventory

                .Include(i => i.Drug)

                .Include(i => i.Supplier)

                .AsNoTracking()

                .FirstOrDefaultAsync(i => i.Drug.Name.ToLower() == drugName.ToLower());

        }

        public async Task<bool> AddDrugToInventoryAsync(string drugName, int supplierId, int quantity, DateTime? expiryDate)

        {

            var drug = await _context.Drugs.FirstOrDefaultAsync(d => d.Name.ToLower() == drugName.ToLower());

            // If drug doesn't exist, create a placeholder drug entry
            if (drug == null)
            {
                _logger.LogInformation("Drug '{DrugName}' not found. Creating placeholder entry.", drugName);
                drug = new Drug
                {
                    Name = drugName,
                    Manufacturer = "TBD",
                    PricePerUnit = 0,
                    Stock = 0,
                    StorageInstructions = "Pending catalog entry",
                    IsPrescriptionRequired = false
                };
                _context.Drugs.Add(drug);
                await _context.SaveChangesAsync(); // Save to get DrugId
            }

            // If supplier doesn't exist, use or create default supplier
            var supplier = await _context.Suppliers.FindAsync(supplierId);
            if (supplier == null)
            {
                _logger.LogInformation("Supplier ID {SupplierId} not found. Using default supplier.", supplierId);
                supplier = await _supplierRepo.EnsureDefaultSupplierAsync();
                supplierId = supplier.SupplierId;
            }

            var existingInventory = await _context.Inventory

                .FirstOrDefaultAsync(i => i.DrugId == drug.DrugId && i.SupplierId == supplierId);

            DateTime now = DateTime.UtcNow;

            if (existingInventory != null)
            {
                _logger.LogInformation("Updating existing inventory for Drug ID: {DrugId}", drug.DrugId);
                existingInventory.Quantity += quantity;
                existingInventory.LastRestockDate = now;
                if (expiryDate.HasValue)
                    existingInventory.ExpiryDate = expiryDate;
                _context.Inventory.Update(existingInventory);
            }
            else
            {
                _logger.LogInformation("Creating new inventory record for Drug ID: {DrugId}", drug.DrugId);
                await _context.Inventory.AddAsync(new Inventory
                {
                    DrugId = drug.DrugId,
                    SupplierId = supplierId,
                    Quantity = quantity,
                    LastRestockDate = now,
                    ExpiryDate = expiryDate
                });
            }

            await _context.SaveChangesAsync();

            return true;

        }

        public async Task<bool> UpdateDrugQuantityAsync(string drugName, int newQuantity)

        {

            var inventoryItem = await _context.Inventory

                .Include(i => i.Drug)

                .FirstOrDefaultAsync(i => i.Drug.Name.ToLower() == drugName.ToLower());

            if (inventoryItem == null)

                throw new KeyNotFoundException($"Inventory for drug '{drugName}' not found.");

            inventoryItem.Quantity += newQuantity;

            _context.Inventory.Update(inventoryItem);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated quantity for Drug: {DrugName} to {NewQuantity}", drugName, newQuantity);

            return true;

        }


        public async Task<IEnumerable<Inventory>> GetExpiringBatchesAsync(int warningDays)
        {
            var thresholdDate = DateTime.UtcNow.AddDays(warningDays);
            return await _context.Inventory
                .Include(i => i.Drug)
                .Include(i => i.Supplier)
                .Where(i => i.ExpiryDate.HasValue
                    && i.ExpiryDate.Value.Date > DateTime.UtcNow.Date
                    && i.ExpiryDate.Value.Date <= thresholdDate.Date
                    && i.Quantity > 0)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Inventory>> GetExpiredBatchesAsync()
        {
            return await _context.Inventory
                .Include(i => i.Drug)
                .Include(i => i.Supplier)
                .Where(i => i.ExpiryDate.HasValue
                    && i.ExpiryDate.Value.Date <= DateTime.UtcNow.Date
                    && i.Quantity > 0)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}