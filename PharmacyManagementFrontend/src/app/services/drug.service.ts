import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Drug, CreateDrugRequest, UpdateDrugRequest } from '../models/drug.models';

@Injectable({
  providedIn: 'root'
})
export class DrugService {
  private apiUrl = 'http://localhost:5063/api/drugs';

  constructor(private http: HttpClient) { }

  getAllDrugs(): Observable<Drug[]> {
    return this.http.get<Drug[]>(this.apiUrl);
  }

  getDrugById(id: number): Observable<Drug> {
    return this.http.get<Drug>(`${this.apiUrl}/${id}`);
  }

  createDrug(drug: CreateDrugRequest): Observable<Drug> {
    return this.http.post<Drug>(this.apiUrl, drug);
  }

  updateDrug(drugId: number, drug: UpdateDrugRequest): Observable<Drug> {
    return this.http.put<Drug>(`${this.apiUrl}/${drugId}`, drug);
  }

  deleteDrug(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  searchDrugs(searchTerm: string): Observable<Drug[]> {
    return this.http.get<Drug[]>(`${this.apiUrl}/search?term=${searchTerm}`);
  }
}