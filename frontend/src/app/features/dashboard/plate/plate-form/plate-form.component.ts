import { Component, inject, OnInit, signal, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormArray, Validators, ReactiveFormsModule, FormControl } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { debounceTime, switchMap, tap } from 'rxjs/operators';
import { of } from 'rxjs';

import { PlateService } from '../../../../core/services/plate.service';
import { IngredientService } from '../../../../core/services/ingredient.service';
import { UiService } from '../../../../core/services/ui.service';
import { Ingredient } from '../../../../shared/models/api.models';

import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatDividerModule } from '@angular/material/divider';
import { MatCardModule } from '@angular/material/card';
import { MatTooltip } from '@angular/material/tooltip';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';

const CATEGORIES = [
  { id: 0, name: 'Antipasti' },
  { id: 1, name: 'Primi' },
  { id: 2, name: 'Secondi' },
  { id: 3, name: 'Contorni' },
  { id: 4, name: 'Dolci' },
  { id: 5, name: 'Bevande' },
];

const LINE_TYPES = [
  { value: 0, label: 'Standard' },
  { value: 1, label: 'Gourmet' },
  { value: 2, label: 'Vegetale' },
  { value: 3, label: 'Fitness' },
  { value: 4, label: 'Planted' }
];

const DIETARY_ICONS = [
  { value: 0, label: 'Nessuna' },
  { value: 1, label: 'Vegano' },
  { value: 2, label: 'Vegetariano' },
  { value: 3, label: 'Carne' },
  { value: 4, label: 'Pesce' }
];

