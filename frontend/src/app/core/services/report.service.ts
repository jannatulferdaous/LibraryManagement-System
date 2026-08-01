import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { MostBorrowedBookReport, OverdueLoanReport } from '../models/domain.models';

@Injectable({ providedIn: 'root' })
export class ReportService {
  private readonly baseUrl = `${environment.apiUrl}/reports`;

  constructor(private http: HttpClient) {}

  getOverdueLoans(): Observable<OverdueLoanReport[]> {
    return this.http.get<OverdueLoanReport[]>(`${this.baseUrl}/overdue-loans`);
  }

  getMostBorrowedBooks(top = 10): Observable<MostBorrowedBookReport[]> {
    return this.http.get<MostBorrowedBookReport[]>(`${this.baseUrl}/most-borrowed-books`, {
      params: { top }
    });
  }

  exportOverdueLoans(): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/overdue-loans/export`, { responseType: 'blob' });
  }

  exportOverdueLoansPdf(): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/overdue-loans/export-pdf`, { responseType: 'blob' });
  }
}
