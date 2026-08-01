import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { NavbarComponent } from '../../shared/components/navbar/navbar.component';
import { PaginationComponent } from '../../shared/components/pagination/pagination.component';
import { BranchService } from '../../core/services/branch.service';
import { Branch } from '../../core/models/domain.models';
import { PagedResult } from '../../core/models/paged-result.model';

@Component({
  selector: 'app-branches',
  standalone: true,
  imports: [ReactiveFormsModule, NavbarComponent, PaginationComponent],
  template: `
    <app-navbar />
    <div class="page">
      <h1>Branches</h1>

      <form class="create-form" [formGroup]="form" (ngSubmit)="createBranch()">
        <input formControlName="name" placeholder="Branch name" />
        <input formControlName="address" placeholder="Address" />
        <input formControlName="phone" placeholder="Phone" />
        <button type="submit" [disabled]="form.invalid">+ Add Branch</button>
      </form>

      @if (errorMessage) { <div class="error">{{ errorMessage }}</div> }

      <table>
        <thead><tr><th>Name</th><th>Address</th><th>Phone</th><th></th></tr></thead>
        <tbody>
          @for (branch of result?.items ?? []; track branch.id) {
            <tr>
              <td>{{ branch.name }}</td>
              <td>{{ branch.address }}</td>
              <td>{{ branch.phone }}</td>
              <td><button class="delete-btn" (click)="deleteBranch(branch.id)">Delete</button></td>
            </tr>
          } @empty {
            <tr><td colspan="4" class="empty">No branches found.</td></tr>
          }
        </tbody>
      </table>

      <app-pagination [page]="page" [totalPages]="result?.totalPages ?? 1" [totalCount]="result?.totalCount ?? 0" (pageChange)="onPageChange($event)" />
    </div>
  `,
  styles: [`
    .page { padding: 1.5rem 2rem; max-width: 900px; margin: 0 auto; }
    h1 { margin-bottom: 1rem; }
    .create-form { display: flex; gap: 0.5rem; flex-wrap: wrap; margin-bottom: 1.5rem; padding: 1rem; background: #f8fafc; border-radius: 8px; }
    .create-form input { padding: 0.5rem; border: 1px solid #cbd5e1; border-radius: 6px; flex: 1; min-width: 150px; }
    .create-form button { padding: 0.5rem 1rem; background: #1e293b; color: white; border: none; border-radius: 6px; cursor: pointer; }
    .create-form button:disabled { opacity: 0.5; cursor: not-allowed; }
    table { width: 100%; border-collapse: collapse; }
    th, td { text-align: left; padding: 0.6rem 0.75rem; border-bottom: 1px solid #e2e8f0; font-size: 0.9rem; }
    th { color: #64748b; font-weight: 600; font-size: 0.8rem; text-transform: uppercase; }
    .delete-btn { background: none; border: 1px solid #fca5a5; color: #dc2626; padding: 0.3rem 0.7rem; border-radius: 4px; cursor: pointer; font-size: 0.8rem; }
    .empty { text-align: center; color: #94a3b8; padding: 2rem !important; }
    .error { padding: 0.6rem; background: #fef2f2; color: #b91c1c; border-radius: 6px; margin-bottom: 1rem; font-size: 0.85rem; }
  `]
})
export class BranchesComponent implements OnInit {
  result: PagedResult<Branch> | null = null;
  page = 1;
  errorMessage = '';

  private fb = inject(FormBuilder);

  form = this.fb.group({
    name: ['', Validators.required],
    address: ['', Validators.required],
    phone: ['', Validators.required]
  });

  constructor(private branchService: BranchService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.branchService.getAll('', this.page, 20).subscribe({
      next: (res) => (this.result = res),
      error: () => (this.errorMessage = 'Failed to load branches.')
    });
  }

  onPageChange(newPage: number): void {
    this.page = newPage;
    this.load();
  }

  createBranch(): void {
    if (this.form.invalid) return;

    this.branchService.create(this.form.value as any).subscribe({
      next: () => {
        this.form.reset();
        this.load();
      },
      error: () => (this.errorMessage = 'Failed to create branch.')
    });
  }

  deleteBranch(id: string): void {
    if (!confirm('Delete this branch?')) return;

    this.branchService.delete(id).subscribe({
      next: () => this.load(),
      error: () => (this.errorMessage = 'Failed to delete branch.')
    });
  }
}
