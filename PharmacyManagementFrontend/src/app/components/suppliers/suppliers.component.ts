import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SupplierService } from '../../services/supplier.service';
import { AuthService } from '../../services/auth.service';
import { Supplier, CreateSupplierRequest, UpdateSupplierRequest } from '../../models/supplier.models';

@Component({
  selector: 'app-suppliers',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './suppliers.component.html',
  styleUrl: './suppliers.component.css'
})
export class SuppliersComponent implements OnInit {
  suppliers: Supplier[] = [];
  filteredSuppliers: Supplier[] = [];
  loading = false;
  showForm = false;
  editingSupplier: Supplier | null = null;
  searchTerm = '';
  successMessage = '';
  errorMessage = '';

  supplierForm = {
    name: '',
    email: '',
    phoneNumber: ''
  };

  constructor(
    private supplierService: SupplierService,
    public authService: AuthService
  ) {}

  ngOnInit() {
    this.loadSuppliers();
  }

  loadSuppliers() {
    this.loading = true;
    this.supplierService.getAllSuppliers().subscribe({
      next: (suppliers) => {
        this.suppliers = suppliers;
        this.filteredSuppliers = suppliers;
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Error loading suppliers:', error);
        this.errorMessage = 'Failed to load suppliers';
        this.loading = false;
      }
    });
  }

  onSearch() {
    if (!this.searchTerm.trim()) {
      this.filteredSuppliers = this.suppliers;
      return;
    }

    this.filteredSuppliers = this.suppliers.filter(supplier =>
      supplier.name.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
      supplier.email.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
      (supplier.phoneNumber && supplier.phoneNumber.includes(this.searchTerm))
    );
  }

  openForm() {
    this.showForm = true;
    this.editingSupplier = null;
    this.resetForm();
  }

  editSupplier(supplier: Supplier) {
    this.editingSupplier = supplier;
    this.showForm = true;
    this.supplierForm = {
      name: supplier.name,
      email: supplier.email,
      phoneNumber: supplier.phoneNumber || ''
    };
  }

  closeForm() {
    this.showForm = false;
    this.resetForm();
  }

  resetForm() {
    this.supplierForm = {
      name: '',
      email: '',
      phoneNumber: ''
    };
    this.clearMessages();
  }

  onSubmit() {
    if (!this.supplierForm.name.trim() || !this.supplierForm.email.trim()) {
      this.errorMessage = 'Name and email are required';
      return;
    }

    this.loading = true;
    
    if (this.editingSupplier) {
      // Update existing supplier
      this.supplierService.updateSupplier(this.editingSupplier.supplierId, this.supplierForm as UpdateSupplierRequest).subscribe({
        next: (supplier: Supplier) => {
          const index = this.suppliers.findIndex(s => s.supplierId === supplier.supplierId);
          if (index !== -1) {
            this.suppliers[index] = supplier;
            this.filteredSuppliers = this.suppliers;
          }
          this.successMessage = 'Supplier updated successfully';
          this.closeForm();
          this.loading = false;
        },
        error: (error: any) => {
          console.error('Error updating supplier:', error);
          this.errorMessage = 'Failed to update supplier';
          this.loading = false;
        }
      });
    } else {
      // Create new supplier
      this.supplierService.createSupplier(this.supplierForm as CreateSupplierRequest).subscribe({
        next: (supplier: Supplier) => {
          this.suppliers.push(supplier);
          this.filteredSuppliers = this.suppliers;
          this.successMessage = 'Supplier added successfully';
          this.closeForm();
          this.loading = false;
        },
        error: (error: any) => {
          console.error('Error creating supplier:', error);
          this.errorMessage = 'Failed to create supplier';
          this.loading = false;
        }
      });
    }
  }

  clearMessages() {
    this.successMessage = '';
    this.errorMessage = '';
  }
}