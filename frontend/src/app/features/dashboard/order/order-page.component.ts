import { Component, DestroyRef, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { BehaviorSubject, switchMap, tap } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { OrderService } from '../../../core/services/order.service';
import { UiService } from '../../../core/services/ui.service';
import { OrderResponseDto, OrderStatus } from '../../../shared/models/api.models';
import { DataGridComponent } from '../../../shared/data-grid/data-grid.component';
import { ColumnDef, PageChange, SortChange } from '../../../shared/data-grid/data-grid.models';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDividerModule } from '@angular/material/divider';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { OrderStatusDialogComponent } from './order-status-dialog/order-status-dialog.component';

interface OrderRow extends OrderResponseDto {
  totalFormatted: string;
  statusLabel: string;
}

interface OrderPagedRequest {
  page: number;
  pageSize: number;
  status?: OrderStatus;
  customerId?: string;
  dateFrom?: string;
  dateTo?: string;
  sortColumn?: string;
  sortDirection?: string;
}

@Component({
  selector: 'app-order-page',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    DataGridComponent,
    MatFormFieldModule,
    MatSelectModule,
    MatIconModule,
    MatButtonModule,
    MatTooltipModule,
    MatDividerModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatDialogModule,
  ],
  templateUrl: './order-page.component.html',
  styleUrls: ['./order-page.component.css']
})
export class OrderPageComponent {
  private orderService = inject(OrderService);
  private uiService = inject(UiService);
  private destroyRef = inject(DestroyRef);
  private router = inject(Router);
  private dialog = inject(MatDialog);

  public OrderStatus = OrderStatus;

  readonly data = signal<OrderRow[]>([]);
  readonly totalItems = signal<number>(0);
  readonly loading = signal<boolean>(false);
  readonly error = signal<string | null>(null);

  filters = {
    status: null as OrderStatus | null,
    dateFrom: new Date() as Date | null,
    dateTo: new Date() as Date | null
  };

  columns: ColumnDef<OrderRow>[] = [
    { field: 'orderDate', header: 'Data Ordine', cellType: 'date', sortable: true },
    { field: 'customerBusinessName', header: 'Cliente', sortable: true },
    { field: 'deliveryHubName', header: 'Hub di Consegna', sortable: true },
    { field: 'calculatedDeliveryDate', header: 'Data Consegna', cellType: 'date', sortable: true },
    { field: 'totalFormatted', header: 'Totale', sortable: true },
    { field: 'statusLabel', header: 'Stato', cellType: 'chip', sortable: true }
  ];

  readonly querySubject = new BehaviorSubject<OrderPagedRequest>(this.getDefaultQuery());

  constructor() {
    this.loadData();
  }

  private getDefaultQuery(): OrderPagedRequest {
    const start = new Date();
    start.setHours(0, 0, 0, 0);
    const end = new Date();
    end.setHours(23, 59, 59, 999);
    return {
      page: 1,
      pageSize: -1,
      dateFrom: start.toISOString(),
      dateTo: end.toISOString(),
      sortColumn: 'calculatedDeliveryDate',
      sortDirection: 'asc'
    };
  }

  private loadData(): void {
    this.querySubject
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        tap(() => {
          this.loading.set(true);
          this.error.set(null);
        }),
        switchMap(query => this.orderService.getPaged(query))
      )
      .subscribe({
        next: (res) => {
          const mappedRows: OrderRow[] = res.items.map(o => ({
            ...o,
            totalFormatted: `€ ${(o.totalGrossCents / 100).toFixed(2)}`,
            statusLabel: this.getStatusLabel(o.status)
          }));
          this.data.set(mappedRows);
          this.totalItems.set(res.totalCount);
          this.loading.set(false);
        },
        error: (err) => {
          this.loading.set(false);
          this.data.set([]);
          this.totalItems.set(0);
          this.error.set('Si è verificato un errore JavaScript durante il caricamento degli ordini.');
        }
      });
  }

  updateQuery(partialQuery: Partial<OrderPagedRequest>): void {
    const current = this.querySubject.value;
    const isNavigation = 'page' in partialQuery || 'pageSize' in partialQuery;
    this.querySubject.next({
      ...current,
      ...partialQuery,
      page: isNavigation ? (partialQuery.page ?? current.page) : 1
    });
  }

  onFilterChange(): void {
    if (this.filters.dateFrom && !this.filters.dateTo) {
      return;
    }

    let df: string | undefined = undefined;
    let dt: string | undefined = undefined;

    if (this.filters.dateFrom) {
      const start = new Date(this.filters.dateFrom);
      start.setHours(0, 0, 0, 0);
      df = start.toISOString();
    }

    if (this.filters.dateTo) {
      const end = new Date(this.filters.dateTo);
      end.setHours(23, 59, 59, 999);
      dt = end.toISOString();
    }

    this.updateQuery({
      status: this.filters.status || undefined,
      dateFrom: df,
      dateTo: dt,
      page: 1
    });
  }

  onPageChange(event: PageChange): void {
    this.updateQuery({
      page: event.page,
      pageSize: event.pageSize
    });
  }

  onSortChange(sort: SortChange): void {
    this.updateQuery({
      sortColumn: sort.direction ? sort.column : undefined,
      sortDirection: sort.direction ? sort.direction : undefined
    });
  }

  refreshTable(): void {
    this.updateQuery({});
  }

  viewDetails(row: OrderRow): void {
    this.router.navigate(['/orders', row.id]);
  }

  changeStatus(row: OrderRow): void {
    const dialogRef = this.dialog.open(OrderStatusDialogComponent, {
      data: { orderId: row.id, currentStatus: row.status },
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (!result) return;

      this.uiService.showLoader('Aggiornamento stato...');
      this.orderService.updateStatus(result.orderId, result.newStatus).subscribe({
        next: () => {
          this.uiService.hideLoader();
          this.uiService.showToast('Stato aggiornato con successo');
          this.refreshTable();
        },
        error: () => {
          this.uiService.hideLoader();
          this.uiService.showAlert('Errore', 'Impossibile aggiornare lo stato dell\'ordine.');
        },
      });
    });
  }

  private getStatusLabel(status: OrderStatus): string {
    const map: Record<OrderStatus, string> = {
      [OrderStatus.Pending]: 'In Attesa',
      [OrderStatus.Confirmed]: 'Confermato',
      [OrderStatus.InProduction]: 'In Cucina',
      [OrderStatus.Shipped]: 'In Consegna',
      [OrderStatus.Delivered]: 'Consegnato',
      [OrderStatus.Cancelled]: 'Annullato'
    };
    return map[status] || 'Sconosciuto';
  }

  goToCreateOrder(): void {
    this.router.navigate(['/orders/create']);
  }
}
