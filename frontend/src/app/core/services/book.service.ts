import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Book } from '../models/domain.models';
import { PagedResult } from '../models/paged-result.model';

export interface CreateBookRequest {
  title: string;
  author: string;
  isbn: string;
  branchId: string;
  initialCopies: number;
}

export interface UpdateBookRequest {
  title: string;
  author: string;
  isbn: string;
}

@Injectable({ providedIn: 'root' })
export class BookService {
  private readonly baseUrl = `${environment.apiUrl}/books`;

  constructor(private http: HttpClient) {}

  getAll(searchTerm = '', page = 1, pageSize = 20): Observable<PagedResult<Book>> {
    return this.http.get<PagedResult<Book>>(this.baseUrl, {
      params: { searchTerm, page, pageSize }
    });
  }

  getById(id: string): Observable<Book> {
    return this.http.get<Book>(`${this.baseUrl}/${id}`);
  }

  create(request: CreateBookRequest): Observable<string> {
    return this.http.post<string>(this.baseUrl, request);
  }

  update(id: string, request: UpdateBookRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
