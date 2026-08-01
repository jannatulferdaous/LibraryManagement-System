import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class BorrowingService {
  private readonly baseUrl = `${environment.apiUrl}/borrowing`;

  constructor(private http: HttpClient) {}

  borrow(bookCopyId: string, memberId: string): Observable<{ loanId: string }> {
    return this.http.post<{ loanId: string }>(`${this.baseUrl}/borrow`, { bookCopyId, memberId });
  }

  return(loanId: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/return`, { loanId });
  }
}
