import { Component, inject, signal, DestroyRef, TemplateRef, ViewChild, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BehaviorSubject, switchMap, tap } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

// Servizi
import { SaleService } from '../../../core/services/sale.service';
import { UiService } from '../../../core/services/ui.service';

// Modelli e Grid
import { PlateDiscountDto, CategoryDiscountDto, DiscountPagedRequest } from '../../../shared/models/api.models';
import { DataGridComponent } from '../../../shared/data-grid/data-grid.component';
import { ColumnDef, SortChange, PageChange } from '../../../shared/data-grid/data-grid.models';
import { SalePlateFormComponent } from './sale-plate-form/sale-plate-form.component';
import { SaleCategoryFormComponent } from './sale-category-form/sale-category-form.component';

// Material
import { MatTabsModule } from '@angular/material/tabs';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select'; // <-- Aggiunto per le select dei filtri
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';

@Component({
  selector: 'app-sale-global',
  standalone: true,
  imports: [
    CommonModule, FormsModule, DataGridComponent,
    MatTabsModule, MatFormFieldModule, MatInputModule, MatSelectModule,
    MatIconModule, MatButtonModule, MatTooltipModule, MatDialogModule
  ],
  templateUrl: './sale.component.html',
  styles: [`
    .page-container { padding: 24px; }
    .toolbar-row { display: flex; gap: 16px; align-items: center; flex-wrap: wrap; margin-bottom: 16px; margin-top: 16px; }
    .search-field { width: 300px; }
    .spacer { flex: 1 1 auto; }
    .action-buttons-container { display: flex; gap: 8px; justify-content: center; }
  `]
})
export class SaleComponent implements OnInit {
  private saleService = inject(SaleService);
  private uiService = inject(UiService);
  private destroyRef = inject(DestroyRef);
  private dialog = inject(MatDialog);

  @ViewChild('plateActionsTemplate', { static: true }) plateActionsTemplate!: TemplateRef<any>;
  @ViewChild('categoryActionsTemplate', { static: true }) categoryActionsTemplate!: TemplateRef<any>;
  @ViewChild('priceTemplate', { static: true }) priceTemplate!: TemplateRef<any>;
  @ViewChild('percentageTemplate', { static: true }) percentageTemplate!: TemplateRef<any>;

  // ── DEFINIZIONE COLONNE ──
  plateColumns: ColumnDef<PlateDiscountDto>[] = [];
  categoryColumns: ColumnDef<CategoryDiscountDto>[] = [];

  // ── STATO PIATTI ──
  readonly plateData = signal<PlateDiscountDto[]>([]);
  readonly plateTotal = signal<number>(0);
  readonly plateLoading = signal<boolean>(false);

  plateSearchTerm = '';
  plateIsActiveFilter: boolean | null = null; // Filtro stato

  readonly plateQuery = new BehaviorSubject<DiscountPagedRequest>({
    page: 1, pageSize: 10, sortColumn: 'customer', sortDirection: 'asc'
  });

  // ── STATO CATEGORIE ──
  readonly categoryData = signal<CategoryDiscountDto[]>([]);
  readonly categoryTotal = signal<number>(0);
  readonly categoryLoading = signal<boolean>(false);

  categorySearchTerm = '';
  categoryIsActiveFilter: boolean | null = null; // Filtro stato

  readonly categoryQuery = new BehaviorSubject<DiscountPagedRequest>({
    page: 1, pageSize: 10, sortColumn: 'customer', sortDirection: 'asc'
  });

  ngOnInit() {
    this.initColumns();
    this.loadPlateData();
    this.loadCategoryData();
  }

  private initColumns() {
    this.plateColumns = [
      { field: 'businessName', header: 'Cliente', sortable: true },
      { field: 'plateName', header: 'Piatto', sortable: true },
      { field: 'overridePrice', header: 'Prezzo Fissato', sortable: true, customTemplateRef: 'priceTemplate' },
      { field: 'validFrom', header: 'Inizio Validità', cellType: 'date', sortable: true },
      { field: 'validTo', header: 'Fine Validità', cellType: 'date', sortable: true },
      { field: 'isActive', header: 'Stato', cellType: 'boolean', sortable: false }
      // RIMOSSO: { field: 'customerId', header: 'Azioni'... }
    ];

    this.categoryColumns = [
      { field: 'businessName', header: 'Cliente', sortable: true },
      { field: 'categoryName', header: 'Categoria', sortable: true },
      { field: 'discountPercentage', header: 'Sconto (%)', sortable: true, customTemplateRef: 'percentageTemplate' },
      { field: 'validFrom', header: 'Inizio Validità', cellType: 'date', sortable: true },
      { field: 'validTo', header: 'Fine Validità', cellType: 'date', sortable: true },
      { field: 'isActive', header: 'Stato', cellType: 'boolean', sortable: false }
    ];
  }

  // ==========================================
  // --- CARICAMENTO DATI (RxJS Pipes) ---
  // ==========================================

  private loadPlateData(): void {
    this.plateQuery.pipe(
      takeUntilDestroyed(this.destroyRef),
      tap(() => this.plateLoading.set(true)),
      switchMap(query => this.saleService.getPagedPlateDiscounts(query))
    ).subscribe({
      next: (res) => {
        this.plateData.set(res.items);
        this.plateTotal.set(res.totalCount);
        this.plateLoading.set(false);
      },
      error: (err) => {
        this.plateLoading.set(false);
        this.uiService.showToast('Errore caricamento sconti piatti.', 'error');
      }
    });
  }

