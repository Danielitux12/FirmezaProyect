import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { SaleDetailService } from '../../services/sale-detail.service';
import { SaleDetailDto } from '../../models/sale-detail.model';

@Component({
  selector: 'app-sale-detail-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="entity-container">
      <div class="entity-header">
        <h2>Detalles de Venta</h2>
        <a routerLink="/admin/sale-details/new" class="btn btn-primary">+ Nuevo Detalle</a>
      </div>

      <div *ngIf="loading" class="loading">Cargando detalles...</div>
      <div *ngIf="error" class="error-msg">{{ error }}</div>

      <table *ngIf="!loading && details.length > 0" class="data-table">
        <thead>
          <tr>
            <th>Producto</th>
            <th>Cantidad</th>
            <th>Precio Unitario</th>
            <th>Subtotal</th>
            <th>Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let item of details">
            <td>{{ item.productName || item.productId }}</td>
            <td>{{ item.quantity }}</td>
            <td>{{ item.unitPrice | currency }}</td>
            <td>{{ item.subtotal | currency }}</td>
            <td>
              <button (click)="deleteDetail(item.id)" class="btn btn-sm btn-danger">Eliminar</button>
            </td>
          </tr>
        </tbody>
      </table>

      <p *ngIf="!loading && details.length === 0">No hay detalles de venta registrados.</p>
    </div>
  `
})
export class SaleDetailListComponent implements OnInit {
  private readonly saleDetailService = inject(SaleDetailService);

  details: SaleDetailDto[] = [];
  loading = false;
  error = '';

  ngOnInit(): void {
    this.loadDetails();
  }

  loadDetails(): void {
    this.loading = true;
    this.error = '';
    this.saleDetailService.getAll().subscribe({
      next: (data) => {
        this.details = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Error al cargar detalles de venta.';
        this.loading = false;
        console.error(err);
      }
    });
  }

  deleteDetail(id: string): void {
    if (confirm('¿Estás seguro de eliminar este detalle?')) {
      this.saleDetailService.delete(id).subscribe({
        next: () => this.loadDetails(),
        error: (err) => {
          this.error = 'Error al eliminar el detalle.';
          console.error(err);
        }
      });
    }
  }
}
