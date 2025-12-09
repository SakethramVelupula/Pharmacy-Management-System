export interface SpecialOrder {
  requestId: number;
  dateRequested: string;
  drugName: string;
  manufacturer?: string;
  reason: string;
  status: string;
  adminNotes?: string;
  dateResolved?: string;
  doctorId: string;
}

export interface CreateSpecialOrderRequest {
  drugName: string;
  manufacturer?: string;
  reason: string;
}

export interface UpdateSpecialOrderStatusRequest {
  status: string;
  adminNotes?: string;
}

export type SpecialOrderStatus = 'Pending' | 'Approved' | 'Rejected' | 'Ordered' | 'Received';