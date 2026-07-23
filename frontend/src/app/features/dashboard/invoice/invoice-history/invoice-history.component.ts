import { Component, DestroyRef, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { BehaviorSubject, switchMap, tap } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { InvoiceService } from '../../../../core/services/invoice.service';
import { UiService } from '../../../../core/services/ui.service';
import {
  InvoiceHistoryItem,
  InvoiceHistoryRequest
} from '../../../../shared/models/api.models';

import { DataGridComponent } from '../../../../shared/data-grid/data-grid.component';
import {
  ColumnDef,
  PageChange,
  SortChange
} from '../../../../shared/data-grid/data-grid.models';

import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';

@Component({
  selector: 'app-invoice-history',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,

    DataGridComponent,

    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatButtonModule,
    MatTooltipModule,
    MatDatepickerModule,
    MatNativeDateModule
  ],
  templateUrl: './invoice-history.component.html',
  styleUrl: './invoice-history.component.css'
})
export class InvoiceHistoryComponent {
  private invoiceService = inject(InvoiceService);
  private uiService = inject(UiService);
  private destroyRef = inject(DestroyRef);

  readonly data = signal<InvoiceHistoryItem[]>([]);
  readonly totalItems = signal<number>(0);
  readonly loading = signal<boolean>(false);
  readonly error = signal<string | null>(null);

  searchTerm = '';

  dateFrom: string | null = null;
  dateTo: string | null = null;

  columns: ColumnDef<InvoiceHistoryItem>[] = [
    {
      field: 'invoiceNumber',
      header: 'N. Fattura',
      sortable: true
    },
    {
      field: 'customerName',
      header: 'Cliente',
      sortable: true
    },
    {
      field: 'ordersCount',
      header: 'Ordini Accorpati'
    },
    {
      field: 'totalGrossCents',
      header: 'Totale Lordo',
      cellType: 'currency',
      divisor: 100
    },
    {
      field: 'maxDeliveryDate',
      header: 'Data Consegna',
      cellType: 'date',
      sortable: true
    }
  ];

  readonly querySubject = new BehaviorSubject<InvoiceHistoryRequest>({
    page: 1,
    pageSize: 10,
    search: '',
    sortColumn: 'maxDeliveryDate',
    sortDirection: 'desc'
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
        switchMap(query => this.invoiceService.getInvoiceHistory(query))
      )
      .subscribe({
        next: (res) => {
          this.data.set(res.items);
          this.totalItems.set(res.totalCount);
          this.loading.set(false);
        },
        error: (err) => {
          console.error('Errore caricamento storico fatture:', err);
          this.loading.set(false);
          this.data.set([]);
          this.totalItems.set(0);
          const errorMsg = 'Si è verificato un errore durante il caricamento dello storico fatture.';
          this.error.set(errorMsg);
          this.uiService.showToast(errorMsg, 'error');
        }
      });
  }

  updateQuery(partialQuery: Partial<InvoiceHistoryRequest>): void {
    const current = this.querySubject.value;
    const isNavigation = 'page' in partialQuery || 'pageSize' in partialQuery;
    this.querySubject.next({
      ...current,
      ...partialQuery,
      page: isNavigation
        ? (partialQuery.page ?? current.page)
        : 1
    });
  }

  onSearch(value: string): void {
    this.updateQuery({ search: value });
  }

  onDateFilterChange(): void {
    this.updateQuery({
      dateFrom: this.dateFrom || undefined,
      dateTo: this.dateTo || undefined
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
    this.dateFrom = null;
    this.dateTo = null;
    this.querySubject.next({
      page: 1,
      pageSize: this.querySubject.value.pageSize,
      search: '',
      sortColumn: 'maxDeliveryDate',
      sortDirection: 'desc'
    });
  }

  refreshTable(): void {
    this.updateQuery({});
  }

  deleteInvoice(row: InvoiceHistoryItem): void {
    const displayName = row.invoiceNumber;

    this.uiService.askConfirm(
      `Sei sicuro di voler eliminare la fattura "${displayName}"?`,
      () => {
        this.uiService.showLoader('Eliminazione fattura in corso...');

        this.invoiceService
          .deleteInvoice(row.ficDocumentId)
          .subscribe({
            next: () => {
              this.uiService.hideLoader();
              this.uiService.showToast('Fattura eliminata correttamente.');
              this.refreshTable();
            },
            error: (err: any) => {
              console.error(err);
              this.uiService.hideLoader();
              this.uiService.showToast(
                err?.error?.message || "Errore durante l'eliminazione della fattura.",
                'error'
              );
            }
          });
      },
      'Conferma Eliminazione'
    );
  }

  downloadPdf(row: InvoiceHistoryItem): void {
    this.uiService.showLoader('Download PDF in corso...');

    this.invoiceService
      .getInvoicePdfUrl(row.ficDocumentId)
      .subscribe({
        next: (pdfUrl: string) => {
          this.uiService.hideLoader();
          window.open(pdfUrl, '_blank');
        },
        error: (err: any) => {
          console.error(err);
          this.uiService.hideLoader();
          this.uiService.showToast(
            err?.error?.message || 'Errore durante il download del PDF.',
            'error'
          );
        }
      });
  }
}
