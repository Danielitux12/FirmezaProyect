import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { SaleService } from '../../services/sale.service';
import { SaleDto } from '../../models/sale.model';
import { AuthService } from '../../../../core/services/auth.service';
import { ExportService } from '../../../../core/services/export.service';

@Component({
  selector: 'app-sale-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="entity-container">
      <div class="entity-header">
        <h2>{{ authService.isClient() ? 'Mis Ventas y Pedidos' : 'Gestión de Ventas' }}</h2>
        <div class="header-actions" style="display: flex; gap: 0.5rem; align-items: center; flex-wrap: wrap;">
          <button *ngIf="authService.isAdmin()" type="button" (click)="exportExcel()" class="btn btn-sm btn-outline-success" [disabled]="exporting">
            📊 Exportar Excel
          </button>
          <button *ngIf="authService.isAdmin()" type="button" (click)="exportPdf()" class="btn btn-sm btn-outline-danger" [disabled]="exporting">
            📄 Exportar PDF
          </button>
          <a *ngIf="authService.isAdmin()" routerLink="/admin/sales/new" class="btn btn-primary">+ Registrar Venta</a>
        </div>
      </div>

      <div *ngIf="loading" class="loading">Cargando ventas...</div>
      <div *ngIf="error" class="error-msg">{{ error }}</div>

      <div *ngIf="!loading && sales.length > 0">
        <table class="data-table">
          <thead>
            <tr>
              <th>Fecha</th>
              <th *ngIf="authService.isAdmin()">Cliente</th>
              <th>Total</th>
              <th>Productos</th>
              <th>Acciones</th>
            </tr>
          </thead>
          <tbody>
            <ng-container *ngFor="let sale of sales">
              <tr>
                <td>{{ sale.date | date:'medium' }}</td>
                <td *ngIf="authService.isAdmin()">{{ sale.clientName || sale.clientId }}</td>
                <td><strong>{{ sale.total | currency }}</strong></td>
                <td>{{ sale.saleDetails.length || 0 }} ítems</td>
                <td style="display: flex; gap: 0.5rem; flex-wrap: wrap;">
                  <button (click)="toggleDetails(sale.id)" class="btn btn-sm">
                    {{ expandedSaleId === sale.id ? 'Ocultar Detalle' : 'Ver Detalle' }}
                  </button>
                  <button (click)="downloadReceipt(sale.id)" class="btn btn-sm" style="background-color: #0284c7; color: white;" title="Descargar comprobante en PDF">
                    📄 Comprobante
                  </button>
                  <button *ngIf="authService.isAdmin()" (click)="deleteSale(sale.id)" class="btn btn-sm btn-danger">
                    Eliminar
                  </button>
                </td>
              </tr>
              <!-- Detalle expandible de la venta -->
              <tr *ngIf="expandedSaleId === sale.id">
                <td [attr.colspan]="authService.isAdmin() ? 5 : 4" style="background-color: #f8fafc; padding: 1.5rem;">
                  <h4>Detalle de la Venta (ID: {{ sale.id }})</h4>
                  <table class="data-table" style="margin-top: 0.5rem; background: #ffffff;">
                    <thead>
                      <tr>
                        <th>Producto</th>
                        <th>Cantidad</th>
                        <th>Precio Unitario</th>
                        <th>Subtotal</th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr *ngFor="let item of sale.saleDetails">
                        <td>{{ item.productName || item.productId }}</td>
                        <td>{{ item.quantity }}</td>
                        <td>{{ item.unitPrice | currency }}</td>
                        <td><strong>{{ item.subtotal | currency }}</strong></td>
                      </tr>
                    </tbody>
                  </table>
                </td>
              </tr>
            </ng-container>
          </tbody>
        </table>
      </div>

      <p *ngIf="!loading && sales.length === 0">No hay ventas registradas.</p>
    </div>
  `,
  styles: [`
    .btn-outline-success {
      background-color: transparent;
      border: 1px solid #10b981;
      color: #059669;
    }
    .btn-outline-success:hover:not(:disabled) {
      background-color: #ecfdf5;
    }
    .btn-outline-danger {
      background-color: transparent;
      border: 1px solid #ef4444;
      color: #dc2626;
    }
    .btn-outline-danger:hover:not(:disabled) {
      background-color: #fef2f2;
    }
  `]
})
export class SaleListComponent implements OnInit {
  private readonly saleService = inject(SaleService);
  private readonly exportService = inject(ExportService);
  readonly authService = inject(AuthService);

  sales: SaleDto[] = [];
  loading = false;
  exporting = false;
  error = '';
  expandedSaleId: string | null = null;

  ngOnInit(): void {
    this.loadSales();
  }

  loadSales(): void {
    this.loading = true;
    this.error = '';
    this.saleService.getAll().subscribe({
      next: (data) => {
        this.sales = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Error al cargar ventas.';
        this.loading = false;
        console.error(err);
      }
    });
  }

  toggleDetails(saleId: string): void {
    this.expandedSaleId = this.expandedSaleId === saleId ? null : saleId;
  }

  downloadReceipt(saleId: string): void {
    this.saleService.downloadReceipt(saleId).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `recibo_${saleId}.pdf`;
        a.click();
        window.URL.revokeObjectURL(url);
      },
      error: (err) => {
        console.error('Error al descargar comprobante vía API:', err);
        // Respaldo de descarga directa
        window.open(`http://localhost:5115/recibos/recibo_${saleId}.pdf`, '_blank');
      }
    });
  }

  exportExcel(): void {
    this.exporting = true;
    this.exportService.exportSalesExcel().subscribe({
      next: (blob) => {
        this.exportService.downloadFile(blob, `ventas_firmeza_${new Date().toISOString().slice(0, 10)}.xlsx`);
        this.exporting = false;
      },
      error: (err) => {
        console.error('Error al exportar ventas a Excel:', err);
        this.exporting = false;
        alert('Error al exportar a Excel.');
      }
    });
  }

  exportPdf(): void {
    this.exporting = true;
    this.exportService.exportSalesPdf().subscribe({
      next: (blob) => {
        this.exportService.downloadFile(blob, `reporte_ventas_${new Date().toISOString().slice(0, 10)}.pdf`);
        this.exporting = false;
      },
      error: (err) => {
        console.error('Error al exportar ventas a PDF:', err);
        this.exporting = false;
        alert('Error al exportar a PDF.');
      }
    });
  }

  deleteSale(id: string): void {
    if (confirm('¿Estás seguro de eliminar esta venta?')) {
      this.saleService.delete(id).subscribe({
        next: () => this.loadSales(),
        error: (err) => {
          this.error = 'Error al eliminar la venta.';
          console.error(err);
        }
      });
    }
  }
}
