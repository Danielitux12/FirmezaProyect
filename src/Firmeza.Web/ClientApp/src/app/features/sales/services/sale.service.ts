import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { SaleDto, CreateSaleDto } from '../models/sale.model';

@Injectable({
  providedIn: 'root'
})
export class SaleService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/Sales`;

  getAll(): Observable<SaleDto[]> {
    return this.http.get<SaleDto[]>(this.apiUrl);
  }

  getById(id: string): Observable<SaleDto> {
    return this.http.get<SaleDto>(`${this.apiUrl}/${id}`);
  }

  create(dto: CreateSaleDto): Observable<SaleDto> {
    return this.http.post<SaleDto>(this.apiUrl, dto);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  downloadReceipt(id: string): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/${id}/receipt`, { responseType: 'blob' });
  }
}
