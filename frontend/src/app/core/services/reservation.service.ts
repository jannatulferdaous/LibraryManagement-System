import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ReservationService {
  private readonly baseUrl = `${environment.apiUrl}/reservations`;

  constructor(private http: HttpClient) {}

  create(bookId: string, memberId: string): Observable<{ reservationId: string }> {
    return this.http.post<{ reservationId: string }>(this.baseUrl, { bookId, memberId });
  }

  cancel(reservationId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${reservationId}`);
  }
}
