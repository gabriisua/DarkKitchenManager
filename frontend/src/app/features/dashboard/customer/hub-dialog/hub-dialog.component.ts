import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import {
  MatDialogRef,
  MAT_DIALOG_DATA,
  MatDialogTitle,
  MatDialogContent,
  MatDialogActions,
  MatDialogClose
} from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatButtonModule } from '@angular/material/button';

import { CustomerService } from '../../../../core/services/customer.service';

@Component({
  selector: 'app-delivery-hub-dialog',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, MatDialogTitle, MatDialogContent,
    MatDialogActions, MatDialogClose, MatFormFieldModule, MatInputModule,
    MatCheckboxModule, MatButtonModule
  ],
  templateUrl: './hub-dialog.component.html',
  styleUrl: './hub-dialog.component.css'
})
export class HubDialogComponent {
  private fb = inject(FormBuilder);
  private customerService = inject(CustomerService);
  public dialogRef = inject(MatDialogRef<HubDialogComponent>);

  // Riceviamo l'ID del cliente e, se siamo in modifica, i dati dell'hub
  public data = inject(MAT_DIALOG_DATA) as { customerId: string; hub?: any };

  hubForm: FormGroup;
  isEditMode: boolean;
  serverError: string = '';

  constructor() {
    this.isEditMode = !!this.data.hub;
    const hub = this.data.hub || {};

    this.hubForm = this.fb.group({
      name: [hub.name || '', Validators.required],
      shippingAddress: [hub.shippingAddress || '', Validators.required],
      addressSuffix: [hub.addressSuffix || ''],
      city: [hub.city || '', Validators.required],
      zipCode: [hub.zipCode || '', Validators.required],
      province: [hub.province || ''],
      contactPhone: [hub.contactPhone || ''],
      deliveryOpenTime: [hub.deliveryOpenTime || null],
      deliveryCloseTime: [hub.deliveryCloseTime || null],
      deliveryNotes: [hub.deliveryNotes || ''],
      isDefault: [hub.isDefault || false]
    });
  }

  save() {
    if (this.hubForm.invalid) return;
    this.serverError = '';

    const formValue = this.hubForm.getRawValue();

    if (this.isEditMode) {
      this.customerService.updateHub(this.data.customerId, this.data.hub.id, formValue).subscribe({
        next: () => this.dialogRef.close(true),
        error: (err) => this.serverError = err.error?.message || 'Errore durante il salvataggio.'
      });
    } else {
      this.customerService.createHub(this.data.customerId, formValue).subscribe({
        next: () => this.dialogRef.close(true),
        error: (err) => this.serverError = err.error?.message || 'Errore durante la creazione.'
      });
    }
  }
}
