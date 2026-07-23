import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogRef, MatDialogTitle, MatDialogContent, MatDialogActions } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { PrintLabelRequest } from '../../../../shared/models/api.models';

@Component({
  selector: 'app-print-settings-dialog',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, MatDialogTitle, MatDialogContent,
    MatDialogActions, MatFormFieldModule, MatInputModule,
    MatButtonModule, MatIconModule
  ],
  templateUrl: './print-settings-dialog.component.html',
  styleUrl: './print-settings-dialog.component.css'
})
export class PrintSettingsDialogComponent {
  private fb = inject(FormBuilder);
  public dialogRef = inject(MatDialogRef<PrintSettingsDialogComponent, PrintLabelRequest | undefined>);

  form = this.fb.group({
    copies: [1, [Validators.required, Validators.min(1)]],
    pauseAfter: [0, [Validators.required, Validators.min(0)]],
    lotNumber: [null as string | null]
  });

  confirm(): void {
    if (this.form.invalid) return;
    const raw = this.form.getRawValue();
    this.dialogRef.close({
      copies: raw.copies!,
      pauseAfter: raw.pauseAfter!,
      lotNumber: raw.lotNumber || null
    });
  }

  cancel(): void {
    this.dialogRef.close();
  }
}
