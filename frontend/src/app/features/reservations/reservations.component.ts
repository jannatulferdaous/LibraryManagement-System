import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { NavbarComponent } from '../../shared/components/navbar/navbar.component';
import { ReservationService } from '../../core/services/reservation.service';

@Component({
  selector: 'app-reservations',
  standalone: true,
  imports: [ReactiveFormsModule, NavbarComponent],
  template: `
    <app-navbar />
    <div class="page">
      <h1>Reservations</h1>

      <div class="grid">
        <div class="panel">
          <h2>Reserve a Book</h2>
          <form [formGroup]="createForm" (ngSubmit)="create()">
            <label>Book ID</label>
            <input formControlName="bookId" placeholder="GUID of the book" />
            <label>Member ID</label>
            <input formControlName="memberId" placeholder="GUID of the member" />
            <button type="submit" [disabled]="createForm.invalid">Reserve</button>
          </form>
          @if (createMessage) { <div class="message" [class.error]="createError">{{ createMessage }}</div> }
        </div>

        <div class="panel">
          <h2>Cancel a Reservation</h2>
          <form [formGroup]="cancelForm" (ngSubmit)="cancel()">
            <label>Reservation ID</label>
            <input formControlName="reservationId" placeholder="GUID of the reservation" />
            <button type="submit" [disabled]="cancelForm.invalid">Cancel Reservation</button>
          </form>
          @if (cancelMessage) { <div class="message" [class.error]="cancelError">{{ cancelMessage }}</div> }
        </div>
      </div>

      <p class="note">
        When a reserved book is returned, the next member in the FIFO queue is notified automatically -
        see <code>BookReturnedEventHandler</code> on the backend.
      </p>
    </div>
  `,
  styles: [`
    .page { padding: 1.5rem 2rem; max-width: 900px; margin: 0 auto; }
    h1 { margin-bottom: 1.5rem; }
    .grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(320px, 1fr)); gap: 1.5rem; }
    .panel { padding: 1.5rem; border: 1px solid #e2e8f0; border-radius: 10px; }
    .panel h2 { font-size: 1rem; margin: 0 0 1rem; }
    label { display: block; font-size: 0.8rem; font-weight: 600; color: #334155; margin: 0.5rem 0 0.25rem; }
    input { width: 100%; padding: 0.55rem; border: 1px solid #cbd5e1; border-radius: 6px; box-sizing: border-box; }
    button { width: 100%; margin-top: 1.25rem; padding: 0.6rem; background: #1e293b; color: white; border: none; border-radius: 6px; cursor: pointer; }
    button:disabled { opacity: 0.5; cursor: not-allowed; }
    .message { margin-top: 1rem; padding: 0.6rem; background: #dcfce7; color: #166534; border-radius: 6px; font-size: 0.85rem; }
    .message.error { background: #fef2f2; color: #b91c1c; }
    .note { margin-top: 2rem; font-size: 0.8rem; color: #94a3b8; }
    code { background: #f1f5f9; padding: 0.1rem 0.4rem; border-radius: 4px; }
  `]
})
export class ReservationsComponent {
  private fb = inject(FormBuilder);

  createForm = this.fb.group({
    bookId: ['', Validators.required],
    memberId: ['', Validators.required]
  });
  cancelForm = this.fb.group({
    reservationId: ['', Validators.required]
  });

  createMessage = '';
  createError = false;
  cancelMessage = '';
  cancelError = false;

  constructor(private reservationService: ReservationService) {}

  create(): void {
    if (this.createForm.invalid) return;
    const { bookId, memberId } = this.createForm.value;

    this.reservationService.create(bookId!, memberId!).subscribe({
      next: (res) => {
        this.createError = false;
        this.createMessage = `Reserved successfully. Reservation ID: ${res.reservationId}`;
        this.createForm.reset();
      },
      error: (err) => {
        this.createError = true;
        this.createMessage = err.error?.detail ?? 'Failed to create the reservation.';
      }
    });
  }

  cancel(): void {
    if (this.cancelForm.invalid) return;
    const { reservationId } = this.cancelForm.value;

    this.reservationService.cancel(reservationId!).subscribe({
      next: () => {
        this.cancelError = false;
        this.cancelMessage = 'Reservation cancelled.';
        this.cancelForm.reset();
      },
      error: (err) => {
        this.cancelError = true;
        this.cancelMessage = err.error?.detail ?? 'Failed to cancel the reservation.';
      }
    });
  }
}
