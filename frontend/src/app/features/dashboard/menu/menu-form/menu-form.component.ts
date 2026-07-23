import { Component, inject, OnInit, signal, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormArray, Validators, ReactiveFormsModule, FormControl } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { debounceTime, switchMap, tap } from 'rxjs/operators';
import { of } from 'rxjs';

import { MenuService } from '../../../../core/services/menu.service';
import { PlateService } from '../../../../core/services/plate.service';
import { UiService } from '../../../../core/services/ui.service';
import { Plate, Customer } from '../../../../shared/models/api.models';
import { CustomerService } from '../../../../core/services/customer.service';

import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatDividerModule } from '@angular/material/divider';
import { MatCardModule } from '@angular/material/card';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';

@Component({
  selector: 'app-menu-form',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, RouterModule,
    MatFormFieldModule, MatInputModule, MatSelectModule,
    MatButtonModule, MatIconModule, MatAutocompleteModule,
    MatDividerModule, MatCardModule, MatSlideToggleModule, MatTooltipModule,
    MatDatepickerModule, MatNativeDateModule
  ],
  templateUrl: './menu-form.component.html',
  styleUrls: ['./menu-form.component.scss']
})
export class MenuFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private menuService = inject(MenuService);
  private plateService = inject(PlateService);
  private customerService = inject(CustomerService);
  private uiService = inject(UiService);
  private destroyRef = inject(DestroyRef);

  form!: FormGroup;
  menuId: number | null = null;
  isEditMode = false;

  searchPlateCtrl = new FormControl('');
  searchResults = signal<Plate[]>([]);
  isSearching = signal(false);
  customers = signal<Customer[]>([]);

  ngOnInit(): void {
    this.initForm();
    this.setupPlateSearch();
    this.loadCustomers();

    this.route.paramMap.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(params => {
      const idParam = params.get('id');
      if (idParam) {
        this.menuId = +idParam;
        this.isEditMode = true;
        this.loadMenuData(this.menuId);
      }
    });
  }

  private initForm(): void {
    this.form = this.fb.group({
      name: ['', Validators.required],
      description: [''],
      isActive: [true],
      customerId: [null],
      menuItems: this.fb.array([])
    });
  }

  get menuItemsArray(): FormArray {
    return this.form.get('menuItems') as FormArray;
  }

  private setupPlateSearch(): void {
    this.searchPlateCtrl.valueChanges.pipe(
      takeUntilDestroyed(this.destroyRef),
      debounceTime(300),
      tap(() => this.isSearching.set(true)),
      switchMap(value => {
        if (!value || typeof value !== 'string' || value.length < 2) {
          this.isSearching.set(false);
          return of({ items: [] });
        }
        return this.plateService.getPaged({ search: value, page: 1, pageSize: 20 });
      })
    ).subscribe({
      next: (res: any) => {
        this.searchResults.set(res.items || []);
        this.isSearching.set(false);
      },
      error: () => this.isSearching.set(false)
    });
  }

  private loadCustomers(): void {
    this.customerService.getPaged({ isActive: true, page: 1, pageSize: 200 }).subscribe({
      next: (res) => {
        this.customers.set(res.items.filter(c => c.isActive));
      }
    });
  }

  onPlateSelected(event: any): void {
    const selectedPlate: Plate = event.option.value;

    const exists = this.menuItemsArray.controls.some(
      ctrl => ctrl.value.plateId === selectedPlate.id
    );

    if (!exists) {
      this.menuItemsArray.push(this.fb.group({
        plateId: [selectedPlate.id, Validators.required],
        plateName: [selectedPlate.name],
        overridePriceEuro: [null],
        availableFrom: [null],
        availableTo: [null]
      }));
    } else {
      this.uiService.showToast('Piatto già presente nel menu');
    }

    this.searchPlateCtrl.setValue('');
  }

  removePlate(index: number): void {
    this.menuItemsArray.removeAt(index);
  }

  displayPlateName(plate: Plate): string {
    return plate && plate.name ? plate.name : '';
  }

  private loadMenuData(id: number): void {
    this.uiService.showLoader('Caricamento menu...');

    this.menuService.getById(id).subscribe({
      next: (res: any) => {
        const menu = res.data || res;

        this.form.patchValue({
          name: menu.name,
          description: menu.description,
          isActive: menu.isActive,
          customerId: menu.customerId || null
        });

        if (menu.menuItems && menu.menuItems.length > 0) {
          this.menuItemsArray.clear();

          menu.menuItems.forEach((item: any) => {
            this.menuItemsArray.push(this.fb.group({
              plateId: [item.plateId, Validators.required],
              plateName: [item.plateName],
              overridePriceEuro: [
                item.overridePrice != null ? item.overridePrice / 100 : null
              ],
              availableFrom: [item.availableFrom || null],
              availableTo: [item.availableTo || null]
            }));
          });
        }

        this.uiService.hideLoader();
      },
      error: () => {
        this.uiService.hideLoader();
        this.uiService.showToast('Errore durante il caricamento del menu', 'error');
        this.goBack();
      }
    });
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.uiService.showToast('Compila tutti i campi obbligatori');
      return;
    }

    // Validate date ranges per item
    const items = this.form.value.menuItems || [];
    for (let i = 0; i < items.length; i++) {
      const item = items[i];
      if (item.availableFrom && item.availableTo && item.availableTo < item.availableFrom) {
        this.uiService.showToast(
          `Riga ${i + 1}: La data di fine validità non può essere antecedente alla data di inizio.`,
          'error'
        );
        return;
      }
    }

    const payload = {
      name: this.form.value.name,
      description: this.form.value.description || '',
      isActive: this.form.value.isActive,
      customerId: this.form.value.customerId || null,
      menuItems: items.map((item: any) => ({
        plateId: item.plateId,
        ...(item.overridePriceEuro != null
          ? { overridePrice: Math.round(item.overridePriceEuro * 100) }
          : {}),
        ...(item.availableFrom ? { availableFrom: item.availableFrom } : {}),
        ...(item.availableTo ? { availableTo: item.availableTo } : {})
      }))
    };

    this.uiService.showLoader('Salvataggio in corso...');

    const request$ = this.isEditMode
      ? this.menuService.update(this.menuId!, payload as any)
      : this.menuService.create(payload as any);

    request$.subscribe({
      next: () => {
        this.uiService.hideLoader();
        this.uiService.showToast(`Menu ${this.isEditMode ? 'aggiornato' : 'creato'} con successo!`);
        this.router.navigate(['/menus']);
      },
      error: (err) => {
        this.uiService.hideLoader();
        console.error(err);
        this.uiService.showToast(err.error?.message || 'Errore durante il salvataggio', 'error');
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/menus']);
  }
}
