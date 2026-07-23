import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogTitle, MatDialogContent, MatDialogActions, MatDialogClose } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { OrderStatus } from '../../../../shared/models/api.models';

export interface OrderStatusDialogData {
  orderId: string;
  currentStatus: OrderStatus;
}

@Component({
  selector: 'app-order-status-dialog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatDialogTitle,
    MatDialogContent,
    MatDialogActions,
    MatFormFieldModule,
    MatSelectModule,
    MatButtonModule,
  ],
  templateUrl: './order-status-dialog.component.html',
  styleUrls: ['./order-status-dialog.component.css'],
})
export class OrderStatusDialogComponent {
  private dialogRef = inject(MatDialogRef<OrderStatusDialogComponent>);
  public data = inject<OrderStatusDialogData>(MAT_DIALOG_DATA);

  readonly selectedStatus = signal<OrderStatus>(this.data.currentStatus);
  readonly OrderStatus = OrderStatus;

  statusOptions = [
    { value: OrderStatus.Pending, label: 'In Attesa' },
    { value: OrderStatus.Confirmed, label: 'Confermato' },
    { value: OrderStatus.InProduction, label: 'In Cucina' },
    { value: OrderStatus.Shipped, label: 'In Consegna' },
    { value: OrderStatus.Delivered, label: 'Consegnato' },
    { value: OrderStatus.Cancelled, label: 'Annullato' },
  ];

  getStatusLabel(status: OrderStatus): string {
    return this.statusOptions.find((o) => o.value === status)?.label || 'Sconosciuto';
  }

  onSave(): void {
    this.dialogRef.close({
      orderId: this.data.orderId,
      newStatus: this.selectedStatus(),
    });
  }

  onCancel(): void {
    this.dialogRef.close();
  }
}
