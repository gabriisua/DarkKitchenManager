import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';

// Angular Material Modules
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';

// Servizi e Modelli
import { CategoryService } from '../../../../core/services/category.service';
import { UiService } from '../../../../core/services/ui.service';
import { Category } from '../../../../shared/models/api.models';

@Component({
  selector: 'app-category-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatSlideToggleModule
  ],
  templateUrl: './category-form.component.html',
  styles: [`
    .dialog-form-layout {
      display: flex;
      flex-direction: column;
      gap: 16px;
      margin-top: 16px;
    }
    .full-width {
      width: 100%;
    }
    .toggle-container {
      padding: 8px 4px;
    }
  `]
})
export class CategoryFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private categoryService = inject(CategoryService);
  private uiService = inject(UiService);
  private dialogRef = inject(MatDialogRef<CategoryFormComponent>);
  private data = inject<Category | null>(MAT_DIALOG_DATA);

  form!: FormGroup;
  isEditMode = !!this.data;

  ngOnInit(): void {
    this.initForm();
  }

  private initForm(): void {
    this.form = this.fb.group({
      name: [
        this.data?.name || '',
        [Validators.required, Validators.maxLength(100)]
      ],
      isActive: [
        this.data ? this.data.isActive : true
      ]
    });
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const rawValues = this.form.getRawValue();
    this.uiService.showLoader('Salvataggio in corso...');

    if (this.isEditMode && this.data) {
      // MODALITÀ MODIFICA (PUT)
      const updatePayload = {
        name: rawValues.name,
        isActive: rawValues.isActive
      };

      this.categoryService.update(this.data.id, updatePayload).subscribe({
        next: () => {
          this.uiService.hideLoader();
          this.uiService.showToast('Categoria aggiornata con successo!');
          this.dialogRef.close(true); // Chiude la modale restituendo "true" per ricaricare la grid
        },
        error: (err) => {
          this.uiService.hideLoader();
          this.uiService.showToast(err.error?.message || 'Errore durante l\'aggiornamento.', 'error');
        }
      });
    } else {
      // MODALITÀ CREAZIONE (POST)
      const createPayload = {
        name: rawValues.name
      };

      this.categoryService.create(createPayload).subscribe({
        next: () => {
          this.uiService.hideLoader();
          this.uiService.showToast('Nuova categoria creata con successo!');
          this.dialogRef.close(true);
        },
        error: (err) => {
          this.uiService.hideLoader();
          this.uiService.showToast(err.error?.message || 'Errore durante la creazione.', 'error');
        }
      });
    }
  }
}
