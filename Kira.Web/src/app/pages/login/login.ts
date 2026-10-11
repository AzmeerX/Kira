import { Component, OnInit, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import {
  ReactiveFormsModule,
  FormControl,
  FormGroup,
  Validators
} from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  accountForm = new FormGroup({
    displayName: new FormControl('', { nonNullable: true }),
    email: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.email]
    }),

    password: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required]
    })
  });

  readonly mode = signal<'login' | 'register'>('login');
  readonly errorMessage = signal('');
  readonly successMessage = signal('');
  readonly isSubmitting = signal(false);

  ngOnInit(): void {
    if (this.authService.getToken()) void this.router.navigate(['/dashboard']);
  }

  submit() {
    if (this.accountForm.invalid || this.isSubmitting()) {
      this.accountForm.markAllAsTouched();
      return;
    }

    this.errorMessage.set('');
    this.successMessage.set('');
    this.isSubmitting.set(true);

    const { displayName, email, password } = this.accountForm.getRawValue();
    if (this.mode() === 'register') {
      if (!displayName.trim() || displayName.length > 100) {
        this.errorMessage.set('Enter a name with 1 to 100 characters.');
        this.isSubmitting.set(false);
        return;
      }

      this.authService.register(displayName.trim(), email, password).subscribe({
        next: () => {
          this.mode.set('login');
          this.accountForm.controls.password.reset();
          this.successMessage.set('Your account is ready. Sign in to continue.');
          this.isSubmitting.set(false);
        },
        error: (error: unknown) => {
          this.errorMessage.set(this.accountErrorMessage(error));
          this.isSubmitting.set(false);
        }
      });
      return;
    }

    this.authService.login(email, password).subscribe({
      next: () => void this.router.navigate(['/dashboard']),
      error: (error: unknown) => {
        this.errorMessage.set(this.accountErrorMessage(error));
        this.isSubmitting.set(false);
      }
    });
  }

  toggleMode(): void {
    this.mode.update((current) => current === 'login' ? 'register' : 'login');
    this.accountForm.controls.password.setValidators(this.mode() === 'register'
      ? [Validators.required, Validators.minLength(8), Validators.pattern('(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^A-Za-z0-9]).*')]
      : [Validators.required]);
    this.accountForm.controls.password.updateValueAndValidity();
    this.errorMessage.set('');
    this.successMessage.set('');
    this.accountForm.controls.password.reset();
  }

  private accountErrorMessage(error: unknown): string {
    const action = this.mode() === 'register' ? 'create your account' : 'sign you in';
    if (!(error instanceof HttpErrorResponse)) {
      return `We could not ${action}. Please try again.`;
    }

    const payload: unknown = error.error;
    if (Array.isArray(payload)) {
      const descriptions = payload
        .map(item => this.readStringProperty(item, 'description'))
        .filter((description): description is string => description !== null);
      if (descriptions.length > 0) return descriptions.join(' ');
    }

    const message = this.readStringProperty(payload, 'message');
    if (message) return message;

    if (error.status === 0) return 'Could not reach Kira. Check that the API is running and try again.';
    if (error.status >= 500) return `The server could not ${action}. Check the API log for the error details.`;
    return `We could not ${action}. Check your details and try again.`;
  }

  private readStringProperty(value: unknown, property: string): string | null {
    if (typeof value !== 'object' || value === null || !(property in value)) return null;
    const result: unknown = (value as Record<string, unknown>)[property];
    return typeof result === 'string' && result.trim() ? result : null;
  }
}
