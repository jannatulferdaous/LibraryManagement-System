import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Member, MembershipType } from '../models/domain.models';
import { PagedResult } from '../models/paged-result.model';

export interface CreateMemberRequest {
  fullName: string;
  email: string;
  membershipType: MembershipType;
}

@Injectable({ providedIn: 'root' })
export class MemberService {
  private readonly baseUrl = `${environment.apiUrl}/members`;

  constructor(private http: HttpClient) {}

  getAll(searchTerm = '', page = 1, pageSize = 20): Observable<PagedResult<Member>> {
    return this.http.get<PagedResult<Member>>(this.baseUrl, {
      params: { searchTerm, page, pageSize }
    });
  }

  getById(id: string): Observable<Member> {
    return this.http.get<Member>(`${this.baseUrl}/${id}`);
  }

  create(request: CreateMemberRequest): Observable<string> {
    return this.http.post<string>(this.baseUrl, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
