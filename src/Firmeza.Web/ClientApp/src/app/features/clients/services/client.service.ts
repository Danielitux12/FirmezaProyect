import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { ClientDto, CreateClientDto } from '../models/client.model';

@Injectable({
  providedIn: 'root'
})
export class ClientService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/Clients`;

  getAll(): Observable<ClientDto[]> {
    return this.http.get<ClientDto[]>(this.apiUrl);
  }

  getById(id: string): Observable<ClientDto> {
    return this.http.get<ClientDto>(`${this.apiUrl}/${id}`);
  }

  create(dto: CreateClientDto): Observable<ClientDto> {
    return this.http.post<ClientDto>(this.apiUrl, dto);
  }

  update(id: string, dto: CreateClientDto): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, dto);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
