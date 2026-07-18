import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { ApiService } from '../../services/api';

interface SelectedSeat {
  id: string;
  number: string;
}

@Component({
  selector: 'app-seat-plan',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './seat-plan.component.html',
  styleUrls: ['./seat-plan.component.css'],
})
export class SeatPlanComponent implements OnInit {
  scheduleId = '';
  seats: any[] = [];
  selectedSeats: SelectedSeat[] = [];
  busName = '';
  companyName = '';
  passengerName = '';
  mobileNumber = '';
  boardingPoints: string[] = [];
  droppingPoints: string[] = [];
  boardingPoint = '';
  droppingPoint = '';
  ticketId = '';
  cancellationMobile = '';
  lastTicketIds: string[] = [];
  showCancelForm = false;
  loading = true;
  loadingError = '';
  bookingInProgress = false;
  successMessage = '';

  constructor(
    private readonly route: ActivatedRoute,
    private readonly api: ApiService,
  ) {}

  ngOnInit(): void {
    this.scheduleId = this.route.snapshot.paramMap.get('scheduleId') ?? '';
    this.loadSeatPlan();
  }


  getOperatorLogo(companyName: string): string {
    const name = (companyName || '').toLowerCase();
    if (name.includes('daewoo')) return '/Daewoo.png';
    if (name.includes('faisal')) return '/faisal.jpg';
    if (name.includes('niazi')) return '/Niazi.png';
    if (name.includes('road master')) return '/road.jpg';
    if (name.includes('kainat')) return '/kainat.jpg';
    if (name.includes('bilal')) return '/bilal.png';
    if (name.includes('sky')) return '/sky.jpg';
    if (name.includes('warraich')) return '/warraich.png';
    return '/logo.png';
  }

  get selectedSeatNumbers(): string {
    return this.selectedSeats.map((seat) => seat.number).join(', ');
  }

  getSeatLayout(): any[][] {
    const layout: any[][] = [];
    for (let index = 0; index < this.seats.length; index += 4) {
      layout.push(this.seats.slice(index, index + 4));
    }
    return layout;
  }

  toggleSeat(seat: any): void {
    if (seat.status !== 'Available') return;

    const index = this.selectedSeats.findIndex((selected) => selected.id === seat.seatId);
    if (index >= 0) {
      this.selectedSeats.splice(index, 1);
    } else {
      this.selectedSeats.push({ id: seat.seatId, number: seat.seatNumber });
    }
  }

  isSelected(seat: any): boolean {
    return this.selectedSeats.some((selected) => selected.id === seat.seatId);
  }

  getSeatClass(seat: any): string {
    if (seat.status === 'Available') {
      return this.isSelected(seat) ? 'seat seat--selected' : 'seat seat--available';
    }
    if (seat.status === 'Booked') return 'seat seat--booked';
    if (seat.status === 'Sold') return 'seat seat--sold';
    return 'seat seat--unavailable';
  }

  isValidMobile(mobile: string): boolean {
    return /^03\d{9}$/.test(mobile.trim());
  }

  bookNow(): void {
    this.submitBooking('Book');
  }

  buyNow(): void {
    this.submitBooking('Buy');
  }

  cancelTicket(): void {
    if (!this.ticketId || !this.isValidMobile(this.cancellationMobile)) return;

    this.bookingInProgress = true;
    this.api
      .cancelTicket({
        ticketId: this.ticketId.trim(),
        passengerMobile: this.cancellationMobile.trim(),
      })
      .pipe(finalize(() => (this.bookingInProgress = false)))
      .subscribe({
        next: () => {
          this.successMessage = 'Ticket cancelled successfully. The seat is available again.';
          this.ticketId = '';
          this.cancellationMobile = '';
          this.showCancelForm = false;
          this.loadSeatPlan(false);
        },
        error: (error) => {
          this.loadingError = error?.error?.message || error?.error?.title || 'Cancellation failed. Check the ticket ID and booking mobile number.';
        },
      });
  }

  private submitBooking(action: 'Book' | 'Buy'): void {
    if (!this.canSubmitBooking()) return;

    this.bookingInProgress = true;
    this.loadingError = '';
    this.successMessage = '';

    const bookingRequest = {
      busScheduleId: this.scheduleId,
      seatId: this.selectedSeats.map((seat) => seat.id),
      passengerName: this.passengerName.trim(),
      passengerMobile: this.mobileNumber.trim(),
      boardingPoint: this.boardingPoint,
      droppingPoint: this.droppingPoint,
      action,
    };

    const request$ = action === 'Buy'
      ? this.api.buySeats(bookingRequest)
      : this.api.bookSeats(bookingRequest);

    request$
      .pipe(finalize(() => (this.bookingInProgress = false)))
      .subscribe({
        next: (response) => {
          this.lastTicketIds = (response.seat ?? [])
            .map((seat: any) => seat.ticketId)
            .filter(Boolean);
          this.successMessage = action === 'Buy'
            ? 'Your ticket purchase is confirmed.'
            : 'Your seats have been reserved successfully.';
          this.selectedSeats = [];
          this.passengerName = '';
          this.mobileNumber = '';
          this.loadSeatPlan(false);
        },
        error: (error) => {
          console.error('Booking failed:', error);
          this.loadingError = error?.error?.message || error?.error?.title || 'Booking failed. The seats may have been taken; refresh and try again.';
        },
      });
  }

  private canSubmitBooking(): boolean {
    return (
      this.selectedSeats.length > 0 &&
      Boolean(this.passengerName.trim()) &&
      this.isValidMobile(this.mobileNumber) &&
      Boolean(this.boardingPoint) &&
      Boolean(this.droppingPoint)
    );
  }

  private loadSeatPlan(showLoader = true): void {
    if (!this.scheduleId) {
      this.loading = false;
      this.loadingError = 'The bus schedule could not be identified.';
      return;
    }

    if (showLoader) this.loading = true;
    this.loadingError = '';

    this.api
      .getSeatPlan(this.scheduleId)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (response) => {
          this.busName = response.busName;
          this.companyName = response.companyName;
          this.seats = (response.seats ?? []).sort((a: any, b: any) => {
            const numberA = Number.parseInt(a.seatNumber.replace(/\D/g, ''), 10);
            const numberB = Number.parseInt(b.seatNumber.replace(/\D/g, ''), 10);
            return numberA - numberB;
          });

          if (response.boardingPoint) {
            this.boardingPoints = [response.boardingPoint];
            if (!this.boardingPoint) this.boardingPoint = response.boardingPoint;
          }

          if (response.droppingPoint) {
            this.droppingPoints = [response.droppingPoint];
            if (!this.droppingPoint) this.droppingPoint = response.droppingPoint;
          }
        },
        error: (error) => {
          console.error('Seat plan failed:', error);
          this.loadingError = 'We could not load the seat plan. Confirm the backend is running.';
        },
      });
  }
}
