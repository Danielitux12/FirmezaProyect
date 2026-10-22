import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ExportService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/Export`;

  exportProductsExcel(): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/products/excel`, { responseType: 'blob' });
  }

  exportProductsPdf(): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/products/pdf`, { responseType: 'blob' });
  }

  exportClientsExcel(): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/clients/excel`, { responseType: 'blob' });
  }

  exportClientsPdf(): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/clients/pdf`, { responseType: 'blob' });
  }

  exportSalesExcel(): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/sales/excel`, { responseType: 'blob' });
  }

  exportSalesPdf(): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/sales/pdf`, { responseType: 'blob' });
  }

  downloadFile(blob: Blob, defaultFilename: string): void {
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = defaultFilename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    window.URL.revokeObjectURL(url);
  }
}