  private loadCategoryData(): void {
    this.categoryQuery.pipe(
      takeUntilDestroyed(this.destroyRef),
      tap(() => this.categoryLoading.set(true)),
      switchMap(query => this.saleService.getPagedCategoryDiscounts(query))
    ).subscribe({
      next: (res) => {
        this.categoryData.set(res.items);
        this.categoryTotal.set(res.totalCount);
        this.categoryLoading.set(false);
      },
      error: (err) => {
        this.categoryLoading.set(false);
        this.uiService.showToast('Errore caricamento sconti categorie.', 'error');
      }
    });
  }

  // ==========================================
  // --- HANDLERS EVENTI GRID ---
  // ==========================================

  // Piatti
  onPlateSearch(value: string): void {
    this.plateQuery.next({ ...this.plateQuery.value, search: value, page: 1 });
  }

  onPlateFilterChange(): void {
    this.plateQuery.next({ ...this.plateQuery.value, isActive: this.plateIsActiveFilter, page: 1 });
  }

  onPlateSortChange(sort: SortChange): void {
    // Mappatura delle colonne per il backend
    let mappedCol = sort.column;
    if (sort.column === 'businessName') mappedCol = 'customer';
    if (sort.column === 'plateName') mappedCol = 'plate';
    if (sort.column === 'overridePrice') mappedCol = 'price';

    this.plateQuery.next({ ...this.plateQuery.value, sortColumn: mappedCol, sortDirection: sort.direction, page: 1 });
  }

  onPlatePageChange(event: PageChange): void {
    this.plateQuery.next({ ...this.plateQuery.value, page: event.page + 1, pageSize: event.pageSize });
  }

  // Categorie
  onCategorySearch(value: string): void {
    this.categoryQuery.next({ ...this.categoryQuery.value, search: value, page: 1 });
  }

  onCategoryFilterChange(): void {
    this.categoryQuery.next({ ...this.categoryQuery.value, isActive: this.categoryIsActiveFilter, page: 1 });
  }

  onCategorySortChange(sort: SortChange): void {
    // Mappatura delle colonne per il backend
    let mappedCol = sort.column;
    if (sort.column === 'businessName') mappedCol = 'customer';
    if (sort.column === 'categoryName') mappedCol = 'category';
    if (sort.column === 'discountPercentage') mappedCol = 'discount';

    this.categoryQuery.next({ ...this.categoryQuery.value, sortColumn: mappedCol, sortDirection: sort.direction, page: 1 });
  }

  onCategoryPageChange(event: PageChange): void {
    this.categoryQuery.next({ ...this.categoryQuery.value, page: event.page + 1, pageSize: event.pageSize });
  }

  // ==========================================
  // --- AZIONI (CRUD) ---
  // ==========================================

  openPlateDiscountModal(row?: PlateDiscountDto): void {
    const dialogRef = this.dialog.open(SalePlateFormComponent, {
      width: '600px',
      disableClose: true, // Impedisce la chiusura accidentale cliccando fuori dalla modale
      data: row || null
    });

    dialogRef.afterClosed().subscribe((saved: boolean) => {
      if (saved) {
        // Se la modale restituisce true (salvataggio completato), ricarica la tabella
        // mantenendo la pagina e i filtri attuali intatti.
        this.plateQuery.next({ ...this.plateQuery.value });
      }
    });
  }

  openCategoryDiscountModal(row?: CategoryDiscountDto): void {
    const dialogRef = this.dialog.open(SaleCategoryFormComponent, {
      width: '600px',
      disableClose: true,
      data: row || null
    });

    dialogRef.afterClosed().subscribe((saved: boolean) => {
      if (saved) {
        // Ricarica la tabella categorie
        this.categoryQuery.next({ ...this.categoryQuery.value });
      }
    });
  }

  deletePlateDiscount(row: PlateDiscountDto): void {
    this.uiService.askConfirm(
      `Sei sicuro di voler rimuovere il prezzo speciale di "${row.plateName}" per il cliente ${row.businessName}?`,
      () => {
        this.uiService.showLoader('Eliminazione in corso...');
        this.saleService.deletePlateDiscount(row.customerId, row.plateId).subscribe({
          next: () => {
            this.uiService.hideLoader();
            this.uiService.showToast('Sconto piatto eliminato definitivamente.');
            this.plateQuery.next({ ...this.plateQuery.value }); // Refresh
          },
          error: (err) => {
            this.uiService.hideLoader();
            this.uiService.showToast(err.error?.message || 'Errore durante l\'eliminazione.', 'error');
          }
        });
      },
      'Conferma Eliminazione'
    );
  }

  deleteCategoryDiscount(row: CategoryDiscountDto): void {
    this.uiService.askConfirm(
      `Sei sicuro di voler rimuovere lo sconto sulla categoria "${row.categoryName}" per il cliente ${row.businessName}?`,
      () => {
        this.uiService.showLoader('Eliminazione in corso...');
        this.saleService.deleteCategoryDiscount(row.customerId, row.categoryId).subscribe({
          next: () => {
            this.uiService.hideLoader();
            this.uiService.showToast('Sconto categoria eliminato definitivamente.');
            this.categoryQuery.next({ ...this.categoryQuery.value }); // Refresh
          },
          error: (err) => {
            this.uiService.hideLoader();
            this.uiService.showToast(err.error?.message || 'Errore durante l\'eliminazione.', 'error');
          }
        });
      },
      'Conferma Eliminazione'
    );
  }
}
