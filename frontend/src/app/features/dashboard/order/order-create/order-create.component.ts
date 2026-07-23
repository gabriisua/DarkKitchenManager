import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormArray } from '@angular/forms';
import { Router } from '@angular/router';
import { Observable, of } from 'rxjs';
import { debounceTime, distinctUntilChanged, map, startWith, switchMap } from 'rxjs/operators';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { CustomerService } from '../../../../core/services/customer.service';
import { OrderService } from '../../../../core/services/order.service';
import { PlateService } from '../../../../core/services/plate.service';
import { UiService } from '../../../../core/services/ui.service';

import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatAutocompleteModule, MatAutocompleteSelectedEvent } from '@angular/material/autocomplete';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatDivider } from '@angular/material/list';
import {MatTooltip} from '@angular/material/tooltip';

@Component({
  selector: 'app-order-create',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatAutocompleteModule,
    MatButtonModule,
    MatIconModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatSlideToggleModule,
    MatDivider,
    MatTooltip
  ],
  templateUrl: './order-create.component.html',
  styleUrls: ['./order-create.component.css']
})
export class OrderCreateComponent implements OnInit {
  private fb = inject(FormBuilder);
  private customerService = inject(CustomerService);
  private orderService = inject(OrderService);
  private plateService = inject(PlateService);
  private uiService = inject(UiService);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);

  orderForm!: FormGroup;
  filteredCustomers$: Observable<any[]> = of([]);
  hubs = signal<any[]>([]);

  readonly allPlates = signal<any[]>([]);
  filteredPlatesByRow: Observable<any[]>[] = [];

  ngOnInit(): void {
    this.initForm();
    this.setupCustomerSearch();
    this.loadAllPlates();
  }

  private initForm(): void {
    this.orderForm = this.fb.group({
      searchCustomer: [''],
      customerId: ['', Validators.required],
      deliveryHubId: [{ value: '', disabled: true }, Validators.required],
      requestedDeliveryDate: [new Date(), Validators.required],
      customerReference: [''], // <-- NUOVO CAMPO AGGIUNTO
      deliveryNotes: [''],
      bypassCalculator: [true],
      items: this.fb.array([], Validators.required)
    });

    this.addItem();
  }

  private setupCustomerSearch(): void {
    const searchControl = this.orderForm.get('searchCustomer');
    if (!searchControl) return;

    this.filteredCustomers$ = searchControl.valueChanges.pipe(
      takeUntilDestroyed(this.destroyRef),
      debounceTime(300),
      distinctUntilChanged(),
      map(value => typeof value === 'string' ? value : value?.businessName),
      switchMap(searchTerm => {
        if (!searchTerm) return of([]);
        return this.customerService.getCustomers({ page: 1, pageSize: 20, search: searchTerm }).pipe(
          map(res => res.data?.items || res.items || [])
        );
      })
    );
  }

  private loadAllPlates(): void {
    this.plateService.getPaged({ page: 1, pageSize: 500 }).subscribe({
      next: (res) => {
        this.allPlates.set(res.items || []);
        if (this.itemsFormArray.length > 0) {
          this.setupPlateFilterAt(0);
        }
      },
      error: (err) => {
        console.error(err);
        this.uiService.showToast('Impossibile caricare l\'elenco dei piatti', 'error');
      }
    });
  }

  displayCustomerName(customer: any): string {
    return customer && customer.businessName ? customer.businessName : '';
  }

  onCustomerSelected(event: MatAutocompleteSelectedEvent): void {
    const selectedCustomer = event.option.value;
    this.orderForm.patchValue({ customerId: selectedCustomer.id });

    const hubControl = this.orderForm.get('deliveryHubId');
    hubControl?.reset();
    hubControl?.enable();

    this.uiService.showLoader('Caricamento Hub...');
    this.customerService.getCustomerHubs(selectedCustomer.id).subscribe({
      next: (res) => {
        this.hubs.set(res.data || res);
        this.uiService.hideLoader();
      },
      error: () => {
        this.uiService.hideLoader();
        this.uiService.showToast('Errore nel caricamento degli Hub', 'error');
      }
    });
  }

  get itemsFormArray(): FormArray {
    return this.orderForm.get('items') as FormArray;
  }

  addItem(): void {
    const itemGroup = this.fb.group({
      searchPlate: [''],
      plateId: ['', Validators.required],
      quantity: [1, [Validators.required, Validators.min(1)]]
    });

    this.itemsFormArray.push(itemGroup);
    const index = this.itemsFormArray.length - 1;
    this.setupPlateFilterAt(index);
  }

  removeItem(index: number): void {
    this.itemsFormArray.removeAt(index);
    this.filteredPlatesByRow.splice(index, 1);
  }

  private setupPlateFilterAt(index: number): void {
    const rowGroup = this.itemsFormArray.at(index) as FormGroup;
    const searchControl = rowGroup.get('searchPlate');
    if (!searchControl) return;

    this.filteredPlatesByRow[index] = searchControl.valueChanges.pipe(
      startWith(''),
      map(value => typeof value === 'string' ? value : value?.name),
      map(name => name ? this.filterPlates(name) : this.allPlates())
    );
  }

  private filterPlates(name: string): any[] {
    const filterValue = name.toLowerCase();
    return this.allPlates().filter(plate => plate.name.toLowerCase().includes(filterValue));
  }

  displayPlateName(plate: any): string {
    return plate && plate.name ? plate.name : '';
  }

  onPlateSelected(event: MatAutocompleteSelectedEvent, index: number): void {
    const selectedPlate = event.option.value;
    const rowGroup = this.itemsFormArray.at(index);
    rowGroup.patchValue({ plateId: selectedPlate.id });
  }

  onSubmit(): void {
    if (this.orderForm.invalid) {
      this.orderForm.markAllAsTouched();
      this.uiService.showAlert('Attenzione', 'Compila tutti i campi obbligatori prima di salvare.');
      return;
    }

    const formValue = this.orderForm.getRawValue();
    const payload = {
      customerId: formValue.customerId,
      deliveryHubId: formValue.deliveryHubId,
      requestedDeliveryDate: formValue.requestedDeliveryDate,
      customerReference: formValue.customerReference, // <-- NUOVO CAMPO INVIATO AL BACKEND
      deliveryNotes: formValue.deliveryNotes,
      bypassCalculator: formValue.bypassCalculator,
      items: formValue.items.map((i: any) => ({
        plateId: i.plateId,
        quantity: i.quantity
      }))
    };

    this.uiService.showLoader('Creazione ordine in corso...');
    this.orderService.createOrder(payload).subscribe({
      next: () => {
        this.uiService.hideLoader();
        this.uiService.showToast('Ordine creato con successo!', 'success');
        this.router.navigate(['/orders']);
      },
      error: (err) => {
        this.uiService.hideLoader();
        this.uiService.showAlert('Errore', 'Impossibile creare l\'ordine.');
        console.error(err);
      }
    });
  }

  cancel(): void {
    this.router.navigate(['/orders']);
  }
}
