import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { SaleDetailDto, CreateSaleDetailDto } from '../models/sale-detail.model';

@Injectable({
  providedIn: 'root'
})
export class SaleDetailService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/SaleDetails`;

  getAll(): Observable<SaleDetailDto[]> {
    return this.http.get<SaleDetailDto[]>(this.apiUrl);
  }

  getById(id: string): Observable<SaleDetailDto> {
    return this.http.get<SaleDetailDto>(`${this.apiUrl}/${id}`);
  }

  create(dto: CreateSaleDetailDto): Observable<SaleDetailDto> {
    return this.http.post<SaleDetailDto>(this.apiUrl, dto);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
