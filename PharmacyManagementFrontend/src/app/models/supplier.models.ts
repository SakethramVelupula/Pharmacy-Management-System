export interface Supplier {
  supplierId: number;
  name: string;
  email: string;
  phoneNumber?: string;
}

export interface CreateSupplierRequest {
  name: string;
  email: string;
  phoneNumber?: string;
}

export interface UpdateSupplierRequest {
  name: string;
  email: string;
  phoneNumber?: string;
}