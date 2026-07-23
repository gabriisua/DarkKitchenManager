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

import { MenuService } from '../../../core/services/menu.service';
import { UiService } from '../../../core/services/ui.service';

import {
  Menu,
  MenuPagedRequest
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
  selector: 'app-dash-menu',
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
  templateUrl: './menu-page.component.html'
})
export class MenuPageComponent {

  private menuService = inject(MenuService);
  private uiService = inject(UiService);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);

  readonly data = signal<Menu[]>([]);
  readonly totalItems = signal<number>(0);
  readonly loading = signal<boolean>(false);
  readonly error = signal<string | null>(null);

  searchTerm = '';

  filters = {
    name: '',
    isActive: null as boolean | null
  };

  columns: ColumnDef<Menu>[] = [
    {
      field: 'name',
      header: 'Nome',
      sortable: true
    },
    {
      field: 'description',
      header: 'Descrizione',
      sortable: true
    },
    {
      field: 'isActive',
      header: 'Stato',
      sortable: true,
      cellType: 'boolean'
    },
  ];

  readonly querySubject = new BehaviorSubject<MenuPagedRequest>({
    page: 1,
    pageSize: 10,
    search: '',
    name: '',
    isActive: null,
    sortColumn: 'name',
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

        switchMap(query => this.menuService.getPaged(query))
      )
      .subscribe({
        next: (res) => {
          this.data.set(res.items);
          this.totalItems.set(res.totalCount);
          this.loading.set(false);
        },

        error: (err) => {
          console.error('Errore caricamento menu:', err);

          this.loading.set(false);
          this.data.set([]);
          this.totalItems.set(0);

          const errorMsg =
            'Si è verificato un errore durante il caricamento dei menu.';

          this.error.set(errorMsg);

          this.uiService.showToast(errorMsg, 'error');
        }
      });
  }

  updateQuery(partialQuery: Partial<MenuPagedRequest>): void {

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
      name: this.filters.name || undefined,
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
    this.filters.name = '';
    this.filters.isActive = null;
    this.querySubject.next({
      page: 1,
      pageSize: this.querySubject.value.pageSize,
      search: '',
      name: '',
      isActive: null,
      sortColumn: 'name',
      sortDirection: 'asc'
    });
  }

  refreshTable(): void {
    this.updateQuery({});
  }

  addMenu(): void {
    this.router.navigate(['/menus/new']);
  }

  viewMenuDetails(row: Menu): void {
    this.router.navigate(['/menus', row.id]);
  }

  editMenu(row: Menu): void {
    this.router.navigate([
      '/menus',
      row.id,
      'edit'
    ]);
  }

  deleteMenu(row: Menu): void {

    const displayName =
      row.name;

    this.uiService.askConfirm(
      `Sei sicuro di voler eliminare il menu "${displayName}"?`,
      () => {

        this.uiService.showLoader(
          'Eliminazione menu in corso...'
        );

        this.menuService
          .delete(row.id)
          .subscribe({
            next: () => {

              this.uiService.hideLoader();

              this.uiService.showToast(
                'Menu eliminato correttamente.'
              );

              this.refreshTable();
            },

            error: (err) => {

              console.error(err);

              this.uiService.hideLoader();

              this.uiService.showToast(
                err.error?.message ||
                'Errore durante l\'eliminazione del menu.',
                'error'
              );
            }
          });
      },
      'Conferma Eliminazione'
    );
  }
}
