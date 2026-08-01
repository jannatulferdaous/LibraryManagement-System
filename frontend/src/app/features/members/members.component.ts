import { Component, OnInit, inject } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { NavbarComponent } from '../../shared/components/navbar/navbar.component';
import { PaginationComponent } from '../../shared/components/pagination/pagination.component';
import { MemberService } from '../../core/services/member.service';
import { AuthService } from '../../core/auth/auth.service';
import { UserRole } from '../../core/models/auth.models';
import { Member, MembershipType } from '../../core/models/domain.models';
import { PagedResult } from '../../core/models/paged-result.model';

@Component({
  selector: 'app-members',
  standalone: true,
  imports: [ReactiveFormsModule, DecimalPipe, NavbarComponent, PaginationComponent],
  template: `
    <app-navbar />
    <div class="page">
      <div class="page-header">
        <h1>Members</h1>
        <input class="search" placeholder="Search by name or email..." [value]="searchTerm" (input)="onSearch($any($event.target).value)" />
      </div>

      <form class="create-form" [formGroup]="form" (ngSubmit)="createMember()">
        <input formControlName="fullName" placeholder="Full name" />
        <input formControlName="email" placeholder="Email" />
        <select formControlName="membershipType">
          <option [value]="MembershipType.Standard">Standard</option>
          <option [value]="MembershipType.Student">Student</option>
          <option [value]="MembershipType.Premium">Premium</option>
        </select>
        <button type="submit" [disabled]="form.invalid">+ Add Member</button>
      </form>

      @if (errorMessage) { <div class="error">{{ errorMessage }}</div> }

      <table>
        <thead>
          <tr><th>Name</th><th>Email</th><th>Type</th><th>Active Loans</th><th>Fines</th><th>Status</th>
            @if (auth.hasAnyRole(UserRole.Admin)) { <th></th> }
          </tr>
        </thead>
        <tbody>
          @for (member of result?.items ?? []; track member.id) {
            <tr>
              <td>{{ member.fullName }}</td>
              <td>{{ member.email }}</td>
              <td>{{ member.membershipType }}</td>
              <td>{{ member.activeLoanCount }}</td>
              <td [class.low]="member.outstandingFines > 0">{{ member.outstandingFines | number:'1.2-2' }}</td>
              <td>
                <span class="badge" [class.inactive]="!member.isActive">{{ member.isActive ? 'Active' : 'Inactive' }}</span>
              </td>
              @if (auth.hasAnyRole(UserRole.Admin)) {
                <td><button class="delete-btn" (click)="deleteMember(member.id)">Delete</button></td>
              }
            </tr>
          } @empty {
            <tr><td colspan="7" class="empty">No members found.</td></tr>
          }
        </tbody>
      </table>

      <app-pagination [page]="page" [totalPages]="result?.totalPages ?? 1" [totalCount]="result?.totalCount ?? 0" (pageChange)="onPageChange($event)" />
    </div>
  `,
  styles: [`
    .page { padding: 1.5rem 2rem; max-width: 1100px; margin: 0 auto; }
    .page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1rem; flex-wrap: wrap; gap: 0.5rem; }
    .search { padding: 0.5rem 0.75rem; border: 1px solid #cbd5e1; border-radius: 6px; min-width: 280px; }
    .create-form { display: flex; gap: 0.5rem; flex-wrap: wrap; margin-bottom: 1.5rem; padding: 1rem; background: #f8fafc; border-radius: 8px; }
    .create-form input, .create-form select { padding: 0.5rem; border: 1px solid #cbd5e1; border-radius: 6px; }
    .create-form button { padding: 0.5rem 1rem; background: #1e293b; color: white; border: none; border-radius: 6px; cursor: pointer; }
    .create-form button:disabled { opacity: 0.5; cursor: not-allowed; }
    table { width: 100%; border-collapse: collapse; }
    th, td { text-align: left; padding: 0.6rem 0.75rem; border-bottom: 1px solid #e2e8f0; font-size: 0.9rem; }
    th { color: #64748b; font-weight: 600; font-size: 0.8rem; text-transform: uppercase; }
    .low { color: #dc2626; font-weight: 600; }
    .badge { padding: 0.15rem 0.6rem; border-radius: 999px; font-size: 0.75rem; background: #dcfce7; color: #166534; }
    .badge.inactive { background: #fee2e2; color: #991b1b; }
    .delete-btn { background: none; border: 1px solid #fca5a5; color: #dc2626; padding: 0.3rem 0.7rem; border-radius: 4px; cursor: pointer; font-size: 0.8rem; }
    .empty { text-align: center; color: #94a3b8; padding: 2rem !important; }
    .error { padding: 0.6rem; background: #fef2f2; color: #b91c1c; border-radius: 6px; margin-bottom: 1rem; font-size: 0.85rem; }
  `]
})
export class MembersComponent implements OnInit {
  UserRole = UserRole;
  MembershipType = MembershipType;
  result: PagedResult<Member> | null = null;
  searchTerm = '';
  page = 1;
  errorMessage = '';
  private searchDebounce?: ReturnType<typeof setTimeout>;

  private fb = inject(FormBuilder);

  form = this.fb.group({
    fullName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    membershipType: [MembershipType.Standard, Validators.required]
  });

  constructor(private memberService: MemberService, public auth: AuthService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.memberService.getAll(this.searchTerm, this.page, 20).subscribe({
      next: (res) => (this.result = res),
      error: () => (this.errorMessage = 'Failed to load members.')
    });
  }

  onSearch(term: string): void {
    this.searchTerm = term;
    this.page = 1;
    clearTimeout(this.searchDebounce);
    this.searchDebounce = setTimeout(() => this.load(), 300);
  }

  onPageChange(newPage: number): void {
    this.page = newPage;
    this.load();
  }

  createMember(): void {
    if (this.form.invalid) return;

    this.memberService.create(this.form.value as any).subscribe({
      next: () => {
        this.form.reset({ membershipType: MembershipType.Standard });
        this.load();
      },
      error: () => (this.errorMessage = 'Failed to create member. Email may already be registered.')
    });
  }

  deleteMember(id: string): void {
    if (!confirm('Delete this member?')) return;

    this.memberService.delete(id).subscribe({
      next: () => this.load(),
      error: (err) => (this.errorMessage = err.status === 409
        ? 'Cannot delete: member has active loans or outstanding fines.'
        : 'Failed to delete member.')
    });
  }
}
