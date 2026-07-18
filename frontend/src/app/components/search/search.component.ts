import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { finalize } from 'rxjs';
import { ApiService, AvailableRoute } from '../../services/api';

interface Partner {
  name: string;
  logo: string;
}

@Component({
  selector: 'app-search',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './search.component.html',
  styleUrls: ['./search.component.css'],
})
export class SearchComponent implements OnInit {
  readonly cities: string[] = [
    'Abbottabad', 'Aliabad (Hunza)', 'Attock', 'Badin', 'Bahawalnagar', 'Bahawalpur',
    'Bannu', 'Chakwal', 'Chaman', 'Chiniot', 'Dera Ghazi Khan', 'Dera Ismail Khan',
    'Faisalabad', 'Gilgit', 'Gujranwala', 'Gujrat', 'Gwadar', 'Hafizabad', 'Hyderabad',
    'Islamabad', 'Jacobabad', 'Jhang', 'Jhelum', 'Karachi', 'Kasur', 'Khanewal',
    'Khuzdar', 'Kohat', 'Kotli', 'Lahore', 'Larkana', 'Lodhran', 'Loralai', 'Mansehra',
    'Mardan', 'Mianwali', 'Mingora', 'Mirpur', 'Mirpur Khas', 'Multan', 'Murree',
    'Muzaffarabad', 'Muzaffargarh', 'Narowal', 'Nawabshah', 'Nowshera', 'Okara',
    'Peshawar', 'Quetta', 'Rahim Yar Khan', 'Rawalpindi', 'Sahiwal', 'Sargodha',
    'Sheikhupura', 'Shikarpur', 'Sialkot', 'Sibi', 'Skardu', 'Sukkur', 'Taxila',
    'Thatta', 'Toba Tek Singh', 'Turbat', 'Vehari', 'Wah Cantt', 'Zhob',
  ];

  readonly partners: Partner[] = [
    { name: 'Daewoo Express', logo: '/Daewoo.png' },
    { name: 'Faisal Movers', logo: '/faisal.jpg' },
    { name: 'Niazi Express', logo: '/Niazi.png' },
    { name: 'Road Master', logo: '/road.jpg' },
    { name: 'Kainat Travels', logo: '/kainat.jpg' },
    { name: 'Bilal Travels', logo: '/bilal.png' },
    { name: 'Skyways', logo: '/sky.jpg' },
    { name: 'Warraich Express', logo: '/warraich.png' },
  ];

  from = '';
  to = '';
  date = '';
  returnDate = '';
  journeyType: 'one-way' | 'round-trip' = 'one-way';

  results: any[] = [];
  minDate = '';
  searched = false;
  loading = false;
  searchError = '';

  availableRoutes: AvailableRoute[] = [];
  routeQuery = '';
  routesLoading = false;
  routesError = '';
  hasMoreRoutes = true;
  readonly routePageSize = 36;
  private routeOffset = 0;

  selectedCompanies: Record<string, boolean> = {};
  maxPrice = 0;
  departureWindow = 'all';
  busType = 'all';

  constructor(private readonly api: ApiService) {}

  ngOnInit(): void {
    const today = new Date();
    this.minDate = this.toDateInput(today);
    this.date = this.toDateInput(new Date(today.getFullYear(), today.getMonth(), today.getDate() + 1));
    this.loadAvailableRoutes(true);
  }

  get companies(): string[] {
    return [...new Set(this.results.map((bus) => bus.companyName).filter(Boolean))].sort();
  }

  get maximumResultPrice(): number {
    const prices = this.results.map((bus) => Number(bus.price) || 0);
    return Math.max(...prices, 0);
  }

  get filteredResults(): any[] {
    return this.results.filter((bus) => {
      const selected = Object.entries(this.selectedCompanies)
        .filter(([, enabled]) => enabled)
        .map(([company]) => company);
      const companyMatches = selected.length === 0 || selected.includes(bus.companyName);
      const priceMatches = !this.maxPrice || Number(bus.price) <= this.maxPrice;
      const typeMatches = this.busType === 'all' || this.getBusType(bus) === this.busType;
      const departureMatches = this.matchesDepartureWindow(bus.startTime);
      return companyMatches && priceMatches && typeMatches && departureMatches;
    });
  }

  get routeHeading(): string {
    if (!this.from || !this.to) return 'Available buses';
    return `${this.from} to ${this.to} buses`;
  }

  get routeDateLabel(): string {
    if (!this.date) return 'selected date';
    return new Intl.DateTimeFormat('en-PK', {
      weekday: 'short',
      day: 'numeric',
      month: 'short',
      year: 'numeric',
    }).format(new Date(`${this.date}T12:00:00`));
  }

