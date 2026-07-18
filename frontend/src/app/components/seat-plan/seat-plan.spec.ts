import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { ApiService } from '../../services/api';
import { SeatPlanComponent } from './seat-plan.component';

describe('SeatPlanComponent', () => {
  let component: SeatPlanComponent;
  let fixture: ComponentFixture<SeatPlanComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SeatPlanComponent],
      providers: [
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              paramMap: convertToParamMap({ scheduleId: 'test-schedule-id' }),
            },
          },
        },
        {
          provide: ApiService,
          useValue: {
            getSeatPlan: () => of({
              busName: 'Demo Bus',
              companyName: 'Demo Company',
              seats: [],
            }),
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(SeatPlanComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
