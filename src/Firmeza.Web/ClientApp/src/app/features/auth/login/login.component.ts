import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { LoginDto, RegisterDto } from '../../../core/models/auth.model';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="login-wrapper">
      <div class="login-card">
        <div class="login-header">
          <h2>{{ isRegisterMode ? 'Crear Cuenta de Cliente' : 'Iniciar Sesión' }}</h2>
          <p class="login-subtitle">Sistema de Gestión Firmeza</p>
        </div>

        <div *ngIf="errorMessage" class="error-msg">
          {{ errorMessage }}
        </div>

        <!-- Formulario de Login -->
        <form *ngIf="!isRegisterMode" (ngSubmit)="onLogin()" #loginForm="ngForm">
          <div class="form-group">
            <label for="email">Correo Electrónico:</label>
            <input
              id="email"
              type="email"
              name="email"
              [(ngModel)]="loginCredentials.email"
              required
              class="form-control"
              placeholder="ejemplo@firmeza.com"
            />
          </div>

          <div class="form-group">
            <label for="password">Contraseña:</label>
            <input
              id="password"
              type="password"
              name="password"
              [(ngModel)]="loginCredentials.password"
              required
              class="form-control"
              placeholder="••••••••"
            />
          </div>

          <button
            type="submit"
            [disabled]="!loginForm.form.valid || loading"
            class="btn btn-primary btn-block"
          >
            {{ loading ? 'Ingresando...' : 'Iniciar Sesión' }}
          </button>
        </form>

        <!-- Formulario de Registro Público de Clientes -->
        <form *ngIf="isRegisterMode" (ngSubmit)="onRegister()" #registerForm="ngForm">
          <div class="form-group">
            <label for="regName">Nombre o Empresa:</label>
            <input
              id="regName"
              type="text"
              name="name"
              [(ngModel)]="registerData.name"
              required
              class="form-control"
              placeholder="Constructora o Nombre completo"
            />
          </div>

          <div class="form-group">
            <label for="regEmail">Correo Electrónico:</label>
            <input
              id="regEmail"
              type="email"
              name="email"
              [(ngModel)]="registerData.email"
              required
              class="form-control"
              placeholder="cliente@ejemplo.com"
            />
          </div>

          <div class="form-group">
            <label for="regPhone">Teléfono:</label>
            <input
              id="regPhone"
              type="text"
              name="phone"
              [(ngModel)]="registerData.phone"
              class="form-control"
              placeholder="3001234567"
            />
          </div>

          <div class="form-group">
            <label for="regAddress">Dirección:</label>
            <input
              id="regAddress"
              type="text"
              name="address"
              [(ngModel)]="registerData.address"
              class="form-control"
              placeholder="Calle 10 # 5-20"
            />
          </div>

          <div class="form-group">
            <label for="regPassword">Contraseña:</label>
            <input
              id="regPassword"
              type="password"
              name="password"
              [(ngModel)]="registerData.password"
              required
              minlength="6"
              class="form-control"
              placeholder="Mínimo 6 caracteres"
            />
          </div>

          <button
            type="submit"
            [disabled]="!registerForm.form.valid || loading"
            class="btn btn-primary btn-block"
          >
            {{ loading ? 'Registrando...' : 'Crear Cuenta y Entrar' }}
          </button>
        </form>

        <div class="toggle-mode">
          <button type="button" class="btn-link" (click)="toggleMode()">
            {{ isRegisterMode ? '¿Ya tienes cuenta? Inicia sesión aquí' : '¿Eres cliente nuevo? Regístrate aquí' }}
          </button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .login-wrapper {
      display: flex;
      justify-content: center;
      align-items: center;
      min-height: 60vh;
      padding: 1rem;
    }
    .login-card {
      background: #ffffff;
      border-radius: 8px;
      padding: 2.5rem;
      width: 100%;
      max-width: 440px;
      box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -2px rgba(0, 0, 0, 0.1);
    }
    .login-header {
      text-align: center;
      margin-bottom: 2rem;
    }
    .login-header h2 {
      font-size: 1.75rem;
      color: #0f172a;
      margin-bottom: 0.5rem;
    }
    .login-subtitle {
      color: #64748b;
      font-size: 0.9rem;
    }
    .btn-block {
      width: 100%;
      margin-top: 1rem;
      padding: 0.75rem;
      font-size: 1rem;
    }
    .toggle-mode {
      text-align: center;
      margin-top: 1.5rem;
      padding-top: 1rem;
      border-top: 1px solid #e2e8f0;
    }
    .btn-link {
      background: none;
      border: none;
      color: #0284c7;
      cursor: pointer;
      font-size: 0.9rem;
      text-decoration: underline;
    }
    .btn-link:hover {
      color: #0369a1;
    }
  `]
})
export class LoginComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  isRegisterMode = false;
  loading = false;
  errorMessage = '';

  loginCredentials: LoginDto = {
    email: '',
    password: ''
  };

  registerData: RegisterDto = {
    name: '',
    email: '',
    phone: '',
    address: '',
    password: '',
    role: 'Client'
  };

  toggleMode(): void {
    this.isRegisterMode = !this.isRegisterMode;
    this.errorMessage = '';
  }

  onLogin(): void {
    this.loading = true;
    this.errorMessage = '';

    this.authService.login(this.loginCredentials).subscribe({
      next: (res) => {
        this.loading = false;
        if (res.role === 'Admin') {
          this.router.navigate(['/admin/products']);
        } else {
          this.router.navigate(['/portal/products']);
        }
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error?.message || 'Error al iniciar sesión. Verifica tus credenciales.';
      }
    });
  }

  onRegister(): void {
    this.loading = true;
    this.errorMessage = '';
    this.registerData.role = 'Client';

    this.authService.register(this.registerData).subscribe({
      next: (res) => {
        this.loading = false;
        if (res.role === 'Admin') {
          this.router.navigate(['/admin/products']);
        } else {
          this.router.navigate(['/portal/products']);
        }
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error?.message || 'Error al registrar el cliente. Inténtalo de nuevo.';
      }
    });
  }
}
