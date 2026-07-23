import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { LayoutComponent} from './layout/layout.component';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./features/auth/login/login.component')
        .then(m => m.LoginComponent)
  },
  {
    path: 'forgot-password',
    loadComponent: () =>
      import('./features/auth/forgot-password/forgot-password.component')
        .then(m => m.ForgotPasswordComponent)
  },
  {
    path: 'reset-password',
    loadComponent: () =>
      import('./features/auth/reset-password/reset-password.component')
        .then(m => m.ResetPasswordComponent)
  },
  {
    path: '',
    component: LayoutComponent,
    canMatch: [authGuard],
    children: [
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./features/dashboard/dashboard.component')
            .then(m => m.DashboardComponent)
      },
      {
        path: 'staff',
        loadComponent: () =>
          import('./features/dashboard/staff/staff-page.component')
            .then(m => m.StaffPageComponent)
      },
      {
        path: 'customers',
        loadComponent: () =>
          import('./features/dashboard/customer/customer-page.component')
            .then(m => m.CustomerPageComponent)
      },
      {
        path: 'customers/new',
        loadComponent: () =>
          import('./features/dashboard/customer/customer-form/customer-form.component')
            .then(m => m.CustomerFormComponent)
      },
      {
        path: 'customers/:id/edit',
        loadComponent: () =>
          import('./features/dashboard/customer/customer-form/customer-form.component')
            .then(m => m.CustomerFormComponent)
      },
      {
        path: 'allergens',
        loadComponent: () =>
          import('./features/dashboard/allergen/allergen-page.component')
            .then(m => m.AllergenPageComponent)
      },
      {
        path: 'ingredients',
        loadComponent: () =>
          import('./features/dashboard/ingredient/ingredient-page.component')
            .then(m => m.IngredientPageComponent)
      },
      {
        path: 'menus',
        loadComponent: () =>
          import('./features/dashboard/menu/menu-page.component')
            .then(m => m.MenuPageComponent)
      },
      {
        path: 'menus/new',
        loadComponent: () =>
          import('./features/dashboard/menu/menu-form/menu-form.component')
            .then(m => m.MenuFormComponent)
      },
      {
        path: 'menus/:id/edit',
        loadComponent: () =>
          import('./features/dashboard/menu/menu-form/menu-form.component')
            .then(m => m.MenuFormComponent)
      },
      {
        path: 'menus/:id',
        loadComponent: () =>
          import('./features/dashboard/menu/menu-detail/menu-detail.component')
            .then(m => m.MenuDetailComponent)
      },
      // --- INIZIO BLOCCO PLATES RIPARATO ---
      {
        path: 'plates',
        loadComponent: () =>
          import('./features/dashboard/plate/plate-page.component')
            .then(m => m.PlatePageComponent) // Ripristinata la list view
      },
      {
        path: 'plates/new',
        loadComponent: () =>
          import('./features/dashboard/plate/plate-form/plate-form.component')
            .then(m => m.PlateFormComponent) // "new" messo rigorosamente PRIMA di ":id"
      },
      {
        path: 'plates/:id/edit',
        loadComponent: () =>
          import('./features/dashboard/plate/plate-form/plate-form.component')
            .then(m => m.PlateFormComponent)
      },
      {
        path: 'plates/:id',
        loadComponent: () =>
          import('./features/dashboard/plate/plate-detail/plate-detail.component')
            .then(m => m.PlateDetailComponent)
      },
      // --- FINE BLOCCO PLATES ---
      {
        path: 'sales',
        loadComponent: () =>
          import('./features/dashboard/sale/sale.component')
            .then(m => m.SaleComponent)
      },
      {
        path: 'categories',
        loadComponent: () =>
          import('./features/dashboard/category/category.component')
            .then(m => m.CategoryComponent)
      },
      {
        path: 'orders',
        loadComponent: () =>
          import('./features/dashboard/order/order-page.component')
            .then(m => m.OrderPageComponent)
      },
      {
        path: 'orders/create',
        loadComponent: () =>
          import('./features/dashboard/order/order-create/order-create.component')
            .then(m => m.OrderCreateComponent) // Stessa regola del "new": messo prima di ":id"
      },
      {
        path: 'orders/:id',
        loadComponent: () =>
          import('./features/dashboard/order/order-details/order-details.component')
            .then(m => m.OrderDetailsComponent)
      },
      {
        path: 'invoices/history',
        loadComponent: () =>
          import('./features/dashboard/invoice/invoice-history/invoice-history.component')
            .then(m => m.InvoiceHistoryComponent)
      },
      {
        path: 'invoices',
        loadComponent: () =>
          import('./features/dashboard/invoice/invoice-page.component')
            .then(m => m.InvoicePageComponent)
      }
    ]
  },
  { path: '**', redirectTo: '' }
];
