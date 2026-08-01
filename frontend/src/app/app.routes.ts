import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
import { roleGuard } from './core/auth/role.guard';
import { UserRole } from './core/models/auth.models';

export const routes: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  {
    path: 'login',
    loadComponent: () => import('./features/login/login.component').then((m) => m.LoginComponent)
  },
  {
    path: 'dashboard',
    canActivate: [authGuard],
    loadComponent: () => import('./features/dashboard/dashboard.component').then((m) => m.DashboardComponent)
  },
  {
    path: 'books',
    canActivate: [authGuard],
    loadComponent: () => import('./features/books/books.component').then((m) => m.BooksComponent)
  },
  {
    path: 'members',
    canActivate: [roleGuard(UserRole.Admin, UserRole.Librarian)],
    loadComponent: () => import('./features/members/members.component').then((m) => m.MembersComponent)
  },
  {
    path: 'branches',
    canActivate: [roleGuard(UserRole.Admin)],
    loadComponent: () => import('./features/branches/branches.component').then((m) => m.BranchesComponent)
  },
  {
    path: 'borrow-return',
    canActivate: [roleGuard(UserRole.Admin, UserRole.Librarian)],
    loadComponent: () => import('./features/borrow-return/borrow-return.component').then((m) => m.BorrowReturnComponent)
  },
  {
    path: 'reservations',
    canActivate: [authGuard],
    loadComponent: () => import('./features/reservations/reservations.component').then((m) => m.ReservationsComponent)
  },
  {
    path: 'reports',
    canActivate: [roleGuard(UserRole.Admin, UserRole.BranchManager)],
    loadComponent: () => import('./features/reports/reports.component').then((m) => m.ReportsComponent)
  },
  { path: '**', redirectTo: 'dashboard' }
];
