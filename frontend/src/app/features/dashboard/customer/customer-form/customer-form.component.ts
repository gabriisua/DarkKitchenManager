import { Component, inject, OnInit, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';

import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatTabsModule } from '@angular/material/tabs';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';

import { CustomerService } from '../../../../core/services/customer.service';
import { UiService } from '../../../../core/services/ui.service';
import { HubDialogComponent } from '../hub-dialog/hub-dialog.component';

@Component({
  selector: 'app-customer-form',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, RouterModule,
    MatFormFieldModule, MatInputModule, MatSelectModule,
    MatButtonModule, MatTabsModule, MatIconModule
  ],
  templateUrl: './customer-form.component.html',
  styleUrls: ['./customer-form.component.css']
})
export class CustomerFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private customerService = inject(CustomerService);
  public uiService = inject(UiService);
  private dialog = inject(MatDialog);
  private destroyRef = inject(DestroyRef);

  customerForm: FormGroup;
  isEditMode = false;
  customerId: string | null = null;

  deliveryHubs: any[] = [];

  constructor() {
    this.customerForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      type: ['B2B', Validators.required],
      businessName: [''],
      vatNumber: [''],
      fiscalCode: [''],
      sdiCode: [''],
      pec: [''],
      paymentTermsDays: [null],
      shippingAddress: ['', Validators.required],
      city: ['', Validators.required],
      zipCode: ['', Validators.required],
      contactPhone: ['']
    });
  }

  ngOnInit(): void {
    this.route.paramMap.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(params => {
      const id = params.get('id');
      if (id && id !== 'new') {
        this.isEditMode = true;
        this.customerId = id;
        this.loadCustomerData();
      } else {
        this.isEditMode = false;
      }
    });
  }

  loadCustomerData() {
    if (!this.customerId) return;

    this.uiService.showLoader('Caricamento dati cliente...');

    this.customerService.getCustomerById(this.customerId).subscribe({
      next: (res: any) => {
        const data = res.data;
        this.deliveryHubs = data.deliveryHubs || [];

        const defaultHub = this.deliveryHubs.find((h: any) => h.isDefault) || this.deliveryHubs[0] || {};

        this.customerForm.patchValue({
          email: data.email,
          type: data.type,
          businessName: data.businessName,
          vatNumber: data.vatNumber,
          fiscalCode: data.fiscalCode,
          sdiCode: data.sdiCode,
          pec: data.pec,
          paymentTermsDays: data.paymentTermsDays,
          shippingAddress: defaultHub.shippingAddress || '',
          city: defaultHub.city || '',
          zipCode: defaultHub.zipCode || '',
          contactPhone: defaultHub.contactPhone || ''
        });

        this.uiService.hideLoader();
      },
      error: () => {
        this.uiService.hideLoader();
        this.uiService.showToast('Errore nel caricamento del cliente', 'error');
        this.router.navigate(['/customers']);
      }
    });
  }

  loadHubsOnly() {
    if (!this.customerId) return;
    this.uiService.showLoader('Aggiornamento sedi logistiche...');
    this.customerService.getCustomerHubs(this.customerId).subscribe({
      next: (res: any) => {
        this.deliveryHubs = res.data || [];
        this.uiService.hideLoader();
      },
      error: () => {
        this.uiService.hideLoader();
        this.uiService.showToast("Errore nel ricaricamento delle sedi", 'error');
      }
    });
  }

  save() {
    if (this.customerForm.invalid) {
      this.customerForm.markAllAsTouched();
      this.uiService.showToast('Compila tutti i campi obbligatori', 'error');
      return;
    }

    const formValue = this.customerForm.getRawValue();
    this.uiService.showLoader('Salvataggio in corso...');

    if (this.isEditMode && this.customerId) {
      this.customerService.updateCustomer(this.customerId, formValue).subscribe({
        next: () => {
          this.uiService.hideLoader();
          this.uiService.showToast('Cliente aggiornato con successo', 'success');
        },
        error: (err) => {
          this.uiService.hideLoader();
          this.uiService.showToast(err.error?.message || 'Errore durante l\'aggiornamento', 'error');
        }
      });
    } else {
      const createPayload = { ...formValue, password: "TempPassword123!" };

      this.customerService.createCustomer(createPayload).subscribe({
        next: (res: any) => {
          this.uiService.hideLoader();
          this.uiService.showToast('Cliente creato con successo', 'success');

          const newId = res.data?.customerId || res.data?.id;
          this.router.navigate(['/customers', newId, 'edit'], { replaceUrl: true });
        },
        error: (err) => {
          this.uiService.hideLoader();
          this.uiService.showToast(err.error?.message || 'Errore durante la creazione', 'error');
        }
      });
    }
  }

  openHubDialog() {
    const dialogRef = this.dialog.open(HubDialogComponent, {
      width: '600px',
      data: { customerId: this.customerId },
      disableClose: true
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) this.loadHubsOnly();
    });
  }

  editHub(hub: any) {
    const dialogRef = this.dialog.open(HubDialogComponent, {
      width: '600px',
      data: { customerId: this.customerId, hub: hub },
      disableClose: true
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) this.loadHubsOnly();
    });
  }

  deleteHub(hubId: string) {
    this.uiService.askConfirm('Sei sicuro di voler eliminare questa sede?', () => {
      this.uiService.showLoader('Eliminazione sede...');
      this.customerService.deleteHub(this.customerId!, hubId).subscribe({
        next: () => {
          this.uiService.hideLoader();
          this.uiService.showToast('Sede eliminata con successo');
          this.loadHubsOnly();
        },
        error: () => {
          this.uiService.hideLoader();
          this.uiService.showToast('Errore durante l\'eliminazione della sede', 'error');
        }
      });
    });
  }

  goBack() {
    this.router.navigate(['/customers']);
  }
}
