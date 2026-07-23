import { Component, OnInit, inject, signal, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BehaviorSubject, switchMap, tap } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

// Material
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatTooltipModule } from '@angular/material/tooltip';

// Servizi e Modelli
import { CategoryService } from '../../../core/services/category.service';
import { UiService } from '../../../core/services/ui.service';
import { Category, CategoryPagedRequest } from '../../../shared/models/api.models';
import { DataGridComponent } from '../../../shared/data-grid/data-grid.component';
import { ColumnDef, PageChange, SortChange } from '../../../shared/data-grid/data-grid.models';

// Importa il form della modale (Verifica che il percorso sia corretto nel tuo progetto)
import { CategoryFormComponent } from './category-form/category-form.component';

@Component({
  selector: 'app-category-page',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    DataGridComponent,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDialogModule,
    MatTooltipModule
  ],
  templateUrl: './category.component.html'
})
export class CategoryComponent implements OnInit {
  private categoryService = inject(CategoryService);
  private uiService = inject(UiService);
  private destroyRef = inject(DestroyRef);
  private dialog = inject(MatDialog);

  // Definizione delle colonne per la grid
  columns: ColumnDef<Category>[] = [
    { field: 'id', header: 'ID', sortable: true, width: '80px' },
    { field: 'name', header: 'Nome Categoria', sortable: true },
    { field: 'isActive', header: 'Stato', cellType: 'boolean', sortable: false }
  ];

  // Stato reattivo
  data = signal<Category[]>([]);
  total = signal<number>(0);
  loading = signal<boolean>(false);

  // Filtri UI
  searchTerm = '';
  isActiveFilter: boolean | null = null;

  // RxJS Subject per triggerare le ricariche della tabella
  query = new BehaviorSubject<CategoryPagedRequest>({
    page: 1,
    pageSize: 10,
    sortColumn: 'name',
    sortDirection: 'asc'
  });

  ngOnInit() {
    this.loadData();
  }

  private loadData(): void {
    this.query.pipe(
      takeUntilDestroyed(this.destroyRef),
      tap(() => this.loading.set(true)),
      switchMap(q => this.categoryService.getPaged(q))
    ).subscribe({
      next: (res) => {
        this.data.set(res.items);
        this.total.set(res.totalCount);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.uiService.showToast('Errore nel caricamento delle categorie.', 'error');
      }
    });
  }

  // Eventi della Toolbar e della Grid
  onSearch(value: string): void {
    this.query.next({ ...this.query.value, search: value, page: 1 });
  }

  onFilterChange(): void {
    this.query.next({ ...this.query.value, isActive: this.isActiveFilter, page: 1 });
  }

  hasActiveFilters(): boolean {
    return !!this.searchTerm || this.isActiveFilter !== null;
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.isActiveFilter = null;
    this.query.next({
      page: 1,
      pageSize: this.query.value.pageSize,
      sortColumn: this.query.value.sortColumn,
      sortDirection: this.query.value.sortDirection,
      search: '',
      isActive: undefined,
    });
  }

  onSortChange(sort: SortChange): void {
    this.query.next({
      ...this.query.value,
      sortColumn: sort.column,
      sortDirection: sort.direction,
      page: 1
    });
  }

  onPageChange(event: PageChange): void {
    this.query.next({
      ...this.query.value,
      page: event.page + 1,
      pageSize: event.pageSize
    });
  }

  // Azioni CRUD
  openModal(row?: Category): void {
    const dialogRef = this.dialog.open(CategoryFormComponent, {
      width: '500px',
      disableClose: true,
      data: row || null
    });

    dialogRef.afterClosed().subscribe((saved: boolean) => {
      if (saved) {
        // Ricarica la tabella mantenendo i filtri attuali
        this.query.next({ ...this.query.value });
      }
    });
  }

  deleteCategory(row: Category): void {
    this.uiService.askConfirm(
      `Sei sicuro di voler eliminare la categoria "${row.name}"?`,
      () => {
        this.uiService.showLoader('Eliminazione...');
        this.categoryService.delete(row.id).subscribe({
          next: () => {
            this.uiService.hideLoader();
            this.uiService.showToast('Categoria eliminata con successo.');
            this.query.next({ ...this.query.value });
          },
          error: (err) => {
            this.uiService.hideLoader();

            // 1. Chiudiamo FORZATAMENTE qualsiasi modale/dialog di Material aperta
            // Questo distrugge il backdrop di Material e ridà il focus alla pagina!
            this.dialog.closeAll();

            // 2. Estraiamo il messaggio reale del backend
            const errorMsg = err.error?.message || 'Impossibile completare l\'operazione.';

            // 3. Ora il tuo overlay comparirà in cima a tutto senza rivali!
            this.uiService.showAlert('Azione Non Consentita', errorMsg);
          }
        });
      },
      'Conferma Eliminazione'
    );
  }
}
