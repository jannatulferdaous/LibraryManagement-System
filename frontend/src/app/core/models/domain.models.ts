export interface Book {
  id: string;
  title: string;
  author: string;
  isbn: string;
  totalCopies: number;
  availableCopies: number;
}

export interface Branch {
  id: string;
  name: string;
  address: string;
  phone: string;
}

export enum MembershipType {
  Standard = 'Standard',
  Student = 'Student',
  Premium = 'Premium'
}

export interface Member {
  id: string;
  fullName: string;
  email: string;
  membershipType: MembershipType;
  isActive: boolean;
  outstandingFines: number;
  activeLoanCount: number;
}

export interface OverdueLoanReport {
  loanId: string;
  memberName: string;
  memberEmail: string;
  bookTitle: string;
  dueDate: string;
  daysOverdue: number;
}

export interface MostBorrowedBookReport {
  title: string;
  author: string;
  timesBorrowed: number;
}
