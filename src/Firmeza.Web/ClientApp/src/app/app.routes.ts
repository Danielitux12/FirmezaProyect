import { Routes } from '@angular/router';
import { LoginComponent } from './features/auth/login/login.component';
import { ClientListComponent } from './features/clients/components/client-list/client-list.component';
import { ClientFormComponent } from './features/clients/components/client-form/client-form.component';
import { ProductListComponent } from './features/products/components/product-list/product-list.component';
import { ProductFormComponent } from './features/products/components/product-form/product-form.component';
import { SaleListComponent } from './features/sales/components/sale-list/sale-list.component';
import { SaleFormComponent } from './features/sales/components/sale-form/sale-form.component';
import { SaleDetailListComponent } from './features/sale-details/components/sale-detail-list/sale-detail-list.component';
import { SaleDetailFormComponent } from './features/sale-details/components/sale-detail-form/sale-detail-form.component';
import { CartComponent } from './features/cart/components/cart/cart.component';
import { adminGuard, clientGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },

  // Panel de Administración (Rol: Admin)
  {
    path: 'admin',
    canActivate: [adminGuard],
    children: [
      { path: '', redirectTo: 'products', pathMatch: 'full' },
      { path: 'clients', component: ClientListComponent },
      { path: 'clients/new', component: ClientFormComponent },
      { path: 'clients/edit/:id', component: ClientFormComponent },
      { path: 'products', component: ProductListComponent },
      { path: 'products/new', component: ProductFormComponent },
      { path: 'products/edit/:id', component: ProductFormComponent },
      { path: 'sales', component: SaleListComponent },
      { path: 'sales/new', component: SaleFormComponent },
      { path: 'sale-details', component: SaleDetailListComponent },
      { path: 'sale-details/new', component: SaleDetailFormComponent }
    ]
  },

  // Portal de Clientes (Rol: Client)
  {
    path: 'portal',
    canActivate: [clientGuard],
    children: [
      { path: '', redirectTo: 'products', pathMatch: 'full' },
      { path: 'products', component: ProductListComponent },
      { path: 'cart', component: CartComponent },
      { path: 'my-sales', component: SaleListComponent }
    ]
  },

  { path: '**', redirectTo: 'login' }
];
