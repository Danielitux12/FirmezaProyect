import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ClientService } from '../../services/client.service';
import { ClientDto } from '../../models/client.model';
import { ExportService } from '../../../../core/services/export.service';

@Component({
  selector: 'app-client-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="entity-container">
      <div class="entity-header">
        <h2>Gestión de Clientes</h2>
        <div class="header-actions" style="display: flex; gap: 0.5rem; align-items: center; flex-wrap: wrap;">
          <button type="button" (click)="exportExcel()" class="btn btn-sm btn-outline-success" [disabled]="exporting">
            📊 Exportar Excel
          </button>
          <button type="button" (click)="exportPdf()" class="btn btn-sm btn-outline-danger" [disabled]="exporting">
            📄 Exportar PDF
          </button>
          <a routerLink="/admin/clients/new" class="btn btn-primary">+ Nuevo Cliente</a>
        </div>
      </div>

      <div *ngIf="loading" class="loading">Cargando clientes...</div>
      <div *ngIf="error" class="error-msg">{{ error }}</div>

      <table *ngIf="!loading && clients.length > 0" class="data-table">
        <thead>
          <tr>
            <th>Nombre</th>
            <th>Email</th>
            <th>Teléfono</th>
            <th>Dirección</th>
            <th>Estado</th>
            <th>Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let client of clients">
            <td>{{ client.name }}</td>
            <td>{{ client.email }}</td>
            <td>{{ client.phone }}</td>
            <td>{{ client.address }}</td>
            <td>{{ client.isActive ? 'Activo' : 'Inactivo' }}</td>
            <td>
              <a [routerLink]="['/admin/clients/edit', client.id]" class="btn btn-sm">Editar</a>
              <button (click)="deleteClient(client.id)" class="btn btn-sm btn-danger">Eliminar</button>
            </td>
          </tr>
        </tbody>
      </table>

      <p *ngIf="!loading && clients.length === 0">No hay clientes registrados.</p>
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
export class ClientListComponent implements OnInit {
  private readonly clientService = inject(ClientService);
  private readonly exportService = inject(ExportService);

  clients: ClientDto[] = [];
  loading = false;
  exporting = false;
  error = '';

  ngOnInit(): void {
    this.loadClients();
  }

  loadClients(): void {
    this.loading = true;
    this.error = '';
    this.clientService.getAll().subscribe({
      next: (data) => {
        this.clients = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Error al cargar clientes.';
        this.loading = false;
        console.error(err);
      }
    });
  }

  exportExcel(): void {
    this.exporting = true;
    this.exportService.exportClientsExcel().subscribe({
      next: (blob) => {
        this.exportService.downloadFile(blob, `clientes_firmeza_${new Date().toISOString().slice(0, 10)}.xlsx`);
        this.exporting = false;
      },
      error: (err) => {
        console.error('Error al exportar clientes a Excel:', err);
        this.exporting = false;
        alert('Error al exportar a Excel.');
      }
    });
  }

  exportPdf(): void {
    this.exporting = true;
    this.exportService.exportClientsPdf().subscribe({
      next: (blob) => {
        this.exportService.downloadFile(blob, `directorio_clientes_${new Date().toISOString().slice(0, 10)}.pdf`);
        this.exporting = false;
      },
      error: (err) => {
        console.error('Error al exportar clientes a PDF:', err);
        this.exporting = false;
        alert('Error al exportar a PDF.');
      }
    });
  }

  deleteClient(id: string): void {
    if (confirm('¿Estás seguro de eliminar este cliente?')) {
      this.clientService.delete(id).subscribe({
        next: () => this.loadClients(),
        error: (err) => {
          this.error = 'Error al eliminar el cliente.';
          console.error(err);
        }
      });
    }
  }
}
