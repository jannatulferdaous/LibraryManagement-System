import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { NavbarComponent } from '../../shared/components/navbar/navbar.component';
import { BorrowingService } from '../../core/services/borrowing.service';

@Component({
  selector: 'app-borrow-return',
  standalone: true,
  imports: [ReactiveFormsModule, NavbarComponent],
  template: `
    <app-navbar />
    <div class="page">
      <h1>Borrow / Return</h1>

      <div class="grid">
        <div class="panel">
          <h2>Borrow a Book</h2>
          <form [formGroup]="borrowForm" (ngSubmit)="borrow()">
            <label>Book Copy ID</label>
            <input formControlName="bookCopyId" placeholder="GUID of the specific copy" />
            <label>Member ID</label>
            <input formControlName="memberId" placeholder="GUID of the borrowing member" />
            <button type="submit" [disabled]="borrowForm.invalid">Borrow</button>
          </form>
          @if (borrowMessage) { <div class="message" [class.error]="borrowError">{{ borrowMessage }}</div> }
        </div>

        <div class="panel">
          <h2>Return a Book</h2>
          <form [formGroup]="returnForm" (ngSubmit)="returnBook()">
            <label>Loan ID</label>
            <input formControlName="loanId" placeholder="GUID of the loan being returned" />
            <button type="submit" [disabled]="returnForm.invalid">Return</button>
          </form>
          @if (returnMessage) { <div class="message" [class.error]="returnError">{{ returnMessage }}</div> }
        </div>
      </div>
    </div>
  `,
  styles: [`
    .page { padding: 1.5rem 2rem; max-width: 900px; margin: 0 auto; }
    h1 { margin-bottom: 1.5rem; }
    .grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(320px, 1fr)); gap: 1.5rem; }
    .panel { padding: 1.5rem; border: 1px solid #e2e8f0; border-radius: 10px; }
    .panel h2 { font-size: 1rem; margin: 0 0 1rem; }
    label { display: block; font-size: 0.8rem; font-weight: 600; color: #334155; margin: 0.5rem 0 0.25rem; }
    input { width: 100%; padding: 0.55rem; border: 1px solid #cbd5e1; border-radius: 6px; box-sizing: border-box; }
    button { width: 100%; margin-top: 1.25rem; padding: 0.6rem; background: #1e293b; color: white; border: none; border-radius: 6px; cursor: pointer; }
    button:disabled { opacity: 0.5; cursor: not-allowed; }
    .message { margin-top: 1rem; padding: 0.6rem; background: #dcfce7; color: #166534; border-radius: 6px; font-size: 0.85rem; }
    .message.error { background: #fef2f2; color: #b91c1c; }
  `]
})
export class BorrowReturnComponent {
  private fb = inject(FormBuilder);

  borrowForm = this.fb.group({
    bookCopyId: ['', Validators.required],
    memberId: ['', Validators.required]
  });
  returnForm = this.fb.group({
    loanId: ['', Validators.required]
  });

  borrowMessage = '';
  borrowError = false;
  returnMessage = '';
  returnError = false;

  constructor(private borrowingService: BorrowingService) {}

  borrow(): void {
    if (this.borrowForm.invalid) return;
    const { bookCopyId, memberId } = this.borrowForm.value;

    this.borrowingService.borrow(bookCopyId!, memberId!).subscribe({
      next: (res) => {
        this.borrowError = false;
        this.borrowMessage = `Borrowed successfully. Loan ID: ${res.loanId}`;
        this.borrowForm.reset();
      },
      error: (err) => {
        this.borrowError = true;
        this.borrowMessage = err.status === 409
          ? 'This copy was just borrowed by someone else - please retry with a different copy.'
          : (err.error?.detail ?? 'Failed to process the borrow.');
      }
    });
  }

  returnBook(): void {
    if (this.returnForm.invalid) return;
    const { loanId } = this.returnForm.value;

    this.borrowingService.return(loanId!).subscribe({
      next: () => {
        this.returnError = false;
        this.returnMessage = 'Returned successfully.';
        this.returnForm.reset();
      },
      error: (err) => {
        this.returnError = true;
        this.returnMessage = err.error?.detail ?? 'Failed to process the return.';
      }
    });
  }
}
