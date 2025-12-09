import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SalesService } from '../../services/sales.service';
import { AuthService } from '../../services/auth.service';
import { Sale, SalesAnalytics } from '../../models/sales.models';

@Component({
  selector: 'app-sales',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './sales.component.html',
  styleUrl: './sales.component.css'
})
export class SalesComponent implements OnInit {
  sales: Sale[] = [];
  analytics: SalesAnalytics | null = null;
  loading = false;
  activeTab = 'analytics';
  
  // Date filters
  startDate = '';
  endDate = '';

  constructor(
    private salesService: SalesService,
    public authService: AuthService
  ) {
    // Set default date range (last 30 days)
    const today = new Date();
    const thirtyDaysAgo = new Date(today.getTime() - (30 * 24 * 60 * 60 * 1000));
    
    this.endDate = today.toISOString().split('T')[0];
    this.startDate = thirtyDaysAgo.toISOString().split('T')[0];
  }

  ngOnInit() {
    this.loadAnalytics();
  }

  setActiveTab(tab: string) {
    this.activeTab = tab;
    if (tab === 'sales' && this.sales.length === 0) {
      this.loadSales();
    } else if (tab === 'analytics') {
      this.loadAnalytics();
    }
  }

  loadSales() {
    this.loading = true;
    this.salesService.getAllSales().subscribe({
      next: (sales) => {
        this.sales = sales;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading sales:', error);
        this.loading = false;
      }
    });
  }

  loadAnalytics() {
    this.loading = true;
    this.salesService.getSalesAnalytics(this.startDate, this.endDate).subscribe({
      next: (analytics) => {
        this.analytics = analytics;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading analytics:', error);
        this.loading = false;
      }
    });
  }

  onDateFilterChange() {
    if (this.startDate && this.endDate) {
      this.loadAnalytics();
    }
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(amount);
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString();
  }

  getPaymentMethodColor(method: string): string {
    const colors: { [key: string]: string } = {
      'Cash': '#28a745',
      'Card': '#007bff',
      'Insurance': '#6f42c1',
      'Credit': '#fd7e14'
    };
    return colors[method] || '#6c757d';
  }
}