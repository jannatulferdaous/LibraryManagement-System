import { Component, OnInit } from '@angular/core';
import { DatePipe } from '@angular/common';
import { NavbarComponent } from '../../shared/components/navbar/navbar.component';
import { ReportService } from '../../core/services/report.service';
import { MostBorrowedBookReport, OverdueLoanReport } from '../../core/models/domain.models';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [DatePipe, NavbarComponent],
  template: `
    <app-navbar />
    <div class="page">
      <div class="section-header">
        <h1>Overdue Loans</h1>
        <div class="export-buttons">
          <button (click)="exportOverdueLoans()" [disabled]="exportingExcel">
            {{ exportingExcel ? 'Exporting...' : '⬇ Export to Excel' }}
          </button>
          <button (click)="exportOverdueLoansPdf()" [disabled]="exportingPdf">
            {{ exportingPdf ? 'Exporting...' : '⬇ Export to PDF' }}
          </button>
        </div>
      </div>

      <table>
        <thead><tr><th>Member</th><th>Book</th><th>Due Date</th><th>Days Overdue</th></tr></thead>
        <tbody>
          @for (row of overdueLoans; track row.loanId) {
            <tr>
              <td>{{ row.memberName }} <span class="muted">({{ row.memberEmail }})</span></td>
              <td>{{ row.bookTitle }}</td>
              <td>{{ row.dueDate | date:'mediumDate' }}</td>
              <td class="overdue">{{ row.daysOverdue }} days</td>
            </tr>
          } @empty {
            <tr><td colspan="4" class="empty">No overdue loans. 🎉</td></tr>
          }
        </tbody>
      </table>

      <h1 class="section-spacer">Most Borrowed Books</h1>
      <table>
        <thead><tr><th>Title</th><th>Author</th><th>Times Borrowed</th></tr></thead>
        <tbody>
          @for (row of mostBorrowed; track row.title) {
            <tr>
              <td>{{ row.title }}</td>
              <td>{{ row.author }}</td>
              <td>{{ row.timesBorrowed }}</td>
            </tr>
          } @empty {
            <tr><td colspan="3" class="empty">No borrowing history yet.</td></tr>
          }
        </tbody>
      </table>
    </div>
  `,
  styles: [`
    .page { padding: 1.5rem 2rem; max-width: 1000px; margin: 0 auto; }
    .section-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1rem; }
    .export-buttons { display: flex; gap: 0.5rem; }
    .section-spacer { margin-top: 2.5rem; }
    h1 { font-size: 1.15rem; margin: 0; }
    button { padding: 0.5rem 1rem; background: #1e293b; color: white; border: none; border-radius: 6px; cursor: pointer; font-size: 0.85rem; }
    button:disabled { opacity: 0.5; cursor: not-allowed; }
    table { width: 100%; border-collapse: collapse; margin-bottom: 1rem; }
    th, td { text-align: left; padding: 0.6rem 0.75rem; border-bottom: 1px solid #e2e8f0; font-size: 0.9rem; }
    th { color: #64748b; font-weight: 600; font-size: 0.8rem; text-transform: uppercase; }
    .muted { color: #94a3b8; font-size: 0.8rem; }
    .overdue { color: #dc2626; font-weight: 600; }
    .empty { text-align: center; color: #94a3b8; padding: 2rem !important; }
  `]
})
export class ReportsComponent implements OnInit {
  overdueLoans: OverdueLoanReport[] = [];
  mostBorrowed: MostBorrowedBookReport[] = [];
  exportingExcel = false;
  exportingPdf = false;

  constructor(private reportService: ReportService) {}

  ngOnInit(): void {
    this.reportService.getOverdueLoans().subscribe((data) => (this.overdueLoans = data));
    this.reportService.getMostBorrowedBooks(10).subscribe((data) => (this.mostBorrowed = data));
  }

  exportOverdueLoans(): void {
    this.exportingExcel = true;
    this.reportService.exportOverdueLoans().subscribe({
      next: (blob) => {
        this.downloadBlob(blob, `overdue-loans-${this.today()}.xlsx`);
        this.exportingExcel = false;
      },
      error: () => (this.exportingExcel = false)
    });
  }

  exportOverdueLoansPdf(): void {
    this.exportingPdf = true;
    this.reportService.exportOverdueLoansPdf().subscribe({
      next: (blob) => {
        this.downloadBlob(blob, `overdue-loans-${this.today()}.pdf`);
        this.exportingPdf = false;
      },
      error: () => (this.exportingPdf = false)
    });
  }

  private downloadBlob(blob: Blob, fileName: string): void {
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    a.click();
    window.URL.revokeObjectURL(url);
  }

  private today(): string {
    return new Date().toISOString().slice(0, 10);
  }
}
