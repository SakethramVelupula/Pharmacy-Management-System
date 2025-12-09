import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { InventoryService } from '../../services/inventory.service';
import { DrugService } from '../../services/drug.service';
import { AuthService } from '../../services/auth.service';
import { SupplierService } from '../../services/supplier.service';
import { Supplier } from '../../models/supplier.models';
import { Inventory, CreateInventoryRequest } from '../../models/inventory.models';
import { Drug } from '../../models/drug.models';

@Component({
  selector: 'app-inventory',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './inventory.component.html',
  styleUrl: './inventory.component.css'
})
export class InventoryComponent implements OnInit {
  inventory: Inventory[] = [];
  filteredInventory: Inventory[] = [];
  drugs: Drug[] = [];
  suppliers: Supplier[] = [];
  searchTerm: string = '';
  isLoading = false;
  errorMessage = '';
  successMessage = '';
  
  // Form fields for adding/editing inventory
  showAddForm = false;
  editingInventory: Inventory | null = null;
  inventoryForm = {
    drugName: '',
    supplierId: 0,
    quantity: 0
  };

  constructor(
    private inventoryService: InventoryService,
    private drugService: DrugService,
    private supplierService: SupplierService,
    public authService: AuthService
  ) {}

  ngOnInit() {
    this.loadInventory();
    this.loadDrugs();
    this.loadSuppliers();
  }

  loadInventory() {
    this.isLoading = true;
    console.log('Loading inventory...');
    this.inventoryService.getAllInventory().subscribe({
      next: (inventory) => {
        console.log('Inventory loaded:', inventory);
        this.inventory = inventory;
        this.filteredInventory = inventory;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Failed to load inventory:', error);
        this.errorMessage = 'Failed to load inventory: ' + error.message;
        this.isLoading = false;
      }
    });
  }

  loadDrugs() {
    this.drugService.getAllDrugs().subscribe({
      next: (drugs) => {
        this.drugs = drugs;
      },
      error: (error) => {
        console.error('Failed to load drugs:', error);
      }
    });
  }

  loadSuppliers() {
    this.supplierService.getAllSuppliers().subscribe({
      next: (suppliers) => {
        this.suppliers = suppliers;
        // Create default supplier if none exist
        if (this.suppliers.length === 0) {
          this.createDefaultSupplier();
        } else if (this.inventoryForm.supplierId === 0) {
          this.inventoryForm.supplierId = this.suppliers[0].supplierId;
        }
      },
      error: (error) => {
        console.error('Failed to load suppliers:', error);
        this.createDefaultSupplier();
      }
    });
  }

  createDefaultSupplier() {
    const defaultSupplier = {
      name: 'Default Supplier',
      email: 'default@pharmacy.com',
      phoneNumber: '000-000-0000'
    };
    
    this.supplierService.createSupplier(defaultSupplier).subscribe({
      next: (supplier) => {
        this.suppliers = [supplier];
        this.inventoryForm.supplierId = supplier.supplierId;
      },
      error: (error) => {
        console.error('Failed to create default supplier:', error);
        // Fallback: use supplier ID 1
        this.inventoryForm.supplierId = 1;
      }
    });
  }

  searchInventory() {
    if (this.searchTerm.trim()) {
      this.filteredInventory = this.inventory.filter(item =>
        item.drugName.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        item.supplierName.toLowerCase().includes(this.searchTerm.toLowerCase())
      );
    } else {
      this.filteredInventory = this.inventory;
    }
  }

  showAddInventoryForm() {
    this.showAddForm = true;
    this.editingInventory = null;
    this.resetForm();
  }

  editInventory(inventory: Inventory) {
    this.editingInventory = inventory;
    this.showAddForm = true;
    this.inventoryForm = {
      drugName: inventory.drugName,
      supplierId: inventory.supplierId,
      quantity: inventory.quantity
    };
  }

  saveInventory() {
    if (this.editingInventory) {
      // Update existing inventory
      this.inventoryService.updateInventory(this.editingInventory.drugName, this.inventoryForm.quantity).subscribe({
        next: () => {
          this.successMessage = 'Inventory updated successfully';
          this.loadInventory();
          this.cancelForm();
        },
        error: () => {
          this.errorMessage = 'Failed to update inventory';
        }
      });
    } else {
      // Create new inventory
      const createRequest: CreateInventoryRequest = {
        drugName: this.inventoryForm.drugName,
        supplierId: this.inventoryForm.supplierId,
        quantity: this.inventoryForm.quantity
      };
      
      this.inventoryService.createInventory(createRequest).subscribe({
        next: () => {
          const drugExists = this.drugs.some(d => d.name.toLowerCase() === createRequest.drugName.toLowerCase());
          this.successMessage = drugExists 
            ? 'Inventory added successfully' 
            : 'Inventory added successfully. New drug created - please update drug details in Drug Management.';
          this.loadInventory();
          this.loadDrugs(); // Refresh drug list to show new placeholder
          this.cancelForm();
        },
        error: (error) => {
          console.error('Inventory creation error:', error);
          if (error.status === 404) {
            this.errorMessage = 'API endpoint not found. Please ensure backend server is running on port 5063.';
          } else if (error.status === 401) {
            this.errorMessage = 'Unauthorized. Please login again.';
          } else if (error.status === 400) {
            this.errorMessage = 'Invalid data. Please check all fields.';
          } else {
            this.errorMessage = `Failed to add inventory: ${error.message || 'Unknown error'}`;
          }
        }
      });
    }
  }

  deleteInventory(inventory: Inventory) {
    if (confirm(`Are you sure you want to delete inventory for ${inventory.drugName}?`)) {
      // Note: Backend doesn't have delete endpoint, removing this functionality
      this.errorMessage = 'Delete functionality not available';
    }
  }

  cancelForm() {
    this.showAddForm = false;
    this.editingInventory = null;
    this.resetForm();
  }

  resetForm() {
    this.inventoryForm = {
      drugName: '',
      supplierId: this.suppliers.length > 0 ? this.suppliers[0].supplierId : 1,
      quantity: 0
    };
  }

  clearMessages() {
    this.errorMessage = '';
    this.successMessage = '';
  }



  isLowStock(inventory: Inventory): boolean {
    return inventory.quantity <= 10; // Fixed threshold since reorderLevel removed
  }
}