@Component({
  selector: 'app-plate-form',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule,
    MatFormFieldModule, MatInputModule, MatSelectModule,
    MatButtonModule, MatIconModule, MatAutocompleteModule,
    MatDividerModule, MatCardModule, MatTooltip, MatSlideToggleModule
  ],
  templateUrl: './plate-form.component.html',
  styleUrls: ['./plate-form.component.scss']
})
export class PlateFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private plateService = inject(PlateService);
  private ingredientService = inject(IngredientService);
  private uiService = inject(UiService);
  private destroyRef = inject(DestroyRef);

  readonly categories = CATEGORIES;
  readonly lineTypes = LINE_TYPES;
  readonly dietaryIcons = DIETARY_ICONS;

  form!: FormGroup;
  plateId: number | null = null;
  isEditMode = false;

  searchIngredientCtrl = new FormControl('');
  searchResults = signal<Ingredient[]>([]);
  isSearching = signal(false);

  ngOnInit(): void {
    this.initForm();
    this.setupIngredientSearch();

    this.route.paramMap.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(params => {
      const idParam = params.get('id');
      if (idParam) {
        this.plateId = +idParam;
        this.isEditMode = true;
        this.loadPlateData(this.plateId);
      }
    });
  }

  private initForm(): void {
    this.form = this.fb.group({
      code: ['', [Validators.maxLength(50)]],
      name: ['', Validators.required],
      description: [''],
      categoryId: [null, Validators.required],
      basePrice: [0, [Validators.required, Validators.min(0)]],
      packagingCost: [0, [Validators.min(0)]],
      vatRate: [10, [Validators.required, Validators.min(0)]],

      eanCode: ['', [Validators.minLength(8), Validators.maxLength(13)]],
      microwaveWattage: [null, [Validators.min(0)]],
      microwaveMinutes: [null, [Validators.min(0)]],
      preparationInstructions: ['', [Validators.maxLength(250)]],

      daysToExpire: [15, [Validators.required, Validators.min(1)]],
      productType: ['Preparazione gastronomica', [Validators.maxLength(100)]],
      packagingDescription: ['', [Validators.maxLength(500)]],
      storageConditions: ['Conservare in frigorifero tra 0°C e +4°C.', [Validators.maxLength(250)]],
      preservationTechnology: ['Confezionato in atmosfera protettiva (ATM).', [Validators.maxLength(250)]],

      lineType: [0, Validators.required],
      dietaryIcon: [0, Validators.required],
      isWowPlate: [false],
      isXlPlate: [false],

      ingredients: this.fb.array([])
    });
  }

  get ingredientsArray(): FormArray {
    return this.form.get('ingredients') as FormArray;
  }

  private setupIngredientSearch(): void {
    this.searchIngredientCtrl.valueChanges.pipe(
      takeUntilDestroyed(this.destroyRef),
      debounceTime(300),
      tap(() => this.isSearching.set(true)),
      switchMap(value => {
        if (!value || typeof value !== 'string' || value.length < 2) {
          this.isSearching.set(false);
          return of({ items: [] });
        }
        return this.ingredientService.getPaged({ search: value, page: 1, pageSize: 20 });
      })
    ).subscribe({
      next: (res: any) => {
        this.searchResults.set(res.items || []);
        this.isSearching.set(false);
      },
      error: () => this.isSearching.set(false)
    });
  }

  onIngredientSelected(event: any): void {
    const selectedIng: Ingredient = event.option.value;
    const exists = this.ingredientsArray.controls.some(
      ctrl => ctrl.value.ingredientId === selectedIng.id
    );

    if (!exists) {
      this.ingredientsArray.push(this.fb.group({
        ingredientId: [selectedIng.id, Validators.required],
        ingredientName: [selectedIng.name],
        weightInGrams: [100, [Validators.required, Validators.min(1)]],
        costPer1000g: [selectedIng.costPer1000g]
      }));
    } else {
      this.uiService.showToast('Ingrediente già presente nella lista');
    }

    this.searchIngredientCtrl.setValue('');
  }

  removeIngredient(index: number): void {
    this.ingredientsArray.removeAt(index);
  }

  displayIngredientName(ingredient: Ingredient): string {
    return ingredient && ingredient.name ? ingredient.name : '';
  }

  private loadPlateData(id: number): void {
    this.uiService.showLoader('Caricamento piatto...');

    this.plateService.getById(id).subscribe({
      next: (res: any) => {
        const plate = res.data || res;

        this.form.patchValue({
          code: plate.code,
          name: plate.name,
          description: plate.description,
          categoryId: CATEGORIES.find(c => c.name === plate.categoryName)?.id ?? 0,
          basePrice: plate.basePrice ? plate.basePrice / 100 : 0,
          packagingCost: plate.packagingCost ? plate.packagingCost / 100 : 0,
          vatRate: plate.vatRate !== undefined ? plate.vatRate : 10,
          eanCode: plate.eanCode,
          microwaveWattage: plate.microwaveWattage,
          microwaveMinutes: plate.microwaveMinutes,
          preparationInstructions: plate.preparationInstructions,

          daysToExpire: plate.daysToExpire,
          productType: plate.productType,
          packagingDescription: plate.packagingDescription,
          storageConditions: plate.storageConditions,
          preservationTechnology: plate.preservationTechnology,

          lineType: plate.lineType ?? 0,
          dietaryIcon: plate.dietaryIcon ?? 0,
          isWowPlate: plate.isWowPlate || false,
          isXlPlate: plate.isXlPlate || false
        });

        if (plate.ingredients && plate.ingredients.length > 0) {
          this.ingredientsArray.clear();
          plate.ingredients.forEach((pi: any) => {
            this.ingredientsArray.push(this.fb.group({
              ingredientId: [pi.ingredientId, Validators.required],
              ingredientName: [pi.ingredientName],
              weightInGrams: [pi.weightInGrams, [Validators.required, Validators.min(1)]]
            }));
          });
        }

        this.uiService.hideLoader();
      },
      error: () => {
        this.uiService.hideLoader();
        this.uiService.showToast('Errore durante il caricamento del piatto', 'error');
        this.goBack();
      }
    });
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.uiService.showToast('Compila tutti i campi obbligatori o correggi gli errori.');
      return;
    }

    const payload = { ...this.form.value };

    const parsedBasePrice = parseFloat(payload.basePrice) || 0;
    payload.basePrice = Math.round(parsedBasePrice * 100);

    const parsedPackaging = parseFloat(payload.packagingCost) || 0;
    payload.packagingCost = Math.round(parsedPackaging * 100);

    payload.categoryId = parseInt(payload.categoryId, 10) || 0;
    payload.vatRate = parseFloat(payload.vatRate) || 0;
    payload.microwaveWattage = payload.microwaveWattage ? parseInt(payload.microwaveWattage, 10) : null;
    payload.microwaveMinutes = payload.microwaveMinutes ? parseFloat(payload.microwaveMinutes) : null;
    payload.daysToExpire = parseInt(payload.daysToExpire, 10) || 3;

    payload.lineType = parseInt(payload.lineType, 10) || 0;
    payload.dietaryIcon = parseInt(payload.dietaryIcon, 10) || 0;
    payload.isWowPlate = !!payload.isWowPlate;
    payload.isXlPlate = !!payload.isXlPlate;

    if (!payload.code) payload.code = null;
    if (!payload.eanCode) payload.eanCode = null;
    if (!payload.preparationInstructions) payload.preparationInstructions = null;

    payload.ingredients = payload.ingredients.map((ing: any) => ({
      ingredientId: parseInt(ing.ingredientId, 10),
      weightInGrams: parseFloat(ing.weightInGrams)
    }));

    this.uiService.showLoader('Salvataggio in corso...');

    const request$ = this.isEditMode
      ? this.plateService.update(this.plateId!, payload)
      : this.plateService.create(payload);

    request$.subscribe({
      next: (res: any) => {
        this.uiService.hideLoader();
        this.uiService.showToast(`Piatto ${this.isEditMode ? 'aggiornato' : 'creato'} con successo!`);

        const savedId = this.isEditMode ? this.plateId : (res.data?.id || res.id);
        this.router.navigate(['/plates', savedId]);
      },
      error: (err) => {
        this.uiService.hideLoader();
        console.error(err);
        this.uiService.showToast(err.error?.message || 'Errore durante il salvataggio', 'error');
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/plates']);
  }
}
