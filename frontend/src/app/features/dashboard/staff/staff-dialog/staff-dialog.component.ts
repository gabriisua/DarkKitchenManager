import { Component, Inject, inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule, NgFor, NgIf } from '@angular/common';
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
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';

import { StaffService } from '../../../../core/services/staff.service';
import { Staff, StaffUpdateRequest } from '../../../../shared/models/api.models';

@Component({
  selector: 'app-staff-dialog',
  templateUrl: './staff-dialog.component.html',
  styleUrls: ['./staff-dialog.component.css'],
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogTitle,
    MatDialogContent,
    MatDialogActions,
    MatDialogClose,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    NgIf,
    NgFor
  ],
  styles: [`
    .dialog-form {
      display: flex;
      flex-direction: column;
      gap: 16px;
      margin-top: 10px;
    }
    .error-msg {
      color: #f44336;
      font-size: 14px;
      margin-bottom: 10px;
    }
  `]
})
export class StaffDialogComponent {
  editForm: FormGroup;
  isEditMode: boolean;
  serverError: string = '';
  availableRoles: string[] = ['MANAGER', 'ADMINISTRATOR', 'OPERATOR', 'LOGISTIC'];

  constructor(
    private fb: FormBuilder,
    private staffService: StaffService,
    public dialogRef: MatDialogRef<StaffDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: Staff | null
  ) {
    this.isEditMode = !!data;

    this.editForm = this.fb.group({
      username: [{ value: data?.username || '', disabled: this.isEditMode }, Validators.required],
      email: [data?.email || '', [Validators.required, Validators.email]],
      password: ['', this.isEditMode ? [] : [Validators.required, Validators.minLength(6)]],
      role: [data?.role || '', Validators.required]
    });
  }

  save() {
    if (this.editForm.invalid) return;

    this.serverError = '';
    const formValue = this.editForm.getRawValue();

    if (this.isEditMode && this.data) {
      const payload: StaffUpdateRequest = {
        email: formValue.email,
        role: formValue.role
      };

      if (formValue.password && formValue.password.trim() !== '') {
        payload.password = formValue.password;
      }

      this.staffService.updateStaff(this.data.id, payload).subscribe({
        next: () => this.dialogRef.close(true),
        error: (err) => this.handleError(err)
      });

    } else {
      const createPayload = {
        username: formValue.username,
        email: formValue.email,
        password: formValue.password,
        role: formValue.role
      };

      this.staffService.createStaff(createPayload).subscribe({
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
