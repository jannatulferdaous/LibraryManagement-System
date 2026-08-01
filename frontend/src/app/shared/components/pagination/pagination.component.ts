import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-pagination',
  standalone: true,
  template: `
    <div class="pagination">
      <button [disabled]="page <= 1" (click)="pageChange.emit(page - 1)">← Previous</button>
      <span>Page {{ page }} of {{ totalPages || 1 }} ({{ totalCount }} total)</span>
      <button [disabled]="page >= totalPages" (click)="pageChange.emit(page + 1)">Next →</button>
    </div>
  `,
  styles: [`
    .pagination {
      display: flex;
      align-items: center;
      gap: 1rem;
      justify-content: center;
      padding: 1rem;
      font-size: 0.9rem;
      color: #475569;
    }
    button {
      padding: 0.4rem 0.9rem;
      border: 1px solid #cbd5e1;
      background: white;
      border-radius: 4px;
      cursor: pointer;
    }
    button:disabled { opacity: 0.4; cursor: not-allowed; }
    button:not(:disabled):hover { background: #f1f5f9; }
  `]
})
export class PaginationComponent {
  @Input() page = 1;
  @Input() totalPages = 1;
  @Input() totalCount = 0;
  @Output() pageChange = new EventEmitter<number>();
}
