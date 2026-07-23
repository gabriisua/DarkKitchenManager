import {Component, inject, OnInit} from '@angular/core';
import {CommonModule} from '@angular/common';
import {FormBuilder, ReactiveFormsModule, Validators} from '@angular/forms';
import {Router, RouterModule} from '@angular/router';
import {AuthService} from '../../../core/services/auth.service';
import {CookieService} from '../../../core/services/cookie.service';
import { firstValueFrom } from 'rxjs';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import {MatIcon} from '@angular/material/icon';
import {MatCard} from '@angular/material/card';
import { AuthResponse } from '../../../shared/models/api.models';

@Component({
  selector: 'app-login-page',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatCheckboxModule,
    MatProgressSpinnerModule,
    RouterModule,
    MatIcon,
    MatCard
  ],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private auth = inject(AuthService);
  private cookieService = inject(CookieService);

  hidePassword = true;
  isSubmitting = false;
  serverError = '';

  form = this.fb.group({
    email: ['', Validators.required],
    password: ['', [Validators.required, Validators.minLength(6)]],
    remember: [false]
  });

  ngOnInit() {
    const savedEmail = this.cookieService.getCookie('rememberEmail');

    if (savedEmail) {
      this.form.patchValue({
        email: decodeURIComponent(savedEmail),
        remember: true
      });
    }
  }

  get f() {
    return this.form.controls;
  }

  async submit() {
    if (this.form.invalid || this.isSubmitting) return;
    this.isSubmitting = true;
    this.serverError = '';

    try {
      const loginData = {
        email: this.f['email'].value!,
        password: this.f['password'].value!,
        remember: this.f['remember'].value
      };

      const res: AuthResponse = await firstValueFrom(this.auth.login(loginData));

      if (!res?.token) {
        this.serverError = 'Invalid response from server';
        return;
      }

      if (loginData.remember) {
        this.cookieService.setCookie('rememberEmail', decodeURIComponent(loginData.email), 30);
      } else {
        this.cookieService.deleteCookie('rememberEmail');
      }

      this.auth.loadUser();

      await this.router.navigateByUrl('/dashboard');

    } catch (e: any) {
      this.serverError = 'Credenziali non valide';
    } finally {
      this.isSubmitting = false;
    }
  }
}
