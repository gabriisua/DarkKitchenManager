import { Component, inject, signal, DestroyRef, TemplateRef, ViewChild, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BehaviorSubject, switchMap, tap } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { PlateService } from '../../../core/services/plate.service';
import { UiService } from '../../../core/services/ui.service';

import { Plate, PlatePagedRequest } from '../../../shared/models/api.models';
import { DataGridComponent } from '../../../shared/data-grid/data-grid.component';
import { ColumnDef, SortChange, PageChange } from '../../../shared/data-grid/data-grid.models';

import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialogModule } from '@angular/material/dialog';
import { Router } from '@angular/router';
import { MatDivider } from '@angular/material/list';

const CATEGORIES = [
  { id: 0, name: 'Antipasti' },
  { id: 1, name: 'Primi' },
  { id: 2, name: 'Secondi' },
  { id: 3, name: 'Contorni' },
  { id: 4, name: 'Dolci' },
  { id: 5, name: 'Bevande' },
];

const LINE_TYPES = [
  { value: 0, label: 'Standard' },
  { value: 1, label: 'Gourmet' },
  { value: 2, label: 'Vegetale' },
  { value: 3, label: 'Fitness' },
  { value: 4, label: 'Planted' }
];

@Component({
  selector: 'app-dash-plate',
  standalone: true,
  imports: [
    CommonModule, FormsModule, DataGridComponent,
    MatFormFieldModule, MatInputModule, MatSelectModule, MatSlideToggleModule,
    MatIconModule, MatButtonModule, MatTooltipModule, MatDialogModule, MatDivider
  ],
  templateUrl: './plate-page.component.html'
})
export class PlatePageComponent implements OnInit {
  private plateService = inject(PlateService);
  private uiService = inject(UiService);
  private destroyRef = inject(DestroyRef);
  private router = inject(Router);

  @ViewChild('actionsTemplate', { static: true }) actionsTemplate!: TemplateRef<any>;

  readonly categories = CATEGORIES;
  readonly lineTypes = LINE_TYPES;
  columns: ColumnDef<Plate>[] = [];

  readonly data = signal<Plate[]>([]);
  readonly totalItems = signal<number>(0);
  readonly loading = signal<boolean>(false);
  readonly error = signal<string | null>(null);

  searchTerm = '';
  filters = {
    categoryId: null as number | null,
    lineType: null as number | null,
    isActive: null as boolean | null,
    minPrice: null as number | null,
    maxPrice: null as number | null,
  };

  readonly querySubject = new BehaviorSubject<PlatePagedRequest>({
    page: 1,
    pageSize: 10,
    sortColumn: 'name',
    sortDirection: 'asc'
  });

  ngOnInit() {
    this.columns = [
      {
        field: 'id',
        header: 'Codice Piatto',
        cellType: 'actions',
        sortable: false,
        width: '120px',
        customTemplateRef: 'actionsTemplate'
      },
      {
        field: 'name',
        header: 'Nome',
        sortable: true
      },
      {
        field: 'categoryName',
        header: 'Categoria',
        cellType: 'text',
        sortable: true,
        width: '120px'
      },
      {
        field: 'basePrice',
        header: 'Prezzo Base',
        cellType: 'currency',
        sortable: true,
        width: '120px'
      },
    ];

    this.loadData();
  }

  addPlate(): void {
    this.router.navigate(['/plates/new']);
  }

  viewPlateDetail(row: Plate): void {
    this.router.navigate(['/plates', row.id]);
  }

  editPlate(row: Plate): void {
    this.router.navigate(['/plates', row.id, 'edit']);
  }

  private loadData(): void {
    this.querySubject.pipe(
      takeUntilDestroyed(this.destroyRef),
      tap(() => {
        this.loading.set(true);
        this.error.set(null);
      }),
      switchMap(query => this.plateService.getPaged(query))
    ).subscribe({
      next: (res: any) => {
        const mappedItems = res.items.map((item: any) => ({
          ...item,
          basePrice: item.basePrice / 100
        }));

        this.data.set(mappedItems);
        this.totalItems.set(res.totalCount);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Errore caricamento piatti:', err);
        this.loading.set(false);
        this.data.set([]);
        this.totalItems.set(0);

        const errorMsg = 'Si è verificato un errore durante il caricamento dei piatti.';
        this.error.set(errorMsg);
        this.uiService.showToast(errorMsg, 'error');
      }
    });
  }

  onSortChange(sort: SortChange): void {
    this.updateQuery({ sortColumn: sort.column, sortDirection: sort.direction, page: 1 });
  }

  onPageChange(event: PageChange): void {
    this.updateQuery({ page: event.page + 1, pageSize: event.pageSize });
  }

  onSearch(value: string): void {
    this.updateQuery({ search: value, page: 1 });
  }

  onFilterChange(): void {
    const update: Partial<PlatePagedRequest> & { page: number } = {
      page: 1,
      isActive: this.filters.isActive ?? undefined,
      categoryId: this.filters.categoryId ?? undefined,
      lineType: this.filters.lineType ?? undefined,
      minPrice: this.filters.minPrice ?? undefined,
      maxPrice: this.filters.maxPrice ?? undefined,
    };
    this.updateQuery(update);
  }

  resetFilters(): void {
    this.searchTerm = '';
    this.filters.categoryId = null;
    this.filters.lineType = null;
    this.filters.isActive = null;
    this.filters.minPrice = null;
    this.filters.maxPrice = null;
    this.querySubject.next({
      page: 1,
      pageSize: this.querySubject.value.pageSize,
      search: '',
      categoryId: undefined,
      lineType: undefined,
      isActive: undefined,
      minPrice: undefined,
      maxPrice: undefined,
      sortColumn: 'name',
      sortDirection: 'asc'
    });
  }

  refreshTable(): void {
    this.querySubject.next({ ...this.querySubject.value });
  }

  private updateQuery(partial: Partial<PlatePagedRequest>): void {
    this.querySubject.next({ ...this.querySubject.value, ...partial });
  }

  deletePlate(row: Plate): void {
    this.uiService.askConfirm(
      `Sei sicuro di voler eliminare il piatto "${row.name}"?`,
      () => {
        this.uiService.showLoader('Eliminazione piatto in corso...');

        this.plateService.delete(row.id).subscribe({
          next: () => {
            this.uiService.hideLoader();
            this.uiService.showToast('Piatto eliminato correttamente.');
            this.refreshTable();
          },
          error: (err) => {
            console.error(err);
            this.uiService.hideLoader();
            this.uiService.showToast(
              err.error?.message || 'Errore durante l\'eliminazione del piatto.',
              'error'
            );
          }
        });
      },
      'Conferma Eliminazione'
    );
  }
}
