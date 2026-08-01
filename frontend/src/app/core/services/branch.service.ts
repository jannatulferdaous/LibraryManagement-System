import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Branch } from '../models/domain.models';
import { PagedResult } from '../models/paged-result.model';

export interface CreateBranchRequest {
  name: string;
  address: string;
  phone: string;
}

@Injectable({ providedIn: 'root' })
export class BranchService {
  private readonly baseUrl = `${environment.apiUrl}/branches`;

  constructor(private http: HttpClient) {}

  getAll(searchTerm = '', page = 1, pageSize = 20): Observable<PagedResult<Branch>> {
    return this.http.get<PagedResult<Branch>>(this.baseUrl, {
      params: { searchTerm, page, pageSize }
    });
  }

  create(request: CreateBranchRequest): Observable<string> {
    return this.http.post<string>(this.baseUrl, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
