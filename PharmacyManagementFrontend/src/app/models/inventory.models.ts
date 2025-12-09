export interface Inventory {
  id: number;
  drugId: number;
  drugName: string;
  supplierId: number;
  supplierName: string;
  quantity: number;
  lastRestockDate?: string;
  drugStorageInstructions?: string;
}

export interface CreateInventoryRequest {
  drugName: string;
  supplierId: number;
  quantity: number;
}

export interface UpdateInventoryRequest {
  newQuantity: number;
}