import { Component, OnInit, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ProductService } from '../../services/product.service';
import { ProductDto } from '../../models/product.model';
import { AuthService } from '../../../../core/services/auth.service';
import { CartService } from '../../../../core/services/cart.service';
import { ExportService } from '../../../../core/services/export.service';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="entity-container">
      <div class="entity-header">
        <h2>{{ readOnly || authService.isClient() ? 'Catálogo de Productos' : 'Gestión de Productos' }}</h2>
        <div class="header-actions" style="display: flex; gap: 0.5rem; align-items: center; flex-wrap: wrap;">
          <button *ngIf="!readOnly && authService.isAdmin()" type="button" (click)="exportExcel()" class="btn btn-sm btn-outline-success" [disabled]="exporting">
            📊 Exportar Excel
          </button>
          <button *ngIf="!readOnly && authService.isAdmin()" type="button" (click)="exportPdf()" class="btn btn-sm btn-outline-danger" [disabled]="exporting">
            📄 Exportar PDF
          </button>
          <a *ngIf="!readOnly && authService.isAdmin()" routerLink="/admin/products/new" class="btn btn-primary">+ Nuevo Producto</a>
          <a *ngIf="authService.isClient()" routerLink="/portal/cart" class="btn btn-primary">
            🛒 Ver Carrito ({{ cartService.totalItems() }})
          </a>
        </div>
      </div>

      <!-- Notificación rápida al agregar al carrito -->
      <div *ngIf="notification" class="notification-bar">
        <span>{{ notification }}</span>
        <a routerLink="/portal/cart" class="notification-link">Ir al Carrito &rarr;</a>
      </div>

      <div *ngIf="loading" class="loading">Cargando productos...</div>
      <div *ngIf="error" class="error-msg">{{ error }}</div>

      <table *ngIf="!loading && products.length > 0" class="data-table">
        <thead>
          <tr>
            <th>Nombre</th>
            <th>Descripción</th>
            <th>Precio</th>
            <th>Stock</th>
            <th>Estado</th>
            <th *ngIf="authService.isAdmin() || authService.isClient()">Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let product of products">
            <td><strong>{{ product.name }}</strong></td>
            <td>{{ product.description }}</td>
            <td>{{ product.price | currency }}</td>
            <td>
              <span [class.text-danger]="product.stock === 0">{{ product.stock }} unidades</span>
            </td>
            <td>
              <span [class.status-active]="product.isActive && product.stock > 0" [class.status-inactive]="!product.isActive || product.stock === 0">
                {{ product.isActive && product.stock > 0 ? 'Disponible' : (product.stock === 0 ? 'Agotado' : 'No disponible') }}
              </span>
            </td>

            <!-- Acciones ADMIN -->
            <td *ngIf="!readOnly && authService.isAdmin()">
              <a [routerLink]="['/admin/products/edit', product.id]" class="btn btn-sm">Editar</a>
              <button (click)="deleteProduct(product.id)" class="btn btn-sm btn-danger" style="margin-left: 0.5rem;">Eliminar</button>
            </td>

            <!-- Acciones CLIENTE -->
            <td *ngIf="authService.isClient()">
              <div *ngIf="product.isActive && product.stock > 0; else outOfStockTpl" class="cart-actions">
                <button
                  type="button"
                  (click)="addToCart(product)"
                  [disabled]="isMaxInCart(product)"
                  class="btn btn-sm btn-success"
                >
                  {{ isMaxInCart(product) ? 'Máx. alcanzado' : '+ Agregar' }}
                </button>
                <span *ngIf="cartService.getItemQuantity(product.id) > 0" class="in-cart-badge">
                  (En carrito: {{ cartService.getItemQuantity(product.id) }})
                </span>
              </div>
              <ng-template #outOfStockTpl>
                <span class="badge-muted">Sin stock</span>
              </ng-template>
            </td>
          </tr>
        </tbody>
      </table>

      <p *ngIf="!loading && products.length === 0">No hay productos registrados.</p>
    </div>
  `,
  styles: [`
    .notification-bar {
      background-color: #ecfdf5;
      color: #065f46;
      border: 1px solid #a7f3d0;
      padding: 0.75rem 1rem;
      border-radius: 6px;
      margin-bottom: 1rem;
      display: flex;
      justify-content: space-between;
      align-items: center;
    }
    .notification-link {
      font-weight: 600;
      color: #047857;
      text-decoration: underline;
    }
    .cart-actions {
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }
    .in-cart-badge {
      font-size: 0.8rem;
      color: #0369a1;
      font-weight: 500;
    }
    .btn-success {
      background-color: #10b981;
      color: #ffffff;
      border: none;
    }
    .btn-success:hover:not(:disabled) {
      background-color: #059669;
    }
    .btn-success:disabled {
      background-color: #9ca3af;
      cursor: not-allowed;
    }
    .text-danger {
      color: #dc2626;
      font-weight: 600;
    }
    .status-active {
      color: #059669;
      font-weight: 500;
    }
    .status-inactive {
      color: #dc2626;
      font-weight: 500;
    }
    .badge-muted {
      color: #6b7280;
      font-style: italic;
    }
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
export class ProductListComponent implements OnInit {
  @Input() readOnly = false;

