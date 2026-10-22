import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { CartService } from '../../../../core/services/cart.service';
import { SaleService } from '../../../sales/services/sale.service';
import { AuthService } from '../../../../core/services/auth.service';
import { CreateSaleDto } from '../../../sales/models/sale.model';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="entity-container">
      <div class="entity-header">
        <h2>Carrito de Compras</h2>
        <a routerLink="/portal/products" class="btn">&larr; Seguir Comprando</a>
      </div>

      <!-- Mensajes de Estado -->
      <div *ngIf="errorMessage" class="error-msg">
        {{ errorMessage }}
      </div>

      <div *ngIf="successMessage" class="success-banner">
        <h3>🎉 ¡Compra confirmada exitosamente!</h3>
        <p>{{ successMessage }}</p>
        <div class="receipt-alert">
          <p><strong>📄 Comprobante de Venta:</strong> El comprobante oficial en formato PDF ha sido generado y enviado a tu correo electrónico registrado.</p>
          <div *ngIf="createdSaleId" style="margin-top: 0.75rem;">
            <button type="button" (click)="downloadReceipt(createdSaleId)" class="btn btn-sm btn-primary">
              📥 Descargar Comprobante PDF Ahora
            </button>
          </div>
        </div>
        <p style="margin-top: 1rem; font-size: 0.85rem; color: #475569;">
          Redirigiendo a tu historial de ventas en unos segundos...
        </p>
      </div>

      <!-- Carrito con Productos -->
      <div *ngIf="cartService.items().length > 0 && !successMessage" class="cart-layout">
        <div class="cart-table-wrapper">
          <table class="data-table">
            <thead>
              <tr>
                <th>Producto</th>
                <th>Precio Unitario</th>
                <th>Cantidad</th>
                <th>Subtotal</th>
                <th>Acción</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let item of cartService.items()">
                <td>
                  <strong>{{ item.product.name }}</strong>
                  <div class="stock-hint">Disponible: {{ item.product.stock }}</div>
                </td>
                <td>{{ item.product.price | currency }}</td>
                <td style="max-width: 140px;">
                  <div class="qty-control">
                    <button
                      type="button"
                      (click)="changeQty(item.product.id, item.quantity - 1)"
                      class="btn-qty"
                      [disabled]="item.quantity <= 1"
                    >-</button>
                    <input
                      type="number"
                      [ngModel]="item.quantity"
                      (ngModelChange)="onQtyInput(item.product.id, $event, item.product.stock)"
                      min="1"
                      [max]="item.product.stock"
                      class="qty-input"
                    />
                    <button
                      type="button"
                      (click)="changeQty(item.product.id, item.quantity + 1)"
                      class="btn-qty"
                      [disabled]="item.quantity >= item.product.stock"
                    >+</button>
                  </div>
                </td>
                <td>
                  <strong>{{ (item.product.price * item.quantity) | currency }}</strong>
                </td>
                <td>
                  <button
                    type="button"
                    (click)="removeItem(item.product.id)"
                    class="btn btn-sm btn-danger"
                    title="Quitar producto"
                  >
                    Quitar
                  </button>
                </td>
              </tr>
            </tbody>
          </table>

          <div class="cart-table-actions">
            <button type="button" (click)="clearCart()" class="btn btn-sm btn-outline-danger">
              Vaciar Carrito
            </button>
          </div>
        </div>

        <!-- Resumen del Carrito -->
        <div class="cart-summary-card">
          <h3>Resumen del Pedido</h3>
          <div class="summary-line">
            <span>Total Artículos:</span>
            <strong>{{ cartService.totalItems() }} uds.</strong>
          </div>
          <div class="summary-line">
            <span>Líneas de Detalle:</span>
            <span>{{ cartService.items().length }} productos</span>
          </div>
          <div class="summary-divider"></div>
          <div class="summary-total">
            <span>Total a Pagar:</span>
            <span class="total-price">{{ cartService.totalAmount() | currency }}</span>
          </div>

          <button
            type="button"
            (click)="confirmPurchase()"
            [disabled]="submitting || cartService.items().length === 0"
            class="btn btn-primary btn-block btn-checkout"
          >
            {{ submitting ? 'Procesando compra...' : 'Confirmar Compra' }}
          </button>
        </div>
      </div>

      <!-- Carrito Vacío -->
      <div *ngIf="cartService.items().length === 0 && !successMessage" class="empty-cart">
        <div class="empty-icon">🛒</div>
        <h3>Tu carrito está vacío</h3>
        <p>No tienes productos agregados para comprar en este momento.</p>
        <a routerLink="/portal/products" class="btn btn-primary" style="margin-top: 1rem;">
          Explorar Catálogo de Materiales
        </a>
      </div>
    </div>
  `,
  styles: [`
    .cart-layout {
      display: grid;
      grid-template-columns: 1fr 320px;
      gap: 2rem;
      align-items: start;
    }
    @media (max-width: 900px) {
      .cart-layout {
        grid-template-columns: 1fr;
      }
    }
    .stock-hint {
      font-size: 0.75rem;
      color: #64748b;
    }
    .qty-control {
      display: inline-flex;
      align-items: center;
      border: 1px solid #cbd5e1;
      border-radius: 4px;
      overflow: hidden;
      background: #ffffff;
    }
    .btn-qty {
      background: #f1f5f9;
      border: none;
      width: 28px;
      height: 32px;
      cursor: pointer;
      font-weight: bold;
      color: #334155;
    }
    .btn-qty:disabled {
      opacity: 0.4;
      cursor: not-allowed;
    }
    .qty-input {
      width: 44px;
      height: 32px;
      border: none;
      text-align: center;
      font-size: 0.95rem;
      outline: none;
      -moz-appearance: textfield;
    }
    .qty-input::-webkit-outer-spin-button,
    .qty-input::-webkit-inner-spin-button {
      -webkit-appearance: none;
      margin: 0;
    }
    .cart-table-actions {
      margin-top: 1rem;
      display: flex;
      justify-content: flex-end;
    }
    .btn-outline-danger {
      background: transparent;
      border: 1px solid #dc2626;
      color: #dc2626;
    }
    .btn-outline-danger:hover {
      background: #fee2e2;
    }
    .cart-summary-card {
      background: #ffffff;
      border-radius: 8px;
      padding: 1.5rem;
      box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
      border: 1px solid #e2e8f0;
    }
    .cart-summary-card h3 {
      font-size: 1.2rem;
      margin-bottom: 1.25rem;
      color: #0f172a;
    }
    .summary-line {
      display: flex;
      justify-content: space-between;
      margin-bottom: 0.75rem;
      font-size: 0.95rem;
      color: #475569;
    }
    .summary-divider {
      height: 1px;
      background: #e2e8f0;
      margin: 1rem 0;
    }
    .summary-total {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 1.5rem;
      font-size: 1.1rem;
      font-weight: 700;
      color: #0f172a;
    }
    .total-price {
      color: #059669;
      font-size: 1.35rem;
    }
    .btn-checkout {
      padding: 0.85rem;
      font-size: 1.05rem;
      font-weight: 600;
    }
    .empty-cart {
      text-align: center;
      background: #ffffff;
      padding: 3.5rem 1.5rem;
      border-radius: 8px;
      border: 1px solid #e2e8f0;
      max-width: 500px;
      margin: 2rem auto;
    }
    .empty-icon {
      font-size: 3.5rem;
      margin-bottom: 1rem;
    }
    .success-banner {
      background-color: #ecfdf5;
      border: 1px solid #a7f3d0;
      color: #065f46;
      padding: 2rem;
      border-radius: 8px;
      text-align: center;
      margin-bottom: 2rem;
    }
    .success-banner h3 {
      margin-bottom: 0.5rem;
      color: #047857;
    }
    .receipt-alert {
      background: #ffffff;
      border: 1px solid #a7f3d0;
      border-radius: 6px;
      padding: 1rem;
      margin: 1rem auto;
      max-width: 500px;
      color: #064e3b;
    }
  `]
})
export class CartComponent {
  readonly cartService = inject(CartService);
  private readonly saleService = inject(SaleService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  submitting = false;
  errorMessage = '';
  successMessage = '';
  createdSaleId: string | null = null;

  changeQty(productId: string, newQty: number): void {
    this.errorMessage = '';
    this.cartService.updateQuantity(productId, newQty);
  }

  onQtyInput(productId: string, value: any, maxStock: number): void {
    const val = parseInt(value, 10);
    this.errorMessage = '';

    if (isNaN(val) || val < 1) {
      this.cartService.updateQuantity(productId, 1);
    } else if (val > maxStock) {
      this.cartService.updateQuantity(productId, maxStock);
      this.errorMessage = `Cantidad ajustada al stock máximo disponible (${maxStock} unidades).`;
    } else {
      this.cartService.updateQuantity(productId, val);
    }
  }

  removeItem(productId: string): void {
    this.cartService.removeItem(productId);
  }

  clearCart(): void {
    if (confirm('¿Deseas vaciar todos los productos del carrito?')) {
      this.cartService.clearCart();
    }
  }

  confirmPurchase(): void {
    const items = this.cartService.items();
    if (items.length === 0) {
      this.errorMessage = 'El carrito está vacío.';
      return;
    }

    this.submitting = true;
    this.errorMessage = '';
    this.successMessage = '';

    const payload: CreateSaleDto = {
      clientId: this.authService.currentUser()?.clientId || '',
      saleDetails: items.map(item => ({
        productId: item.product.id,
        quantity: item.quantity,
        unitPrice: item.product.price
      }))
    };

    this.saleService.create(payload).subscribe({
      next: (createdSale) => {
        this.submitting = false;
        this.createdSaleId = createdSale.id;
        this.cartService.clearCart();
        this.successMessage = `Tu orden #${createdSale.id.substring(0, 8).toUpperCase()} por un total de $${createdSale.total} ha sido registrada con éxito.`;

        setTimeout(() => {
          this.router.navigate(['/portal/my-sales']);
        }, 5000);
      },
      error: (err) => {
        this.submitting = false;
        console.error('Error al confirmar venta:', err);
        this.errorMessage = err.error?.message ||
          'No se pudo procesar la compra. Es posible que el stock haya cambiado o hubo un problema de conexión. Por favor intenta de nuevo.';
      }
    });
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
        console.error('Error al descargar comprobante:', err);
        window.open(`http://localhost:5115/recibos/recibo_${saleId}.pdf`, '_blank');
      }
    });
  }
}
