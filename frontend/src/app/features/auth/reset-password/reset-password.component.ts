import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';

import { ActivatedRoute, Router } from '@angular/router';

import { AuthService } from '../../../core/services/auth.service';

/* ANGULAR MATERIAL */
import { MatCardModule } from '@angular/material/card';

import { MatFormFieldModule } from '@angular/material/form-field';

import { MatInputModule } from '@angular/material/input';

import { MatButtonModule } from '@angular/material/button';

import { MatIconModule } from '@angular/material/icon';

import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
  selector: 'app-reset-password',

  standalone: true,

  templateUrl: './reset-password.component.html',

  styleUrls: ['./reset-password.component.scss'],

  imports: [
    CommonModule,
    ReactiveFormsModule,

    /* MATERIAL */
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule
  ]
})
export class ResetPasswordComponent {

  form: FormGroup;

  token: string = '';

  isSubmitting = false;

  error = '';
  success = '';

  hidePassword = true;
  hideConfirmPassword = true;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private authService: AuthService,
    private router: Router
  ) {

    this.form = this.fb.group({
      password: [
        '',
        [
          Validators.required,
          Validators.minLength(8)
        ]
      ],

      confirmPassword: [
        '',
        [
          Validators.required
        ]
      ]
    });

    this.token =
      this.route.snapshot.queryParamMap.get('token') || '';
  }

  onSubmit(): void {

    if (this.form.invalid || this.isSubmitting) {
      return;
    }

    const password = this.form.value.password;

    const confirmPassword =
      this.form.value.confirmPassword;

    this.error = '';
    this.success = '';

    /* PASSWORD CHECK */
    if (password !== confirmPassword) {

      this.error =
        'Le password non coincidono';

      return;
    }

    /* TOKEN CHECK */
    if (!this.token) {

      this.error =
        'Token mancante o non valido';

      return;
    }

    this.isSubmitting = true;

    this.authService.resetPasswordSubmit({
      token: this.token,
      password
    }).subscribe({

      next: () => {

        this.success =
          'Password aggiornata con successo';

        setTimeout(() => {

          this.router.navigate(['/']);

        }, 1500);
      },

      error: (err) => {

        console.error(err);

        this.error =
          'Token scaduto o non valido';
      },

      complete: () => {

        this.isSubmitting = false;
      }
    });
  }
}
