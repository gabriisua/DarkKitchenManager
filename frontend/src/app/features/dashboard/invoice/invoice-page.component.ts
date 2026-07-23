import { Component, DestroyRef, computed, inject, signal, ViewChild } from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { BehaviorSubject, forkJoin, switchMap, tap } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { InvoiceService } from '../../../core/services/invoice.service';
import { UiService } from '../../../core/services/ui.service';
import {
  PendingInvoiceSummary,
  PendingOrderItem,
  InvoicePendingSummaryRequest
} from '../../../shared/models/api.models';
import { MatTable, MatTableModule } from '@angular/material/table';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatTooltipModule } from '@angular/material/tooltip';

@Component({
  selector: 'app-invoice-page',
  standalone: true,
  imports: [
    CommonModule, FormsModule, RouterModule, CurrencyPipe, DatePipe,
    MatTableModule, MatCheckboxModule, MatPaginatorModule,
    MatProgressSpinnerModule, MatIconModule, MatButtonModule,
    MatFormFieldModule, MatInputModule, MatTooltipModule
  ],
  templateUrl: './invoice-page.component.html',
  styleUrl: './invoice-page.component.css'
})
export class InvoicePageComponent {
  private invoiceService = inject(InvoiceService);
  private uiService = inject(UiService);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);

  readonly data = signal<PendingInvoiceSummary[]>([]);
  readonly totalItems = signal<number>(0);
  readonly loading = signal<boolean>(false);
  readonly error = signal<string | null>(null);
  readonly selectedCustomerIds = signal<Set<string>>(new Set());
  readonly expandedCustomerId = signal<string | null>(null);
  readonly detailOrders = signal<PendingOrderItem[]>([]);
  readonly detailLoading = signal<boolean>(false);
  readonly detailPageIndex = signal<number>(0);
  readonly detailPageSize = 5;
  readonly pagedDetailOrders = computed(() => {
    const orders = this.detailOrders();
    const start = this.detailPageIndex() * this.detailPageSize;
    return orders.slice(start, start + this.detailPageSize);
  });
  readonly detailTotalPages = computed(() =>
    Math.max(1, Math.ceil(this.detailOrders().length / this.detailPageSize))
  );

  @ViewChild(MatTable) table!: MatTable<PendingInvoiceSummary>;

  searchTerm: string = '';

  querySubject = new BehaviorSubject<InvoicePendingSummaryRequest>({ page: 1, pageSize: 10 });

  displayedColumns: string[] = [
    'select', 'businessName', 'vatNumber', 'ordersCount',
    'netAmountCents', 'vatAmountCents', 'totalGrossCents'
  ];

  isExpandedRow = (_index: number, row: PendingInvoiceSummary): boolean => {
    return this.expandedCustomerId() === row.customerId;
  };

  constructor() {
    this.querySubject.pipe(
      tap(() => {
        this.loading.set(true);
        this.error.set(null);
      }),
      switchMap(query => this.invoiceService.getPendingSummary(query)),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: (res: any) => {
        this.loading.set(false);
        if (res && res.data) {
          this.data.set(res.data.items || []);
          this.totalItems.set(res.data.totalCount || 0);
        } else {
          this.data.set(res.items || []);
          this.totalItems.set(res.totalCount || 0);
        }
      },
      error: (err) => {
        console.error(err);
        const errorMsg = 'Si è verificato un errore durante il caricamento dei dati.';
        this.loading.set(false);
        this.error.set(errorMsg);
        this.data.set([]);
        this.totalItems.set(0);
        this.uiService.showToast(errorMsg, 'error');
      }
    });
  }

  isAllSelected(): boolean {
    const currentData = this.data();
    if (currentData.length === 0) return false;
    const selected = this.selectedCustomerIds();
    return currentData.every(row => selected.has(row.customerId));
  }

  toggleAllRows(): void {
    const selected = new Set(this.selectedCustomerIds());
    if (this.isAllSelected()) {
      this.data().forEach(row => selected.delete(row.customerId));
    } else {
      this.data().forEach(row => selected.add(row.customerId));
    }
    this.selectedCustomerIds.set(selected);
  }

  toggleRow(customerId: string): void {
    const selected = new Set(this.selectedCustomerIds());
    if (selected.has(customerId)) {
      selected.delete(customerId);
    } else {
      selected.add(customerId);
    }
    this.selectedCustomerIds.set(selected);
  }

  toggleDetailRow(row: PendingInvoiceSummary): void {
    if (this.expandedCustomerId() === row.customerId) {
      this.expandedCustomerId.set(null);
      this.detailOrders.set([]);
      this.table?.renderRows();
      return;
    }

    this.expandedCustomerId.set(row.customerId);
    this.detailPageIndex.set(0);
    this.detailLoading.set(true);
    this.detailOrders.set([]);
    this.table?.renderRows();

    this.invoiceService.getPendingOrders(row.customerId).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: (res: any) => {
        this.detailLoading.set(false);
        this.detailOrders.set(res && res.data ? res.data : res);
      },
      error: (err) => {
        console.error(err);
        this.detailLoading.set(false);
        this.detailOrders.set([]);
        this.uiService.showToast('Errore nel caricamento degli ordini in sospeso.', 'error');
      }
    });
  }

  onSearch(value: string): void {
    this.searchTerm = value;
    this.updateQuery({ search: value });
  }

  updateQuery(partial: Partial<InvoicePendingSummaryRequest>): void {
    const current = this.querySubject.value;
    const isNavigation = 'page' in partial || 'pageSize' in partial;
    this.querySubject.next({
      ...current,
      ...partial,
      page: isNavigation ? (partial.page ?? current.page) : 1
    });
  }

  onPageChange(event: PageEvent): void {
    this.updateQuery({ page: event.pageIndex + 1, pageSize: event.pageSize });
  }

  onDetailPageChange(delta: 1 | -1): void {
    const next = this.detailPageIndex() + delta;
    if (next >= 0 && next < this.detailTotalPages()) {
      this.detailPageIndex.set(next);
    }
  }

  hasActiveFilters(): boolean {
    return !!this.searchTerm;
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.updateQuery({ search: '', page: 1 });
  }

  refreshTable(): void {
    this.updateQuery({});
  }

  navigateToHistory(): void {
    this.router.navigate(['/invoices/history']);
  }

  onGenerateBulkInvoices(): void {
    const selectedIds = Array.from(this.selectedCustomerIds());
    if (selectedIds.length === 0) return;

    this.uiService.showLoader('Generazione fatture in corso...');

    forkJoin(selectedIds.map(id => this.invoiceService.getPendingOrders(id)))
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (ordersArrays: any[]) => {
          const orderIds = ordersArrays.flatMap(res => {
            const orders = res && res.data ? res.data : res;
            return orders.map((o: any) => o.orderId);
          });

          if (orderIds.length === 0) {
            this.uiService.hideLoader();
            this.uiService.showToast('Nessun ordine in sospeso per i clienti selezionati.', 'error');
            return;
          }

          this.invoiceService.bulkInvoice({ orderIds, sendToSdiImmediately: false })
            .pipe(takeUntilDestroyed(this.destroyRef))
            .subscribe({
              next: () => {
                this.uiService.hideLoader();
                this.uiService.showToast('Fatture generate con successo! Il processo è in esecuzione in background.');
                this.selectedCustomerIds.set(new Set());
                this.refreshTable();
              },
              error: (err) => {
                console.error(err);
                this.uiService.hideLoader();
                this.uiService.showToast(
                  err.error?.message || 'Errore durante la generazione delle fatture.',
                  'error'
                );
              }
            });
        },
        error: (err) => {
          console.error(err);
          this.uiService.hideLoader();
          this.uiService.showToast(
            err.error?.message || 'Errore nel recupero degli ordini per i clienti selezionati.',
            'error'
          );
        }
      });
  }
}
