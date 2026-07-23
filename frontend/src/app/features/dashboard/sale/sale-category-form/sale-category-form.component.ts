import { Component, Inject, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';

import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

import { SaleService } from '../../../../core/services/sale.service';
import { CustomerService } from '../../../../core/services/customer.service'; // <-- Import aggiunto
import { UiService } from '../../../../core/services/ui.service';
import { CategoryDiscountDto, Customer } from '../../../../shared/models/api.models';
import {MatDatepickerModule} from '@angular/material/datepicker';
import {MatNativeDateModule, provideNativeDateAdapter} from '@angular/material/core';

const CATEGORIES = [
  { id: 0, name: 'Antipasti' },
  { id: 1, name: 'Primi' },
  { id: 2, name: 'Secondi' },
  { id: 3, name: 'Contorni' },
  { id: 4, name: 'Dolci' },
  { id: 5, name: 'Bevande' },
];

@Component({
  selector: 'app-sale-category-form',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, MatDialogModule,
    MatFormFieldModule, MatInputModule, MatSelectModule, MatButtonModule, MatIconModule, MatDatepickerModule,
    MatNativeDateModule
  ],
  providers: [provideNativeDateAdapter()],
  templateUrl: './sale-category-form.component.html',
  styleUrls: ['./sale-category-form.component.css']
})
export class SaleCategoryFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private saleService = inject(SaleService);
  private customerService = inject(CustomerService);
  private uiService = inject(UiService);

  form!: FormGroup;
  isEditMode = false;

  // Stato Dati Dropdown
  customers = signal<Customer[]>([]);
  categories = signal<{id: number, name: string}[]>(CATEGORIES);

  constructor(
    public dialogRef: MatDialogRef<SaleCategoryFormComponent>,
    @Inject(MAT_DIALOG_DATA) public data: CategoryDiscountDto | null
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
      categoryId: [{ value: null, disabled: this.isEditMode }, [Validators.required]],
      discountPercentage: [null, [Validators.required, Validators.min(0), Validators.max(100)]],
      validFrom: [null],
      validTo: [null]
    });
  }

  private loadDropdownData(): void {
    // Carica solo i clienti attivi per popolare la select
    this.customerService.getPaged({ page: 1, pageSize: 1000, isActive: true }).subscribe({
      next: (res) => this.customers.set(res.items),
      error: () => this.uiService.showToast('Errore nel caricamento clienti', 'error')
    });
  }

  private populateForm(): void {
    const from = this.data!.validFrom ? new Date(this.data!.validFrom).toISOString().split('T')[0] : null;
    const to = this.data!.validTo ? new Date(this.data!.validTo).toISOString().split('T')[0] : null;

    this.form.patchValue({
      customerId: this.data!.customerId,
      categoryId: this.data!.categoryId,
      discountPercentage: this.data!.discountPercentage,
      validFrom: from,
      validTo: to
    });
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const rawValue = this.form.getRawValue();

    const payload = {
      categoryId: rawValue.categoryId,
      discountPercentage: rawValue.discountPercentage,
      validFrom: rawValue.validFrom || null,
      validTo: rawValue.validTo || null
    };

    this.uiService.showLoader('Salvataggio in corso...');

    this.saleService.setCategoryDiscount(rawValue.customerId, payload).subscribe({
      next: (res) => {
        this.uiService.hideLoader();
        if (res.succeeded) {
          this.uiService.showToast('Sconto categoria salvato con successo!');
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
