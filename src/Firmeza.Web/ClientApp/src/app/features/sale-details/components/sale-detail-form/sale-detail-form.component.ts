import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { SaleDetailService } from '../../services/sale-detail.service';
import { CreateSaleDetailDto } from '../../models/sale-detail.model';

@Component({
  selector: 'app-sale-detail-form',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="form-container">
      <h2>Nuevo Detalle de Venta</h2>
      <div *ngIf="error" class="error-msg">{{ error }}</div>

      <form (ngSubmit)="saveDetail()" #detailForm="ngForm">
        <div class="form-group">
          <label for="productId">ID de Producto (Guid):</label>
          <input id="productId" name="productId" [(ngModel)]="model.productId" required class="form-control" placeholder="00000000-0000-0000-0000-000000000000" />
        </div>

        <div class="form-group">
          <label for="quantity">Cantidad:</label>
          <input id="quantity" type="number" name="quantity" [(ngModel)]="model.quantity" required min="1" class="form-control" />
        </div>

        <div class="form-group">
          <label for="unitPrice">Precio Unitario:</label>
          <input id="unitPrice" type="number" step="0.01" name="unitPrice" [(ngModel)]="model.unitPrice" required min="0" class="form-control" />
        </div>

        <div class="form-actions">
          <button type="submit" [disabled]="!detailForm.form.valid || saving" class="btn btn-primary">
            {{ saving ? 'Guardando...' : 'Crear Detalle' }}
          </button>
          <a routerLink="/admin/sale-details" class="btn">Cancelar</a>
        </div>
      </form>
    </div>
  `
})
export class SaleDetailFormComponent {
  private readonly saleDetailService = inject(SaleDetailService);
  private readonly router = inject(Router);

  model: CreateSaleDetailDto = {
    productId: '',
    quantity: 1,
    unitPrice: 0
  };

  saving = false;
  error = '';

  saveDetail(): void {
    this.saving = true;
    this.error = '';

    this.saleDetailService.create(this.model).subscribe({
      next: () => {
        this.saving = false;
        this.router.navigate(['/admin/sale-details']);
      },
      error: (err) => {
        this.error = 'Error al crear el detalle. Verifica los datos.';
        this.saving = false;
        console.error(err);
      }
    });
  }
}