  setJourneyType(type: 'one-way' | 'round-trip'): void {
    this.journeyType = type;
    if (type === 'one-way') this.returnDate = '';
  }

  swapCities(): void {
    [this.from, this.to] = [this.to, this.from];
  }

  onTravelDateChanged(): void {
    if (this.returnDate && this.returnDate < this.date) this.returnDate = '';
    this.loadAvailableRoutes(true);
  }

  selectAvailableRoute(route: AvailableRoute): void {
    this.from = route.from;
    this.to = route.to;
    this.onSearch();
  }

  searchDynamicRoutes(): void {
    this.loadAvailableRoutes(true);
  }

  loadMoreRoutes(): void {
    this.loadAvailableRoutes(false);
  }

  toggleCompany(company: string, checked: boolean): void {
    this.selectedCompanies = { ...this.selectedCompanies, [company]: checked };
  }

  scrollToSearch(): void {
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  resetFilters(): void {
    this.selectedCompanies = {};
    this.maxPrice = this.maximumResultPrice;
    this.departureWindow = 'all';
    this.busType = 'all';
  }

  onSearch(): void {
    this.searchError = '';

    if (!this.from || !this.to || !this.date) {
      this.searchError = 'Select departure, arrival and travel date.';
      return;
    }

    if (this.from === this.to) {
      this.searchError = 'Departure and arrival cities must be different.';
      return;
    }

    if (this.journeyType === 'round-trip' && !this.returnDate) {
      this.searchError = 'Select a return date for the round trip.';
      return;
    }

    this.loading = true;
    this.searched = false;
    this.results = [];

    this.api
      .searchBuses(this.from, this.to, this.date)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (res) => {
          this.results = res ?? [];
          this.searched = true;
          this.maxPrice = this.maximumResultPrice;
          this.selectedCompanies = {};
          setTimeout(() => document.getElementById('search-results')?.scrollIntoView({ behavior: 'smooth' }));
        },
        error: (err) => {
          console.error('API error:', err);
          this.results = [];
          this.searchError = err?.error?.title || err?.error?.message || 'We could not load buses. Confirm the backend API is running and try again.';
          this.searched = true;
        },
      });
  }

  getBusType(bus: any): string {
    const price = Number(bus.price) || 0;
    if (price >= 3600) return 'Business';
    if (price >= 3000) return 'Executive';
    return 'Standard';
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

  getDuration(startTime: string, arrivalTime: string): string {
    const start = new Date(startTime).getTime();
    const arrival = new Date(arrivalTime).getTime();
    if (!Number.isFinite(start) || !Number.isFinite(arrival) || arrival <= start) return 'Direct trip';

    const minutes = Math.round((arrival - start) / 60000);
    const hours = Math.floor(minutes / 60);
    const remaining = minutes % 60;
    return `${hours}h ${remaining}m`;
  }

  trackBySchedule(_: number, bus: any): string {
    return bus.busScheduleId;
  }

  trackByRoute(_: number, route: AvailableRoute): string {
    return `${route.from}-${route.to}`;
  }

  private loadAvailableRoutes(reset: boolean): void {
    if (!this.date) return;

    if (reset) {
      this.routeOffset = 0;
      this.availableRoutes = [];
      this.hasMoreRoutes = true;
    }

    if (!this.hasMoreRoutes || this.routesLoading) return;

    this.routesLoading = true;
    this.routesError = '';

    this.api
      .getAvailableRoutes(this.date, this.routePageSize, this.routeOffset, this.routeQuery)
      .pipe(finalize(() => (this.routesLoading = false)))
      .subscribe({
        next: (routes) => {
          const page = routes ?? [];
          this.availableRoutes = reset ? page : [...this.availableRoutes, ...page];
          this.routeOffset = this.availableRoutes.length;
          this.hasMoreRoutes = page.length === this.routePageSize;
        },
        error: (error) => {
          console.error('Available routes failed:', error);
          this.routesError = error?.error?.title || 'Routes could not be loaded. Start the backend and open https://localhost:7196/swagger once to trust the certificate.';
          this.hasMoreRoutes = false;
        },
      });
  }

  private matchesDepartureWindow(startTime: string): boolean {
    if (this.departureWindow === 'all') return true;
    const hour = new Date(startTime).getHours();
    if (this.departureWindow === 'morning') return hour >= 5 && hour < 12;
    if (this.departureWindow === 'afternoon') return hour >= 12 && hour < 17;
    if (this.departureWindow === 'evening') return hour >= 17 && hour < 21;
    return hour >= 21 || hour < 5;
  }

  private toDateInput(date: Date): string {
    const yyyy = date.getFullYear();
    const mm = String(date.getMonth() + 1).padStart(2, '0');
    const dd = String(date.getDate()).padStart(2, '0');
    return `${yyyy}-${mm}-${dd}`;
  }
}
