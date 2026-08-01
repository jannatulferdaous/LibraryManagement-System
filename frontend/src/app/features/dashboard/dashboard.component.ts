import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { UserRole } from '../../core/models/auth.models';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterLink],
  template: `
    <div class="page">
      <h1>Welcome, {{ auth.currentUser()?.fullName }} 👋</h1>
      <p class="role-badge">Signed in as {{ auth.currentUser()?.role }}</p>

      <div class="card-grid">
        <a routerLink="/books" class="card">
          <h2>📖 Books</h2>
          <p>Browse the catalog, manage titles and copies.</p>
        </a>

        @if (auth.hasAnyRole(UserRole.Admin, UserRole.Librarian)) {
          <a routerLink="/borrow-return" class="card">
            <h2>🔄 Borrow / Return</h2>
            <p>Process a borrow or return at the desk.</p>
          </a>
        }

        <a routerLink="/reservations" class="card">
          <h2>🔖 Reservations</h2>
          <p>Reserve a book, or manage the queue.</p>
        </a>

        @if (auth.hasAnyRole(UserRole.Admin, UserRole.Librarian)) {
          <a routerLink="/members" class="card">
            <h2>👥 Members</h2>
            <p>Manage member accounts and eligibility.</p>
          </a>
        }

        @if (auth.hasAnyRole(UserRole.Admin)) {
          <a routerLink="/branches" class="card">
            <h2>🏢 Branches</h2>
            <p>Manage library branch locations.</p>
          </a>
        }

        @if (auth.hasAnyRole(UserRole.Admin, UserRole.BranchManager)) {
          <a routerLink="/reports" class="card">
            <h2>📊 Reports</h2>
            <p>Overdue loans and most-borrowed titles.</p>
          </a>
        }
      </div>
    </div>
  `,
  styles: [`
    .page { padding: 2rem; max-width: 960px; margin: 0 auto; }
    h1 { margin-bottom: 0.25rem; }
    .role-badge { color: #64748b; margin-bottom: 2rem; }
    .card-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
      gap: 1rem;
    }
    .card {
      display: block;
      padding: 1.25rem;
      border: 1px solid #e2e8f0;
      border-radius: 10px;
      text-decoration: none;
      color: inherit;
      transition: box-shadow 0.15s, transform 0.15s;
    }
    .card:hover { box-shadow: 0 4px 16px rgba(0,0,0,0.08); transform: translateY(-2px); }
    .card h2 { font-size: 1rem; margin: 0 0 0.4rem; }
    .card p { font-size: 0.85rem; color: #64748b; margin: 0; }
  `]
})
export class DashboardComponent {
  UserRole = UserRole;
  constructor(public auth: AuthService) {}
}
