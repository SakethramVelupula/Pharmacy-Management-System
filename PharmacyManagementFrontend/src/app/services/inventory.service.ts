import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Inventory, CreateInventoryRequest, UpdateInventoryRequest } from '../models/inventory.models';

@Injectable({
  providedIn: 'root'
})
export class InventoryService {
  private apiUrl = 'http://localhost:5063/api/inventory';

  constructor(private http: HttpClient) { }

  getAllInventory(): Observable<Inventory[]> {
    return this.http.get<Inventory[]>(`${this.apiUrl}/view`);
  }

  getInventoryById(drugId: number): Observable<Inventory> {
    return this.http.get<Inventory>(`${this.apiUrl}/byId/${drugId}`);
  }

  createInventory(inventory: CreateInventoryRequest): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/add`, inventory);
  }

  updateInventory(drugName: string, quantity: number): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/update/${drugName}`, { quantity });
  }
}