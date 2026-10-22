import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ProductService } from '../../services/product.service';
import { CreateProductDto } from '../../models/product.model';

@Component({
  selector: 'app-product-form',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="form-container">
      <h2>{{ isEdit ? 'Editar Producto' : 'Nuevo Producto' }}</h2>
      <div *ngIf="error" class="error-msg">{{ error }}</div>

      <form (ngSubmit)="saveProduct()" #productForm="ngForm">
        <div class="form-group">
          <label for="name">Nombre:</label>
          <input id="name" name="name" [(ngModel)]="model.name" required class="form-control" />
        </div>

        <div class="form-group">
          <label for="description">Descripción:</label>
          <textarea id="description" name="description" [(ngModel)]="model.description" class="form-control" rows="3"></textarea>
        </div>

        <div class="form-group">
          <label for="price">Precio:</label>
          <input id="price" type="number" step="0.01" name="price" [(ngModel)]="model.price" required class="form-control" />
        </div>

        <div class="form-group">
          <label for="stock">Stock:</label>
          <input id="stock" type="number" name="stock" [(ngModel)]="model.stock" required class="form-control" />
        </div>

        <div class="form-actions">
          <button type="submit" [disabled]="!productForm.form.valid || saving" class="btn btn-primary">
            {{ saving ? 'Guardando...' : (isEdit ? 'Actualizar' : 'Crear') }}
          </button>
          <a routerLink="/admin/products" class="btn">Cancelar</a>
        </div>
      </form>
    </div>
  `
})
export class ProductFormComponent implements OnInit {
  private readonly productService = inject(ProductService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  model: CreateProductDto = {
    name: '',
    description: '',
    price: 0,
    stock: 0
  };

  productId: string | null = null;
  isEdit = false;
  saving = false;
  error = '';

  ngOnInit(): void {
    this.productId = this.route.snapshot.paramMap.get('id');
    if (this.productId) {
      this.isEdit = true;
      this.productService.getById(this.productId).subscribe({
        next: (product) => {
          this.model = {
            name: product.name,
            description: product.description,
            price: product.price,
            stock: product.stock
          };
        },
        error: (err) => {
          this.error = 'No se pudo cargar el producto.';
          console.error(err);
        }
      });
    }
  }

  saveProduct(): void {
    this.saving = true;
    this.error = '';

    if (this.isEdit && this.productId) {
      this.productService.update(this.productId, this.model).subscribe({
        next: () => {
          this.saving = false;
          this.router.navigate(['/admin/products']);
        },
        error: (err) => {
          this.error = 'Error al actualizar el producto.';
          this.saving = false;
          console.error(err);
        }
      });
    } else {
      this.productService.create(this.model).subscribe({
        next: () => {
          this.saving = false;
          this.router.navigate(['/admin/products']);
        },
        error: (err) => {
          this.error = 'Error al crear el producto.';
          this.saving = false;
          console.error(err);
        }
      });
    }
  }
}
