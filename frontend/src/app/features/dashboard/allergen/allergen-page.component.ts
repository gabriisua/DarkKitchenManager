import { Component, inject, signal, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BehaviorSubject, switchMap, tap } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { AllergenService } from '../../../core/services/allergen.service';
import { UiService } from '../../../core/services/ui.service';
import { Allergen, AllergenPagedRequest } from '../../../shared/models/api.models';
import { AllergenDialogComponent } from './allergen-dialog/allergen-dialog.component';

import { DataGridComponent } from '../../../shared/data-grid/data-grid.component';
import { ColumnDef, SortChange, PageChange } from '../../../shared/data-grid/data-grid.models';

import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatTooltip } from '@angular/material/tooltip';

@Component({
  selector: 'app-dash-allergen',
  standalone: true,
  imports: [
    CommonModule, FormsModule, DataGridComponent, MatFormFieldModule,
    MatInputModule, MatIconModule, MatButtonModule, MatDialogModule, MatTooltip
  ],
  templateUrl: './allergen-page.component.html'
})
export class AllergenPageComponent {
  private allergenService = inject(AllergenService);
  private uiService = inject(UiService);
  private dialog = inject(MatDialog);
  private destroyRef = inject(DestroyRef);

  columns: ColumnDef<Allergen>[] = [
    { field: 'name', header: 'Nome', sortable: true },
    { field: 'code', header: 'Codice', cellType: 'chip', sortable: true, width: '100px' },
    { field: 'description', header: 'Descrizione', sortable: false },
  ];

  data = signal<Allergen[]>([]);
  totalItems = signal(0);
  loading = signal(false);
  error = signal<string | null>(null);

  private querySubject = new BehaviorSubject<AllergenPagedRequest>({
    page: 1,
    pageSize: 10,
    sortColumn: 'name',
    sortDirection: 'asc'
  });

  filters = { search: '' };

  constructor() {
    this.querySubject.pipe(
      takeUntilDestroyed(this.destroyRef),
      tap(() => {
        this.loading.set(true);
        this.error.set(null);
      }),
      switchMap(query => this.allergenService.getPaged(query))
    ).subscribe({
      next: (res: any) => {
        this.loading.set(false);
        if (res.succeeded && res.data) {
          this.data.set(res.data.items);
          this.totalItems.set(res.data.totalCount);
        } else if (res.items) {
          this.data.set(res.items);
          this.totalItems.set(res.totalCount);
        } else {
          this.data.set([]);
          this.totalItems.set(0);
        }
      },
      error: (err) => {
        console.error('Errore caricamento allergeni:', err);
        const errorMsg = 'Errore durante il caricamento dei dati';
        this.error.set(errorMsg);
        this.loading.set(false);
        this.data.set([]);
        this.uiService.showToast(errorMsg, 'error');
      }
    });
  }

  onSortChange(sort: SortChange): void {
    this.updateQuery({ sortColumn: sort.column, sortDirection: sort.direction, page: 1 });
  }

  onPageChange(event: PageChange): void {
    this.updateQuery({ page: event.page, pageSize: event.pageSize });
  }
  hasActiveFilters(): boolean {
    return !!this.filters.search && this.filters.search.trim().length > 0;
  }

  onSearch(): void {
    this.updateQuery({ search: this.filters.search, page: 1 });
  }

  clearFilters(): void {
    this.filters.search = '';
    this.updateQuery({ search: '', page: 1, pageSize: 10 });
  }
  refreshTable(): void {
    this.querySubject.next({ ...this.querySubject.value });
  }

  private updateQuery(partial: Partial<AllergenPagedRequest>): void {
    const current = this.querySubject.value;
    const isNavigation = 'page' in partial || 'pageSize' in partial;
    this.querySubject.next({
      ...current,
      ...partial,
      page: isNavigation ? (partial.page ?? current.page) : 1
    });
  }

  openCreateDialog(): void {
    const dialogRef = this.dialog.open(AllergenDialogComponent, { width: '400px', disableClose: true, data: null });
    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.uiService.showToast('Allergene creato con successo!');
        this.refreshTable();
      }
    });
  }

  editAllergen(row: Allergen): void {
    const dialogRef = this.dialog.open(AllergenDialogComponent, { width: '400px', disableClose: true, data: row });
    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.uiService.showToast('Allergene modificato con successo!');
        this.refreshTable();
      }
    });
  }

  deleteAllergen(row: Allergen): void {
    this.uiService.askConfirm(
      `Sei sicuro di voler eliminare l'allergene "${row.name}"?`,
      () => {
        this.uiService.showLoader('Eliminazione in corso...');
        this.allergenService.delete(row.id).subscribe({
          next: () => {
            this.uiService.hideLoader();
            this.refreshTable();
            this.uiService.showToast('Allergene eliminato correttamente.');
          },
          error: (err) => {
            console.error(err);
            this.uiService.hideLoader();
            this.uiService.showToast(err.error?.message || 'Errore durante l\'eliminazione.', 'error');
          }
        });
      },
      'Conferma Eliminazione'
    );
  }
}
