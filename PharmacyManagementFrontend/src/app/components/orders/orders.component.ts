import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { OrderService } from '../../services/order.service';
import { DrugService } from '../../services/drug.service';
import { AuthService } from '../../services/auth.service';
import { Order, CreateOrderRequest, OrderStatus } from '../../models/order.models';
import { Drug } from '../../models/drug.models';

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './orders.component.html',
  styleUrl: './orders.component.css'
})
export class OrdersComponent implements OnInit {
  orders: Order[] = [];
  filteredOrders: Order[] = [];
  drugs: Drug[] = [];
  searchTerm: string = '';
  statusFilter: string = 'All';
  isLoading = false;
  errorMessage = '';
  successMessage = '';
  
  // Form fields for creating orders
  showAddForm = false;
  orderForm: CreateOrderRequest = {
    drugId: 0,
    quantity: 1,
    prescriptionReference: ''
  };

  orderStatuses: OrderStatus[] = ['Pending', 'Processing', 'Delivered', 'Cancelled'];

  constructor(
    private orderService: OrderService,
    private drugService: DrugService,
    public authService: AuthService
  ) {}

  ngOnInit() {
    this.loadOrders();
    this.loadDrugs(); // Load drugs for all users to enable search by drug name
  }

  loadOrders() {
    this.isLoading = true;
    this.orderService.getAllOrders().subscribe({
      next: (orders) => {
        this.orders = orders;
        this.applyFilters();
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Failed to load orders:', error);
        this.errorMessage = 'Failed to load orders';
        this.isLoading = false;
      }
    });
  }

  loadDrugs() {
    this.drugService.getAllDrugs().subscribe({
      next: (drugs) => {
        this.drugs = drugs; // Load all drugs for search functionality
      },
      error: (error) => {
        console.error('Failed to load drugs:', error);
      }
    });
  }

  applyFilters() {
    let filtered = this.orders;

    // Apply search filter
    if (this.searchTerm.trim()) {
      const searchLower = this.searchTerm.toLowerCase();
      filtered = filtered.filter(order => {
        // Search by Order ID
        const orderIdMatch = order.id.toString().includes(searchLower);
        
        // Search by Drug Name (using getDrugName method)
        const drugName = this.getDrugName(order.drugId).toLowerCase();
        const drugNameMatch = drugName.includes(searchLower);
        
        return orderIdMatch || drugNameMatch;
      });
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

  createOrder() {
    if (this.orderForm.drugId === 0) {
      this.errorMessage = 'Please select a drug';
      return;
    }

    this.orderService.createOrder(this.orderForm).subscribe({
      next: () => {
        this.successMessage = 'Order created successfully';
        this.loadOrders();
        this.cancelForm();
      },
      error: (error) => {
        console.error('Failed to create order:', error);
        this.errorMessage = 'Failed to create order';
      }
    });
  }

  updateOrderStatus(order: Order, newStatus: OrderStatus) {
    this.orderService.updateOrderStatus(order.id, newStatus).subscribe({
      next: () => {
        this.successMessage = `Order status updated to ${newStatus}`;
        this.loadOrders();
      },
      error: (error) => {
        console.error('Failed to update order status:', error);
        this.errorMessage = 'Failed to update order status';
      }
    });
  }

  deleteOrder(order: Order) {
    if (confirm(`Are you sure you want to delete order #${order.id}?`)) {
      this.orderService.deleteOrder(order.id).subscribe({
        next: () => {
          this.successMessage = 'Order deleted successfully';
          this.loadOrders();
        },
        error: (error) => {
          console.error('Failed to delete order:', error);
          this.errorMessage = 'Failed to delete order';
        }
      });
    }
  }

  cancelForm() {
    this.showAddForm = false;
    this.resetForm();
  }

  resetForm() {
    this.orderForm = {
      drugId: 0,
      quantity: 1,
      prescriptionReference: ''
    };
  }

  clearMessages() {
    this.errorMessage = '';
    this.successMessage = '';
  }

  getStatusClass(status: string): string {
    switch (status.toLowerCase()) {
      case 'pending': return 'status-pending';
      case 'processing': return 'status-processing';
      case 'delivered': return 'status-delivered';
      case 'cancelled': return 'status-cancelled';
      default: return 'status-pending';
    }
  }

  getDrugName(drugId: number): string {
    const drug = this.drugs.find(d => d.drugId === drugId);
    return drug ? drug.name : 'Unknown Drug';
  }
}