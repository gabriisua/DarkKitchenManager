import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';

import {
  Router,
  RouterLink
} from '@angular/router';

import { AuthService } from '../../../core/services/auth.service';

/* ANGULAR MATERIAL */
import { MatCardModule } from '@angular/material/card';

import { MatFormFieldModule } from '@angular/material/form-field';

import { MatInputModule } from '@angular/material/input';

import { MatButtonModule } from '@angular/material/button';

import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
  selector: 'app-forgot-password',

  standalone: true,

  templateUrl: './forgot-password.component.html',

  styleUrls: ['./forgot-password.component.scss'],

  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,

    /* MATERIAL */
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatProgressSpinnerModule
  ]
})
export class ForgotPasswordComponent {

  form: FormGroup;

  message = '';
  error = '';

  isSubmitting = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {

    this.form = this.fb.group({

      email: [
        '',
        [
          Validators.required,
          Validators.email
        ]
      ]

    });
  }

  onSubmit(): void {

    if (this.form.invalid || this.isSubmitting) {
      return;
    }

    this.isSubmitting = true;

    this.error = '';
    this.message = '';

    const email = this.form.value.email;

    this.authService
      .resetPasswordRequest(email)
      .subscribe({

        next: () => {

          this.message =
            'Email inviata con successo';

          setTimeout(() => {

            this.router.navigate(['/'], {
              state: {
                successMessage:
                  'Email inviata con successo'
              }
            });

          }, 1200);
        },

        error: (err) => {

          console.error(err);

          this.error =
            'Errore durante la richiesta';
        },

        complete: () => {

          this.isSubmitting = false;
        }
      });
  }
}
