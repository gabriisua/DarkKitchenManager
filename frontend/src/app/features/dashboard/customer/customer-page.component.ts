import {
  Component,
  DestroyRef,
  inject,
  signal
} from '@angular/core';

import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';

import { BehaviorSubject, switchMap, tap } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { CustomerService } from '../../../core/services/customer.service';
import { UiService } from '../../../core/services/ui.service';

import {
  Customer,
  CustomerPagedRequest
} from '../../../shared/models/api.models';

import { DataGridComponent } from '../../../shared/data-grid/data-grid.component';
import {
  ColumnDef,
  PageChange,
  SortChange
} from '../../../shared/data-grid/data-grid.models';

import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';

@Component({
  selector: 'app-dash-customer',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,

    DataGridComponent,

    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatIconModule,
    MatButtonModule,
    MatTooltipModule
  ],
  templateUrl: './customer-page.component.html',
  styleUrls: ['./customer-page.component.css']
})
export class CustomerPageComponent {

  private customerService = inject(CustomerService);
  private uiService = inject(UiService);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);

  readonly data = signal<Customer[]>([]);
  readonly totalItems = signal<number>(0);
  readonly loading = signal<boolean>(false);
  readonly error = signal<string | null>(null);

  searchTerm = '';

  filters = {
    type: '',
    isActive: null as boolean | null
  };

  columns: ColumnDef<Customer>[] = [
    {
      field: 'businessName',
      header: 'Ragione Sociale',
      sortable: true
    },
    {
      field: 'email',
      header: 'Email',
      sortable: true
    },
    {
      field: 'type',
      header: 'Tipo',
      sortable: true,
      cellType: 'chip'
    },
    {
      field: 'contactPhone',
      header: 'Telefono'
    },
    {
      field: 'isActive',
      header: 'Stato',
      sortable: true,
      cellType: 'boolean'
    },
  ];

  readonly querySubject = new BehaviorSubject<CustomerPagedRequest>({
    page: 1,
    pageSize: 10,
    search: '',
    type: '',
    isActive: null,
    sortColumn: 'businessName',
    sortDirection: 'asc'
  });

  constructor() {
    this.loadData();
  }

  private loadData(): void {
    this.querySubject
      .pipe(
        takeUntilDestroyed(this.destroyRef),

        tap(() => {
          this.loading.set(true);
          this.error.set(null);
        }),

        switchMap(query => this.customerService.getPaged(query))
      )
      .subscribe({
        next: (res) => {
          this.data.set(res.items);
          this.totalItems.set(res.totalCount);
          this.loading.set(false);
        },

        error: (err) => {
          console.error('Errore caricamento clienti:', err);

          this.loading.set(false);
          this.data.set([]);
          this.totalItems.set(0);

          const errorMsg =
            'Si è verificato un errore durante il caricamento dei clienti.';

          this.error.set(errorMsg);

          this.uiService.showToast(errorMsg, 'error');
        }
      });
  }

  updateQuery(partialQuery: Partial<CustomerPagedRequest>): void {

    const current = this.querySubject.value;

    const isNavigation =
      'page' in partialQuery ||
      'pageSize' in partialQuery;

    this.querySubject.next({
      ...current,
      ...partialQuery,
      page: isNavigation
        ? (partialQuery.page ?? current.page)
        : 1
    });
  }

  onSearch(value: string): void {
    this.updateQuery({
      search: value
    });
  }

  onFilterChange(): void {
    this.updateQuery({
      type: this.filters.type || undefined,
      isActive: this.filters.isActive
    });
  }

  onSortChange(sort: SortChange): void {
    this.updateQuery({
      sortColumn: sort.column,
      sortDirection: sort.direction
    });
  }

  onPageChange(event: PageChange): void {
    this.updateQuery({
      page: event.page,
      pageSize: event.pageSize
    });
  }

  resetFilters(): void {
    this.searchTerm = '';
    this.filters.type = '';
    this.filters.isActive = null;
    this.querySubject.next({
      page: 1,
      pageSize: this.querySubject.value.pageSize,
      search: '',
      type: '',
      isActive: null,
      sortColumn: 'businessName',
      sortDirection: 'asc'
    });
  }

  refreshTable(): void {
    this.updateQuery({});
  }

  addCustomer(): void {
    this.router.navigate(['/customers/new']);
  }

  editCustomer(row: Customer): void {
    this.router.navigate([
      '/customers',
      row.id,
      'edit'
    ]);
  }

  deleteCustomer(row: Customer): void {

    const displayName =
      row.businessName ||
      row.email;

    this.uiService.askConfirm(
      `Sei sicuro di voler eliminare/disabilitare il cliente "${displayName}"?`,
      () => {

        this.uiService.showLoader(
          'Eliminazione cliente in corso...'
        );

        this.customerService
          .deleteCustomer(row.id)
          .subscribe({
            next: () => {

              this.uiService.hideLoader();

              this.uiService.showToast(
                'Cliente eliminato correttamente.'
              );

              this.refreshTable();
            },

            error: (err) => {

              console.error(err);

              this.uiService.hideLoader();

              this.uiService.showToast(
                err.error?.message ||
                'Errore durante l\'eliminazione del cliente.',
                'error'
              );
            }
          });
      },
      'Conferma Eliminazione'
    );
  }
}
