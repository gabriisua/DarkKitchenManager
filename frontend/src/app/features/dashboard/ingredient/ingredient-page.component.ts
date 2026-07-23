import { Component, inject, signal, DestroyRef, TemplateRef, ViewChild, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BehaviorSubject, switchMap, tap } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { IngredientService } from '../../../core/services/ingredient.service';
import { UiService } from '../../../core/services/ui.service';

import { Ingredient, IngredientPagedRequest } from '../../../shared/models/api.models';
import { DataGridComponent } from '../../../shared/data-grid/data-grid.component';
import { ColumnDef, SortChange, PageChange } from '../../../shared/data-grid/data-grid.models';

import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import {IngredientDialogComponent} from './ingredient-dialog/ingredient-dialog.component';

@Component({
  selector: 'app-dash-ingredient',
  standalone: true,
  imports: [
    CommonModule, FormsModule, DataGridComponent,
    MatFormFieldModule, MatInputModule, MatSlideToggleModule,
    MatIconModule, MatButtonModule, MatTooltipModule, MatDialogModule
  ],
  templateUrl: './ingredient-page.component.html'
})
export class IngredientPageComponent implements OnInit {
  private ingredientService = inject(IngredientService);
  private uiService = inject(UiService);
  private destroyRef = inject(DestroyRef);
  private dialog = inject(MatDialog);

  @ViewChild('actionsTemplate', { static: true }) actionsTemplate!: TemplateRef<any>;

  columns: ColumnDef<Ingredient>[] = [];

  // Reactive state
  readonly data = signal<Ingredient[]>([]);
  readonly totalItems = signal<number>(0);
  readonly loading = signal<boolean>(false);
  readonly error = signal<string | null>(null);

  // Filter model
  searchTerm = '';
  filters = {
    minEnergyKcal: null as number | null,
    maxEnergyKcal: null as number | null,
    minCost: null as number | null,
    maxCost: null as number | null,
  };

  // Query state
  readonly querySubject = new BehaviorSubject<IngredientPagedRequest>({
    page: 1,
    pageSize: 10,
    sortColumn: 'name',
    sortDirection: 'asc'
  });

  ngOnInit() {
    this.columns = [
      { field: 'name', header: 'Nome', sortable: true },
      { field: 'energyKcalPer100g', header: 'Kcal/100g', cellType: 'text', sortable: true, width: '100px' },
      { field: 'costPer1000g', header: 'Costo/kg', cellType: 'currency', sortable: true, width: '120px' },
    ];

    this.loadData();
  }

  // ==========================================
  // Caricamento Dati con Gestione Errori UiService
  // ==========================================
  private loadData(): void {
    this.querySubject.pipe(
      takeUntilDestroyed(this.destroyRef),
      tap(() => {
        this.loading.set(true);
        this.error.set(null);
      }),
      switchMap(query => this.ingredientService.getPaged(query))
    ).subscribe({
      next: (res) => {
        this.data.set(res.items);
        this.totalItems.set(res.totalCount);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Errore caricamento ingredienti:', err);

        this.loading.set(false);
        this.data.set([]);
        this.totalItems.set(0);

        const errorMsg = 'Si è verificato un errore durante il caricamento degli ingredienti.';
        this.error.set(errorMsg);

        // Integrazione UiService per errore caricamento
        this.uiService.showToast(errorMsg, 'error');
      }
    });
  }

  // ==========================================
  // Metodi di Gestione Tabella e Filtri
  // ==========================================
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
    const update: Partial<IngredientPagedRequest> & { page: number } = { page: 1 };

    if (this.filters.minEnergyKcal !== null || this.filters.maxEnergyKcal !== null) {
      update.minEnergyKcal = this.filters.minEnergyKcal ?? undefined;
      update.maxEnergyKcal = this.filters.maxEnergyKcal ?? undefined;
    } else {
      update.minEnergyKcal = undefined;
      update.maxEnergyKcal = undefined;
    }

    if (this.filters.minCost !== null || this.filters.maxCost !== null) {
      update.minCost = this.filters.minCost ?? undefined;
      update.maxCost = this.filters.maxCost ?? undefined;
    } else {
      update.minCost = undefined;
      update.maxCost = undefined;
    }
    this.updateQuery(update);
  }

  resetFilters(): void {
    this.searchTerm = '';
    this.filters.minEnergyKcal = null;
    this.filters.maxEnergyKcal = null;
    this.filters.minCost = null;
    this.filters.maxCost = null;
    this.querySubject.next({
      page: 1,
      pageSize: this.querySubject.value.pageSize,
      search: '',
      sortColumn: this.querySubject.value.sortColumn,
      sortDirection: this.querySubject.value.sortDirection,
      minEnergyKcal: undefined,
      maxEnergyKcal: undefined,
      minCost: undefined,
      maxCost: undefined,
    });
  }

  refreshTable(): void {
    this.querySubject.next({ ...this.querySubject.value });
  }

  private updateQuery(partial: Partial<IngredientPagedRequest>): void {
    this.querySubject.next({ ...this.querySubject.value, ...partial });
  }

  // ==========================================
  // Azioni CRUD
  // ==========================================

  // Eliminazione con UiService (Conferma, Loader, Toast)
  deleteIngredient(row: Ingredient): void {
    this.uiService.askConfirm(
      `Sei sicuro di voler eliminare l'ingrediente "${row.name}"?`,
      () => {
        this.uiService.showLoader('Eliminazione ingrediente in corso...');

        this.ingredientService.delete(row.id).subscribe({
          next: () => {
            this.uiService.hideLoader();
            this.uiService.showToast('Ingrediente eliminato correttamente.');
            this.refreshTable();
          },
          error: (err) => {
            console.error(err);
            this.uiService.hideLoader();
            this.uiService.showToast(
              err.error?.message || 'Errore durante l\'eliminazione dell\'ingrediente.',
              'error'
            );
          }
        });
      },
      'Conferma Eliminazione'
    );
  }
  // ==========================================
  // Azioni CRUD (con MatDialog e UiService)
  // ==========================================
  addIngredient(): void {

    const dialogRef = this.dialog.open(IngredientDialogComponent, {
      width: '600px'
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.uiService.showLoader('Creazione ingrediente in corso...');

        this.ingredientService.create(result).subscribe({
          next: () => {
            this.uiService.hideLoader();
            this.uiService.showToast('Ingrediente creato con successo.');
            this.refreshTable();
          },
          error: (err) => {
            console.error(err);
            this.uiService.hideLoader();
            this.uiService.showToast(
              err.error?.message || 'Errore durante la creazione dell\'ingrediente.',
              'error'
            );
          }
        });
      }
    });
  }

  editIngredient(row: Ingredient): void {

    const dialogRef = this.dialog.open(IngredientDialogComponent, {
      width: '600px',
      data: row
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.uiService.showLoader('Aggiornamento ingrediente in corso...');

        this.ingredientService.update(row.id, result).subscribe({
          next: () => {
            this.uiService.hideLoader();
            this.uiService.showToast('Ingrediente aggiornato con successo.');
            this.refreshTable();
          },
          error: (err) => {
            console.error(err);
            this.uiService.hideLoader();
            this.uiService.showToast(
              err.error?.message || 'Errore durante l\'aggiornamento dell\'ingrediente.',
              'error'
            );
          }
        });
      }
    });
  }
}
