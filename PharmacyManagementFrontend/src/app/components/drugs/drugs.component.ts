import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { DrugService } from '../../services/drug.service';
import { AuthService } from '../../services/auth.service';
import { Drug, CreateDrugRequest } from '../../models/drug.models';

@Component({
  selector: 'app-drugs',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './drugs.component.html',
  styleUrl: './drugs.component.css'
})
export class DrugsComponent implements OnInit {
  drugs: Drug[] = [];
  filteredDrugs: Drug[] = [];
  searchTerm: string = '';
  isLoading = false;
  errorMessage = '';
  successMessage = '';
  
  // Form fields for adding/editing drugs
  showAddForm = false;
  editingDrug: Drug | null = null;
  drugForm: CreateDrugRequest = {
    name: '',
    manufacturer: '',
    price: 0,
    storageInstructions: '',
    isPrescriptionRequired: false
  };

  constructor(
    private drugService: DrugService,
    public authService: AuthService
  ) {}

  ngOnInit() {
    this.loadDrugs();
  }

  loadDrugs() {
    this.isLoading = true;
    this.drugService.getAllDrugs().subscribe({
      next: (drugs) => {
        this.drugs = drugs;
        this.filteredDrugs = drugs;
        this.isLoading = false;
      },
      error: (error) => {
        this.errorMessage = 'Failed to load drugs';
        this.isLoading = false;
      }
    });
  }

  searchDrugs() {
    if (this.searchTerm.trim()) {
      this.filteredDrugs = this.drugs.filter(drug =>
        drug.name.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        drug.manufacturer.toLowerCase().includes(this.searchTerm.toLowerCase())
      );
    } else {
      this.filteredDrugs = this.drugs;
    }
  }

  showAddDrugForm() {
    this.showAddForm = true;
    this.editingDrug = null;
    this.resetForm();
  }

  editDrug(drug: Drug) {
    this.editingDrug = drug;
    this.showAddForm = true;
    this.drugForm = {
      name: drug.name,
      manufacturer: drug.manufacturer,
      price: drug.price,
      storageInstructions: drug.storageInstructions || '',
      isPrescriptionRequired: drug.isPrescriptionRequired
    };
  }

  isPlaceholderDrug(): boolean {
    return this.editingDrug?.manufacturer === 'TBD';
  }

  saveDrug() {
    if (this.editingDrug) {
      // Update existing drug
      const isPlaceholder = this.isPlaceholderDrug();
      const updateRequest = {
        name: this.editingDrug.name,
        manufacturer: isPlaceholder ? this.drugForm.manufacturer : this.editingDrug.manufacturer,
        price: this.drugForm.price,
        storageInstructions: this.drugForm.storageInstructions,
        isPrescriptionRequired: isPlaceholder ? this.drugForm.isPrescriptionRequired : this.editingDrug.isPrescriptionRequired
      };
      
      this.drugService.updateDrug(this.editingDrug.drugId, updateRequest).subscribe({
        next: () => {
          this.successMessage = isPlaceholder 
            ? 'Placeholder drug completed successfully!' 
            : 'Drug updated successfully.';
          this.loadDrugs();
          this.cancelForm();
        },
        error: () => {
          this.errorMessage = 'Failed to update drug';
        }
      });
    } else {
      // Create new drug
      this.drugService.createDrug(this.drugForm).subscribe({
        next: () => {
          this.successMessage = 'Drug added successfully. Stock synced from inventory.';
          this.loadDrugs();
          this.cancelForm();
        },
        error: () => {
          this.errorMessage = 'Failed to add drug';
        }
      });
    }
  }



  cancelForm() {
    this.showAddForm = false;
    this.editingDrug = null;
    this.resetForm();
  }

  resetForm() {
    this.drugForm = {
      name: '',
      manufacturer: '',
      price: 0,
      storageInstructions: '',
      isPrescriptionRequired: false
    };
  }

  clearMessages() {
    this.errorMessage = '';
    this.successMessage = '';
  }
}