import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';

export interface AvailableRoute {
  from: string;
  to: string;
  availableBuses: number;
  availableSeats: number;
  lowestPrice: number;
  nextDeparture: string;
}

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly baseUrl = '/api';

  constructor(private readonly http: HttpClient) {}

  searchBuses(from: string, to: string, date: string) {
    return this.http.get<any[]>(`${this.baseUrl}/Search`, { params: { from, to, date } });
  }

  getAvailableRoutes(date: string, limit = 24, offset = 0, query = '') {
    let params = new HttpParams()
      .set('date', date)
      .set('limit', limit)
      .set('offset', offset);

    if (query.trim()) params = params.set('query', query.trim());

    return this.http.get<AvailableRoute[]>(`${this.baseUrl}/Search/routes`, { params });
  }

  getSeatPlan(scheduleId: string) {
    return this.http.get<any>(`${this.baseUrl}/Booking/seatplan/${scheduleId}`);
  }

  bookSeats(dto: any) {
    return this.http.post<any>(`${this.baseUrl}/Booking/book`, dto);
  }

  buySeats(dto: any) {
    return this.http.post<any>(`${this.baseUrl}/Booking/buy`, dto);
  }

  cancelTicket(dto: any) {
    return this.http.post<any>(`${this.baseUrl}/Booking/cancel`, dto);
  }
}
