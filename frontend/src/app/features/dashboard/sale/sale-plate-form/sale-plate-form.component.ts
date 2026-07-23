import { Component, Inject, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';

// Angular Material
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule, provideNativeDateAdapter } from '@angular/material/core';

// Core & Shared
import { SaleService } from '../../../../core/services/sale.service';
import { CustomerService } from '../../../../core/services/customer.service';
import { PlateService } from '../../../../core/services/plate.service';
import { UiService } from '../../../../core/services/ui.service';
import { PlateDiscountDto, Customer, Plate } from '../../../../shared/models/api.models';

@Component({
  selector: 'app-sale-plate-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatDatepickerModule,
    MatNativeDateModule
  ],
  providers: [provideNativeDateAdapter()],
  templateUrl: './sale-plate-form.component.html',
  styleUrls: ['./sale-plate-form.component.css']
})
export class SalePlateFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private saleService = inject(SaleService);
  private customerService = inject(CustomerService);
  private plateService = inject(PlateService);
  private uiService = inject(UiService);

  form!: FormGroup;
  isEditMode = false;

  // Stato Dati Dropdown
  customers = signal<Customer[]>([]);
  plates = signal<Plate[]>([]);

  constructor(
    public dialogRef: MatDialogRef<SalePlateFormComponent>,
    @Inject(MAT_DIALOG_DATA) public data: PlateDiscountDto | null
  ) {
    this.isEditMode = !!data;
  }

  ngOnInit(): void {
    this.initForm();
    this.loadDropdownData();

    if (this.isEditMode && this.data) {
      this.populateForm();
    }
  }

  private initForm(): void {
    this.form = this.fb.group({
      customerId: [{ value: null, disabled: this.isEditMode }, [Validators.required]],
      plateId: [{ value: null, disabled: this.isEditMode }, [Validators.required]],
      overridePriceEuro: [null, [Validators.required, Validators.min(0)]],
      validFrom: [null],
      validTo: [null]
    });
  }

  private loadDropdownData(): void {
    // 1. Carica Clienti (Estrae in sicurezza array da qualsiasi wrapper di risposta)
    this.customerService.getCustomers({ page: 1, pageSize: 1000 }).subscribe({
      next: (res: any) => {
        const items = res.data?.items || res.items || [];
        this.customers.set(items);
      },
      error: () => this.uiService.showToast('Errore caricamento clienti', 'error')
    });

    // 2. Carica Piatti
    this.plateService.getPaged({ page: 1, pageSize: 1000 }).subscribe({
      next: (res: any) => {
        const items = res.data?.items || res.items || [];
        this.plates.set(items);
      },
      error: () => this.uiService.showToast('Errore caricamento piatti', 'error')
    });
  }

  private populateForm(): void {
    // FIX: Il Datepicker di Material vuole oggetti Date reali, non stringhe!
    const fromDate = this.data!.validFrom ? new Date(this.data!.validFrom) : null;
    const toDate = this.data!.validTo ? new Date(this.data!.validTo) : null;

    this.form.patchValue({
      customerId: this.data!.customerId,
      plateId: this.data!.plateId,
      overridePriceEuro: this.data!.overridePrice / 100, // DB = centesimi, UI = Euro
      validFrom: fromDate,
      validTo: toDate
    });
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const rawValue = this.form.getRawValue();

    const payload = {
      plateId: rawValue.plateId,
      overridePrice: Math.round(rawValue.overridePriceEuro * 100), // Converte in centesimi
      validFrom: rawValue.validFrom || null,
      validTo: rawValue.validTo || null
    };

    this.uiService.showLoader('Salvataggio in corso...');

    this.saleService.setPlateDiscount(rawValue.customerId, payload).subscribe({
      next: (res) => {
        this.uiService.hideLoader();
        if (res.succeeded) {
          this.uiService.showToast('Sconto piatto salvato con successo!');
          this.dialogRef.close(true);
        } else {
          this.uiService.showToast(res.message || 'Errore nel salvataggio', 'error');
        }
      },
      error: (err) => {
        this.uiService.hideLoader();
        this.uiService.showToast(err.error?.message || 'Errore di rete', 'error');
      }
    });
  }
}
