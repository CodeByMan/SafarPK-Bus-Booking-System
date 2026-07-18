import { Routes } from '@angular/router';
import { SearchComponent } from './components/search/search.component';
import { SeatPlanComponent } from './components/seat-plan/seat-plan.component';

export const routes: Routes = [
  { path: '', redirectTo: 'search', pathMatch: 'full' },
  { path: 'search', component: SearchComponent },
  { path: 'seatplan/:scheduleId', component: SeatPlanComponent }

];
