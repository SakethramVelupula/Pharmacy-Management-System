import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { SpecialOrderService } from '../../services/special-order.service';
import { AuthService } from '../../services/auth.service';
import { SpecialOrder, CreateSpecialOrderRequest, SpecialOrderStatus } from '../../models/special-order.models';

@Component({
  selector: 'app-special-orders',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './special-orders.component.html',
  styleUrl: './special-orders.component.css'
})
export class SpecialOrdersComponent implements OnInit {
  specialOrders: SpecialOrder[] = [];
  filteredOrders: SpecialOrder[] = [];
  searchTerm: string = '';
  statusFilter: string = 'All';
  isLoading = false;
  errorMessage = '';
  successMessage = '';
  
  // Form fields for creating special orders
  showAddForm = false;
  orderForm: CreateSpecialOrderRequest = {
    drugName: '',
    manufacturer: '',
    reason: ''
  };

  specialOrderStatuses: SpecialOrderStatus[] = ['Pending', 'Approved', 'Rejected', 'Ordered', 'Received'];

  constructor(
    private specialOrderService: SpecialOrderService,
    public authService: AuthService
  ) {}

  ngOnInit() {
    this.loadSpecialOrders();
  }

  loadSpecialOrders() {
    this.isLoading = true;
    this.specialOrderService.getAllSpecialOrders().subscribe({
      next: (orders) => {
        this.specialOrders = orders;
        this.applyFilters();
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Failed to load special orders:', error);
        this.errorMessage = 'Failed to load special orders';
        this.isLoading = false;
      }
    });
  }

  applyFilters() {
    let filtered = this.specialOrders;

    // Apply search filter
    if (this.searchTerm.trim()) {
      const searchLower = this.searchTerm.toLowerCase();
      filtered = filtered.filter(order => 
        order.drugName.toLowerCase().includes(searchLower) ||
        order.manufacturer?.toLowerCase().includes(searchLower) ||
        order.reason.toLowerCase().includes(searchLower) ||
        order.requestId.toString().includes(searchLower)
      );
    }

    // Apply status filter
    if (this.statusFilter !== 'All') {
      filtered = filtered.filter(order => order.status === this.statusFilter);
    }

    this.filteredOrders = filtered;
  }

  onSearchChange() {
    this.applyFilters();
  }

  onStatusFilterChange() {
    this.applyFilters();
  }

  showCreateOrderForm() {
    this.showAddForm = true;
    this.resetForm();
  }

  createSpecialOrder() {
    if (!this.orderForm.drugName.trim()) {
      this.errorMessage = 'Drug name is required';
      return;
    }

    if (!this.orderForm.reason.trim()) {
      this.errorMessage = 'Reason is required';
      return;
    }

    this.specialOrderService.createSpecialOrder(this.orderForm).subscribe({
      next: () => {
        this.successMessage = 'Special order request created successfully';
        this.loadSpecialOrders();
        this.cancelForm();
      },
      error: (error) => {
        console.error('Failed to create special order:', error);
        if (error.status === 403) {
          this.errorMessage = 'Access denied. Only doctors can create special orders. Please ensure you are logged in as a doctor.';
        } else if (error.status === 401) {
          this.errorMessage = 'Unauthorized. Please login again.';
        } else if (error.status === 400) {
          this.errorMessage = 'Invalid data. Please check all required fields.';
        } else {
          this.errorMessage = `Failed to create special order: ${error.message || 'Unknown error'}`;
        }
      }
    });
  }

  updateOrderStatus(order: SpecialOrder, newStatus: SpecialOrderStatus, adminNotes?: string) {
    const statusUpdate = {
      status: newStatus,
      adminNotes: adminNotes || ''
    };

    this.specialOrderService.updateOrderStatus(order.requestId, statusUpdate).subscribe({
      next: () => {
        this.successMessage = `Special order status updated to ${newStatus}`;
        this.loadSpecialOrders();
      },
      error: (error) => {
        console.error('Failed to update special order status:', error);
        this.errorMessage = 'Failed to update special order status';
      }
    });
  }

  cancelForm() {
    this.showAddForm = false;
    this.resetForm();
  }

  resetForm() {
    this.orderForm = {
      drugName: '',
      manufacturer: '',
      reason: ''
    };
  }

  clearMessages() {
    this.errorMessage = '';
    this.successMessage = '';
  }

  getStatusClass(status: string): string {
    switch (status.toLowerCase()) {
      case 'pending': return 'status-pending';
      case 'approved': return 'status-approved';
      case 'rejected': return 'status-rejected';
      case 'ordered': return 'status-ordered';
      case 'received': return 'status-received';
      default: return 'status-pending';
    }
  }

  getUrgencyClass(dateRequested: string): string {
    const requestDate = new Date(dateRequested);
    const daysDiff = Math.floor((Date.now() - requestDate.getTime()) / (1000 * 60 * 60 * 24));
    
    if (daysDiff > 7) return 'urgent';
    if (daysDiff > 3) return 'moderate';
    return 'recent';
  }
}