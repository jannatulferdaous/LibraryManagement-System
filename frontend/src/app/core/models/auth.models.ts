export enum UserRole {
  Admin = 'Admin',
  Librarian = 'Librarian',
  BranchManager = 'BranchManager',
  Member = 'Member'
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResult {
  token: string;
  expiresAt: string;
  fullName: string;
  email: string;
  role: UserRole;
}
