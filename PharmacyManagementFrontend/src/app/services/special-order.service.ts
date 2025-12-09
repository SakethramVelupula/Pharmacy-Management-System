import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { SpecialOrder, CreateSpecialOrderRequest, UpdateSpecialOrderStatusRequest } from '../models/special-order.models';

@Injectable({
  providedIn: 'root'
})
export class SpecialOrderService {
  private apiUrl = 'http://localhost:5063/api/special-orders';

  constructor(private http: HttpClient) { }

  getAllSpecialOrders(status?: string, doctorId?: string): Observable<SpecialOrder[]> {
    let params = '';
    if (status) params += `?status=${status}`;
    if (doctorId) params += params ? `&doctorId=${doctorId}` : `?doctorId=${doctorId}`;
    return this.http.get<SpecialOrder[]>(`${this.apiUrl}${params}`);
  }

  getSpecialOrderById(id: number): Observable<SpecialOrder> {
    return this.http.get<SpecialOrder>(`${this.apiUrl}/${id}`);
  }

  createSpecialOrder(order: CreateSpecialOrderRequest): Observable<SpecialOrder> {
    return this.http.post<SpecialOrder>(this.apiUrl, order);
  }

  updateOrderStatus(id: number, statusUpdate: UpdateSpecialOrderStatusRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}/status`, statusUpdate);
  }
}