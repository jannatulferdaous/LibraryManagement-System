import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { UserRole } from '../../../core/models/auth.models';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  template: `
    <nav class="navbar">
      <div class="navbar-brand">📚 Library Management System</div>
      <div class="navbar-links">
        <a routerLink="/dashboard" routerLinkActive="active">Dashboard</a>
        <a routerLink="/books" routerLinkActive="active">Books</a>

        @if (auth.hasAnyRole(UserRole.Admin, UserRole.Librarian)) {
          <a routerLink="/members" routerLinkActive="active">Members</a>
        }

        @if (auth.hasAnyRole(UserRole.Admin)) {
          <a routerLink="/branches" routerLinkActive="active">Branches</a>
        }

        @if (auth.hasAnyRole(UserRole.Admin, UserRole.Librarian)) {
          <a routerLink="/borrow-return" routerLinkActive="active">Borrow / Return</a>
        }

        <a routerLink="/reservations" routerLinkActive="active">Reservations</a>

        @if (auth.hasAnyRole(UserRole.Admin, UserRole.BranchManager)) {
          <a routerLink="/reports" routerLinkActive="active">Reports</a>
        }
      </div>
      <div class="navbar-user">
        <span>{{ auth.currentUser()?.fullName }} ({{ auth.currentUser()?.role }})</span>
        <button (click)="auth.logout()">Logout</button>
      </div>
    </nav>
  `,
  styles: [`
    .navbar {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 0.75rem 1.5rem;
      background: #1e293b;
      color: white;
      flex-wrap: wrap;
      gap: 0.5rem;
    }
    .navbar-brand { font-weight: 600; }
    .navbar-links { display: flex; gap: 1rem; flex-wrap: wrap; }
    .navbar-links a {
      color: #cbd5e1;
      text-decoration: none;
      font-size: 0.9rem;
      padding: 0.25rem 0.5rem;
      border-radius: 4px;
    }
    .navbar-links a.active, .navbar-links a:hover { color: white; background: #334155; }
    .navbar-user { display: flex; align-items: center; gap: 0.75rem; font-size: 0.85rem; }
    .navbar-user button {
      background: #475569;
      color: white;
      border: none;
      padding: 0.35rem 0.75rem;
      border-radius: 4px;
      cursor: pointer;
    }
    .navbar-user button:hover { background: #64748b; }
  `]
})
export class NavbarComponent {
  UserRole = UserRole;

  constructor(public auth: AuthService) {}
}
