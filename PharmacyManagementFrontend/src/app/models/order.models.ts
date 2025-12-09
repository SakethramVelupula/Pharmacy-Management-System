export interface Order {
  id: number;
  doctorId: string;
  drugId: number;
  quantity: number;
  status: string;
  placedAt: string;
  prescriptionReference?: string;
  dateDispensed?: string;
  drug?: {
    drugId: number;
    name: string;
    manufacturer: string;
    price: number;
    stock: number;
  };
}

export interface CreateOrderRequest {
  drugId: number;
  quantity: number;
  prescriptionReference?: string;
}

export interface UpdateOrderRequest {
  quantity: number;
  status: string;
  prescriptionReference?: string;
  dateDispensed?: string;
}

export type OrderStatus = 'Pending' | 'Processing' | 'Delivered' | 'Cancelled';