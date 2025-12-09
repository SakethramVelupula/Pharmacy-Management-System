export interface Drug {
  drugId: number;
  name: string;
  manufacturer: string;
  price: number;
  stock: number;
  storageInstructions?: string;
  isPrescriptionRequired: boolean;
}

export interface CreateDrugRequest {
  name: string;
  manufacturer: string;
  price: number;
  storageInstructions?: string;
  isPrescriptionRequired: boolean;
}

export interface UpdateDrugRequest {
  name: string;
  manufacturer: string;
  price: number;
  storageInstructions?: string;
  isPrescriptionRequired: boolean;
}