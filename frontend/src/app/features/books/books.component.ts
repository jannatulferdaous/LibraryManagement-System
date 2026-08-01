import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { NavbarComponent } from '../../shared/components/navbar/navbar.component';
import { PaginationComponent } from '../../shared/components/pagination/pagination.component';
import { BookService } from '../../core/services/book.service';
import { BranchService } from '../../core/services/branch.service';
import { AuthService } from '../../core/auth/auth.service';
import { UserRole } from '../../core/models/auth.models';
import { Book, Branch } from '../../core/models/domain.models';
import { PagedResult } from '../../core/models/paged-result.model';

@Component({
  selector: 'app-books',
  standalone: true,
  imports: [ReactiveFormsModule, NavbarComponent, PaginationComponent],
  template: `
    <app-navbar />
    <div class="page">
      <div class="page-header">
        <h1>Books</h1>
        <input
          class="search"
          type="text"
          placeholder="Search by title, author, or ISBN..."
          [value]="searchTerm"
          (input)="onSearch($any($event.target).value)"
        />
      </div>

      @if (auth.hasAnyRole(UserRole.Admin, UserRole.Librarian)) {
        <form class="create-form" [formGroup]="form" (ngSubmit)="createBook()">
          <input formControlName="title" placeholder="Title" />
          <input formControlName="author" placeholder="Author" />
          <input formControlName="isbn" placeholder="ISBN" />
          <select formControlName="branchId">
            <option value="" disabled selected>Branch...</option>
            @for (branch of branches; track branch.id) {
              <option [value]="branch.id">{{ branch.name }}</option>
            }
          </select>
          <input formControlName="initialCopies" type="number" min="0" placeholder="Copies" />
          <button type="submit" [disabled]="form.invalid">+ Add Book</button>
        </form>
      }

      @if (errorMessage) {
        <div class="error">{{ errorMessage }}</div>
      }

      <table>
        <thead>
          <tr>
            <th>Title</th><th>Author</th><th>ISBN</th><th>Available / Total</th>
            @if (auth.hasAnyRole(UserRole.Admin)) { <th></th> }
          </tr>
        </thead>
        <tbody>
          @for (book of result?.items ?? []; track book.id) {
            <tr>
              <td>{{ book.title }}</td>
              <td>{{ book.author }}</td>
              <td>{{ book.isbn }}</td>
              <td>
                <span [class.low]="book.availableCopies === 0">
                  {{ book.availableCopies }} / {{ book.totalCopies }}
                </span>
              </td>
              @if (auth.hasAnyRole(UserRole.Admin)) {
                <td><button class="delete-btn" (click)="deleteBook(book.id)">Delete</button></td>
              }
            </tr>
          } @empty {
            <tr><td colspan="5" class="empty">No books found.</td></tr>
          }
        </tbody>
      </table>

      <app-pagination
        [page]="page"
        [totalPages]="result?.totalPages ?? 1"
        [totalCount]="result?.totalCount ?? 0"
        (pageChange)="onPageChange($event)"
      />
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
    .delete-btn { background: none; border: 1px solid #fca5a5; color: #dc2626; padding: 0.3rem 0.7rem; border-radius: 4px; cursor: pointer; font-size: 0.8rem; }
    .empty { text-align: center; color: #94a3b8; padding: 2rem !important; }
    .error { padding: 0.6rem; background: #fef2f2; color: #b91c1c; border-radius: 6px; margin-bottom: 1rem; font-size: 0.85rem; }
  `]
})
export class BooksComponent implements OnInit {
  UserRole = UserRole;
  result: PagedResult<Book> | null = null;
  branches: Branch[] = [];
  searchTerm = '';
  page = 1;
  errorMessage = '';
  private searchDebounce?: ReturnType<typeof setTimeout>;

  private fb = inject(FormBuilder);

  form = this.fb.group({
    title: ['', Validators.required],
    author: ['', Validators.required],
    isbn: ['', Validators.required],
    branchId: ['', Validators.required],
    initialCopies: [1, [Validators.required, Validators.min(0)]]
  });

  constructor(
    private bookService: BookService,
    private branchService: BranchService,
    public auth: AuthService
  ) {}

  ngOnInit(): void {
    this.load();
    this.branchService.getAll('', 1, 100).subscribe({
      next: (res) => (this.branches = res.items),
      error: () => {} // branch dropdown just stays empty for non-privileged users (403)
    });
  }

  load(): void {
    this.bookService.getAll(this.searchTerm, this.page, 20).subscribe({
      next: (res) => (this.result = res),
      error: () => (this.errorMessage = 'Failed to load books.')
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

  createBook(): void {
    if (this.form.invalid) return;

    this.bookService.create(this.form.value as any).subscribe({
      next: () => {
        this.form.reset({ initialCopies: 1 });
        this.load();
      },
      error: () => (this.errorMessage = 'Failed to create book. Check the fields and try again.')
    });
  }

  deleteBook(id: string): void {
    if (!confirm('Delete this book and all its copies?')) return;

    this.bookService.delete(id).subscribe({
      next: () => this.load(),
      error: () => (this.errorMessage = 'Failed to delete book.')
    });
  }
}
