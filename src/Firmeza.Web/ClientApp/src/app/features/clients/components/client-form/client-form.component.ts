import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ClientService } from '../../services/client.service';
import { CreateClientDto } from '../../models/client.model';

@Component({
  selector: 'app-client-form',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="form-container">
      <h2>{{ isEdit ? 'Editar Cliente' : 'Nuevo Cliente' }}</h2>
      <div *ngIf="error" class="error-msg">{{ error }}</div>

      <form (ngSubmit)="saveClient()" #clientForm="ngForm">
        <div class="form-group">
          <label for="name">Nombre:</label>
          <input id="name" name="name" [(ngModel)]="model.name" required class="form-control" />
        </div>

        <div class="form-group">
          <label for="email">Email:</label>
          <input id="email" type="email" name="email" [(ngModel)]="model.email" required class="form-control" />
        </div>

        <div class="form-group">
          <label for="phone">Teléfono:</label>
          <input id="phone" name="phone" [(ngModel)]="model.phone" class="form-control" />
        </div>

        <div class="form-group">
          <label for="address">Dirección:</label>
          <input id="address" name="address" [(ngModel)]="model.address" class="form-control" />
        </div>

        <div *ngIf="!isEdit" class="form-group">
          <label for="password">Contraseña de acceso al portal (opcional):</label>
          <input
            id="password"
            type="password"
            name="password"
            [(ngModel)]="model.password"
            minlength="6"
            class="form-control"
            placeholder="Si deseas crearle cuenta al cliente de una vez (mín. 6 caracteres)"
          />
        </div>

        <div class="form-actions">
          <button type="submit" [disabled]="!clientForm.form.valid || saving" class="btn btn-primary">
            {{ saving ? 'Guardando...' : (isEdit ? 'Actualizar' : 'Crear') }}
          </button>
          <a routerLink="/admin/clients" class="btn">Cancelar</a>
        </div>
      </form>
    </div>
  `
})
export class ClientFormComponent implements OnInit {
  private readonly clientService = inject(ClientService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  model: CreateClientDto = {
    name: '',
    email: '',
    phone: '',
    address: ''
  };

  clientId: string | null = null;
  isEdit = false;
  saving = false;
  error = '';

  ngOnInit(): void {
    this.clientId = this.route.snapshot.paramMap.get('id');
    if (this.clientId) {
      this.isEdit = true;
      this.clientService.getById(this.clientId).subscribe({
        next: (client) => {
          this.model = {
            name: client.name,
            email: client.email,
            phone: client.phone,
            address: client.address
          };
        },
        error: (err) => {
          this.error = 'No se pudo cargar el cliente.';
          console.error(err);
        }
      });
    }
  }

  saveClient(): void {
    this.saving = true;
    this.error = '';

    if (this.isEdit && this.clientId) {
      this.clientService.update(this.clientId, this.model).subscribe({
        next: () => {
          this.saving = false;
          this.router.navigate(['/admin/clients']);
        },
        error: (err) => {
          this.error = 'Error al actualizar el cliente.';
          this.saving = false;
          console.error(err);
        }
      });
    } else {
      this.clientService.create(this.model).subscribe({
        next: () => {
          this.saving = false;
          this.router.navigate(['/admin/clients']);
        },
        error: (err) => {
          this.error = 'Error al crear el cliente.';
          this.saving = false;
          console.error(err);
        }
      });
    }
  }
}
