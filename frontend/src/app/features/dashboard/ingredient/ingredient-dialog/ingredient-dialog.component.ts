import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';

import { Ingredient, Allergen } from '../../../../shared/models/api.models';
import { AllergenService } from '../../../../core/services/allergen.service';
import { MatDividerModule } from '@angular/material/divider';

@Component({
  selector: 'app-ingredient-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatIconModule,
    MatDividerModule,
  ],
  templateUrl: './ingredient-dialog.component.html',
  styleUrls: ['./ingredient-dialog.component.css']
})
export class IngredientDialogComponent implements OnInit {
  private fb = inject(FormBuilder);
  private dialogRef = inject(MatDialogRef<IngredientDialogComponent>);
  public data: Ingredient | null = inject(MAT_DIALOG_DATA);

  private allergenService = inject(AllergenService);

  form!: FormGroup;
  isEditMode = false;

  // Signal tipizzato con il modello reale Allergen
  allergens = signal<Allergen[]>([]);

  ngOnInit(): void {
    this.isEditMode = !!this.data;
    this.initForm();
    this.loadAllergens();

    if (this.isEditMode && this.data) {
      this.form.patchValue({
        ...this.data,
        allergenIds: this.data.allergenIds || []
      });
    }
  }

  private initForm(): void {
    this.form = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(150)]],
      // NUOVO CAMPO - Opzionale ma con max length
      subIngredients: ['', [Validators.maxLength(1000)]],
      energyKjPer100g: [0, [Validators.min(0)]],
      energyKcalPer100g: [0, [Validators.min(0)]],
      fatsPer100g: [0, [Validators.min(0)]],
      saturatedFatsPer100g: [0, [Validators.min(0)]],
      carbohydratesPer100g: [0, [Validators.min(0)]],
      sugarsPer100g: [0, [Validators.min(0)]],
      fibersPer100g: [0, [Validators.min(0)]],
      proteinsPer100g: [0, [Validators.min(0)]],
      saltPer100g: [0, [Validators.min(0)]],
      costPer1000g: [0, [Validators.required, Validators.min(0)]],
      yieldPercentage: [100, [Validators.required, Validators.min(0), Validators.max(100)]],
      allergenIds: [[]]
    });
  }

  private loadAllergens(): void {
    this.allergenService.getAll().subscribe({
      next: (res) => {
        if (res.data) {
          this.allergens.set(res.data);
        }
      },
      error: (err) => console.error('Errore durante il caricamento degli allergeni', err)
    });
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.dialogRef.close(this.form.value);
  }
}
