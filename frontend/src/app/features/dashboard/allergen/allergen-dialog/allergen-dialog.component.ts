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
import { MatButtonModule } from '@angular/material/button';

import { AllergenService } from '../../../../core/services/allergen.service';
import { Allergen } from '../../../../shared/models/api.models';

@Component({
  selector: 'app-allergen-dialog',
  templateUrl: './allergen-dialog.component.html',
  styleUrls: ['./allergen-dialog.component.css'],
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, MatDialogTitle, MatDialogContent,
    MatDialogActions, MatDialogClose, MatFormFieldModule, MatInputModule, MatButtonModule
  ]
})
export class AllergenDialogComponent {
  private fb = inject(FormBuilder);
  private allergenService = inject(AllergenService);
  private dialogRef = inject(MatDialogRef<AllergenDialogComponent>);
  public data = inject<Allergen | null>(MAT_DIALOG_DATA);

  allergenForm: FormGroup;
  isEditMode = !!this.data;
  serverError = '';

  constructor() {
    this.allergenForm = this.fb.group({
      name: [this.data?.name || '', Validators.required],
      code: [this.data?.code || '', [Validators.required, Validators.maxLength(10)]],
      description: [this.data?.description || '']
    });
  }

  save() {
    if (this.allergenForm.invalid) return;

    this.serverError = '';
    const formValue = this.allergenForm.getRawValue();

    if (this.isEditMode && this.data) {
      this.allergenService.update(this.data.id, formValue).subscribe({
        next: () => this.dialogRef.close(true),
        error: (err) => this.handleError(err)
      });
    } else {
      this.allergenService.create(formValue).subscribe({
        next: () => this.dialogRef.close(true),
        error: (err) => this.handleError(err)
      });
    }
  }

  private handleError(err: any) {
    console.error('API Error:', err);
    this.serverError = err.error?.message || 'Si è verificato un errore durante il salvataggio.';
  }
}