  private readonly productService = inject(ProductService);
  private readonly exportService = inject(ExportService);
  readonly authService = inject(AuthService);
  readonly cartService = inject(CartService);

  products: ProductDto[] = [];
  loading = false;
  exporting = false;
  error = '';
  notification = '';
  private notificationTimer: any = null;

  ngOnInit(): void {
    this.loadProducts();
  }

  loadProducts(): void {
    this.loading = true;
    this.error = '';
    this.productService.getAll().subscribe({
      next: (data) => {
        this.products = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Error al cargar productos.';
        this.loading = false;
        console.error(err);
      }
    });
  }

  addToCart(product: ProductDto): void {
    const added = this.cartService.addItem(product, 1);
    if (added) {
      this.showNotification(`¡"${product.name}" agregado al carrito!`);
    } else {
      this.showNotification(`No puedes agregar más de ${product.stock} unidades de este producto.`);
    }
  }

  isMaxInCart(product: ProductDto): boolean {
    return this.cartService.getItemQuantity(product.id) >= product.stock;
  }

  private showNotification(msg: string): void {
    this.notification = msg;
    if (this.notificationTimer) {
      clearTimeout(this.notificationTimer);
    }
    this.notificationTimer = setTimeout(() => {
      this.notification = '';
    }, 4000);
  }

  exportExcel(): void {
    this.exporting = true;
    this.exportService.exportProductsExcel().subscribe({
      next: (blob) => {
        this.exportService.downloadFile(blob, `productos_firmeza_${new Date().toISOString().slice(0, 10)}.xlsx`);
        this.exporting = false;
      },
      error: (err) => {
        console.error('Error al exportar productos a Excel:', err);
        this.exporting = false;
        alert('Error al exportar a Excel.');
      }
    });
  }

  exportPdf(): void {
    this.exporting = true;
    this.exportService.exportProductsPdf().subscribe({
      next: (blob) => {
        this.exportService.downloadFile(blob, `catalogo_productos_${new Date().toISOString().slice(0, 10)}.pdf`);
        this.exporting = false;
      },
      error: (err) => {
        console.error('Error al exportar productos a PDF:', err);
        this.exporting = false;
        alert('Error al exportar a PDF.');
      }
    });
  }

  deleteProduct(id: string): void {
    if (confirm('¿Estás seguro de eliminar este producto?')) {
      this.productService.delete(id).subscribe({
        next: () => this.loadProducts(),
        error: (err) => {
          this.error = 'Error al eliminar el producto.';
          console.error(err);
        }
      });
    }
  }
}
