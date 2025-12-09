export interface Sale {
  salesId: number;
  date: string;
  totalAmount: number;
  quantity: number;
  unitPrice: number;
  drugId: number;
  drugName: string;
  orderId: number;
  paymentMethod: string;
}

export interface CreateSaleRequest {
  orderId: number;
  paymentMethod: string;
}

export interface SalesAnalytics {
  totalRevenue: number;
  totalSales: number;
  averageOrderValue: number;
  topSellingDrugs: TopSellingDrug[];
  dailySales: DailySales[];
  paymentMethods: PaymentMethod[];
}

export interface TopSellingDrug {
  drugName: string;
  totalQuantity: number;
  totalRevenue: number;
}

export interface DailySales {
  date: string;
  revenue: number;
  orderCount: number;
}

export interface PaymentMethod {
  method: string;
  count: number;
  amount: number;
}