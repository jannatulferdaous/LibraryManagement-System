import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule],
  template: `
    <div class="login-page">
      <form class="login-card" [formGroup]="form" (ngSubmit)="submit()">
        <h1>📚 Library Management System</h1>
        <p class="subtitle">Sign in to continue</p>

        <label>Email</label>
        <input type="email" formControlName="email" placeholder="admin@library.local" />

        <label>Password</label>
        <input type="password" formControlName="password" placeholder="••••••••" />

        @if (errorMessage) {
          <div class="error">{{ errorMessage }}</div>
        }

        <button type="submit" [disabled]="form.invalid || submitting">
          {{ submitting ? 'Signing in...' : 'Sign in' }}
        </button>

        <p class="hint">Default seeded account: admin&#64;library.local / Admin&#64;123</p>
      </form>
    </div>
  `,
  styles: [`
    .login-page {
      min-height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      background: #f1f5f9;
    }
    .login-card {
      background: white;
      padding: 2.5rem;
      border-radius: 12px;
      box-shadow: 0 4px 24px rgba(0,0,0,0.08);
      width: 100%;
      max-width: 380px;
    }
    h1 { font-size: 1.25rem; margin-bottom: 0.25rem; }
    .subtitle { color: #64748b; font-size: 0.9rem; margin-bottom: 1.5rem; }
    label { display: block; font-size: 0.85rem; font-weight: 600; margin: 0.75rem 0 0.25rem; color: #334155; }
    input {
      width: 100%;
      padding: 0.6rem 0.75rem;
      border: 1px solid #cbd5e1;
      border-radius: 6px;
      font-size: 0.95rem;
      box-sizing: border-box;
    }
    button {
      width: 100%;
      margin-top: 1.5rem;
      padding: 0.7rem;
      background: #1e293b;
      color: white;
      border: none;
      border-radius: 6px;
      font-size: 0.95rem;
      cursor: pointer;
    }
    button:disabled { opacity: 0.6; cursor: not-allowed; }
    button:not(:disabled):hover { background: #334155; }
    .error {
      margin-top: 1rem;
      padding: 0.6rem;
      background: #fef2f2;
      color: #b91c1c;
      border-radius: 6px;
      font-size: 0.85rem;
    }
    .hint { margin-top: 1rem; font-size: 0.75rem; color: #94a3b8; text-align: center; }
  `]
})
export class LoginComponent {
  private fb = inject(FormBuilder);

  form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required]
  });
  submitting = false;
  errorMessage = '';

  constructor(private auth: AuthService, private router: Router) {}

  submit(): void {
    if (this.form.invalid) return;

    this.submitting = true;
    this.errorMessage = '';

    this.auth.login({
      email: this.form.value.email!,
      password: this.form.value.password!
    }).subscribe({
      next: () => this.router.navigate(['/dashboard']),
      error: (err) => {
        this.submitting = false;
        this.errorMessage = err.status === 401
          ? 'Invalid email or password.'
          : 'Something went wrong. Please try again.';
      }
    });
  }
}
