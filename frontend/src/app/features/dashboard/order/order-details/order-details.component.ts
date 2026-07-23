import { Component, DestroyRef, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { switchMap, tap } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { OrderService } from '../../../../core/services/order.service';
import { UiService } from '../../../../core/services/ui.service';
import { OrderResponseDto } from '../../../../shared/models/api.models';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { MatTableModule } from '@angular/material/table';

@Component({
  selector: 'app-order-details',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatIconModule,
    MatButtonModule,
    MatDividerModule,
    MatTableModule,
  ],
  templateUrl: './order-details.component.html',
  styleUrls: ['./order-details.component.css'],
})
export class OrderDetailsComponent {
  private route = inject(ActivatedRoute);
  private orderService = inject(OrderService);
  private uiService = inject(UiService);
  private destroyRef = inject(DestroyRef);

  readonly order = signal<OrderResponseDto | null>(null);
  readonly loading = signal<boolean>(false);
  readonly error = signal<string | null>(null);

  displayedColumns: string[] = ['plateName', 'quantity', 'unitPrice'];

  constructor() {
    this.route.paramMap
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        tap(() => {
          this.loading.set(true);
          this.error.set(null);
        }),
        switchMap((params) => {
          const id = params.get('id');
          if (!id) throw new Error('Order ID not found');
          return this.orderService.getById(id);
        })
      )
      .subscribe({
        next: (res) => {
          this.order.set(res);
          this.loading.set(false);
        },
        error: (err) => {
          this.loading.set(false);
          this.error.set('Impossibile caricare il dettaglio dell\'ordine.');
        },
      });
  }

  formatCents(cents: number): string {
    return `€ ${(cents / 100).toFixed(2)}`;
  }

  onDownloadBolla(): void {
    const currentOrder = this.order();
    if (!currentOrder) return;

    this.uiService.showLoader('Generazione bolla in corso...');

    this.orderService.downloadDdt(currentOrder.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (blob: Blob) => {
          this.uiService.hideLoader();

          const url = window.URL.createObjectURL(blob);
          const link = document.createElement('a');
          link.href = url;

          // ECCO LA MODIFICA: abbiamo ripristinato l'estensione .pdf!
          link.download = `DDT_${currentOrder.orderNumber}.pdf`;

          document.body.appendChild(link);
          link.click();

          document.body.removeChild(link);
          window.URL.revokeObjectURL(url);

          this.uiService.showToast('Bolla scaricata con successo!', 'success');
        },
        error: (err) => {
          console.error(err);
          this.uiService.hideLoader();
          this.uiService.showToast('Errore durante la generazione della bolla.', 'error');
        }
      });
  }
}
