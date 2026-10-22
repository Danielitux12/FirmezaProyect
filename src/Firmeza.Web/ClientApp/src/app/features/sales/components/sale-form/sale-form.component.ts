import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { SaleService } from '../../services/sale.service';
import { CreateSaleDto } from '../../models/sale.model';
import { CreateSaleDetailDto } from '../../../sale-details/models/sale-detail.model';

@Component({
  selector: 'app-sale-form',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="form-container">
      <h2>Nueva Venta</h2>
      <div *ngIf="error" class="error-msg">{{ error }}</div>

      <form (ngSubmit)="saveSale()" #saleForm="ngForm">
        <div class="form-group">
          <label for="clientId">ID de Cliente (Guid):</label>
          <input id="clientId" name="clientId" [(ngModel)]="model.clientId" required class="form-control" placeholder="00000000-0000-0000-0000-000000000000" />
        </div>

        <div class="details-section">
          <h3>Detalles de la Venta</h3>
          <div *ngFor="let item of model.saleDetails; let i = index" class="detail-row">
            <input [(ngModel)]="item.productId" [name]="'prod_' + i" placeholder="ID Producto (Guid)" required class="form-control" />
            <input type="number" [(ngModel)]="item.quantity" [name]="'qty_' + i" placeholder="Cantidad" min="1" required class="form-control" />
            <input type="number" step="0.01" [(ngModel)]="item.unitPrice" [name]="'price_' + i" placeholder="Precio Unitario" min="0" required class="form-control" />
            <button type="button" (click)="removeItem(i)" class="btn btn-sm btn-danger">X</button>
          </div>

          <button type="button" (click)="addItem()" class="btn btn-sm">+ Agregar Producto</button>
        </div>

        <div class="form-actions">
          <button type="submit" [disabled]="!saleForm.form.valid || model.saleDetails.length === 0 || saving" class="btn btn-primary">
            {{ saving ? 'Guardando...' : 'Crear Venta' }}
          </button>
          <a routerLink="/admin/sales" class="btn">Cancelar</a>
        </div>
      </form>
    </div>
  `
})
export class SaleFormComponent {
  private readonly saleService = inject(SaleService);
  private readonly router = inject(Router);

  model: CreateSaleDto = {
    clientId: '',
    saleDetails: []
  };

  saving = false;
  error = '';

  addItem(): void {
    this.model.saleDetails.push({
      productId: '',
      quantity: 1,
      unitPrice: 0
    });
  }

  removeItem(index: number): void {
    this.model.saleDetails.splice(index, 1);
  }

  saveSale(): void {
    this.saving = true;
    this.error = '';

    this.saleService.create(this.model).subscribe({
      next: () => {
        this.saving = false;
        this.router.navigate(['/admin/sales']);
      },
      error: (err) => {
        this.error = 'Error al crear la venta. Verifica los datos.';
        this.saving = false;
        console.error(err);
      }
    });
  }
}
