import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { VacationService, VacationDto } from '@app/core/services';
import { ToastService } from '@app/core/services/toast.service';

@Component({
  selector: 'app-team-calendar',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './team-calendar.component.html',
  styleUrl: './team-calendar.component.css'
})
export class TeamCalendarComponent implements OnInit {
  vacations: VacationDto[] = [];
  isLoading = true;
  currentDate = new Date();
  currentMonth: number;
  currentYear: number;
  daysInMonth: number[] = [];
  firstDayOfMonth: number = 0;
  
  private readonly monthNames = [
    'January', 'February', 'March', 'April', 'May', 'June',
    'July', 'August', 'September', 'October', 'November', 'December'
  ];

  constructor(
    private vacationService: VacationService,
    private toastService: ToastService
  ) {
    this.currentMonth = this.currentDate.getMonth();
    this.currentYear = this.currentDate.getFullYear();
  }

  ngOnInit(): void {
    this.loadTeamVacations();
    this.generateCalendarDays();
  }

  loadTeamVacations(): void {
    this.isLoading = true;
    const startDate = new Date(this.currentYear, this.currentMonth, 1);
    const endDate = new Date(this.currentYear, this.currentMonth + 1, 0);
    
    this.vacationService.getTeamVacations(startDate, endDate).subscribe({
      next: (vacations) => {
        this.vacations = vacations;
        this.isLoading = false;
      },
      error: (error) => {
        this.toastService.error(error.message || 'Failed to load team vacations');
        this.isLoading = false;
      }
    });
  }

  generateCalendarDays(): void {
    const firstDay = new Date(this.currentYear, this.currentMonth, 1);
    this.firstDayOfMonth = firstDay.getDay();
    
    const lastDay = new Date(this.currentYear, this.currentMonth + 1, 0);
    const daysCount = lastDay.getDate();
    
    this.daysInMonth = Array.from({ length: daysCount }, (_, i) => i + 1);
  }

  previousMonth(): void {
    if (this.currentMonth === 0) {
      this.currentMonth = 11;
      this.currentYear--;
    } else {
      this.currentMonth--;
    }
    this.generateCalendarDays();
    this.loadTeamVacations();
  }

  nextMonth(): void {
    if (this.currentMonth === 11) {
      this.currentMonth = 0;
      this.currentYear++;
    } else {
      this.currentMonth++;
    }
    this.generateCalendarDays();
    this.loadTeamVacations();
  }

  getVacationsForDay(day: number): VacationDto[] {
    const date = new Date(this.currentYear, this.currentMonth, day);
    return this.vacations.filter(vacation => {
      const start = new Date(vacation.startDate);
      const end = new Date(vacation.endDate);
      return date >= start && date <= end;
    });
  }

  getMonthYear(): string {
    return `${this.monthNames[this.currentMonth]} ${this.currentYear}`;
  }

  getStatusName(status: string | number): string {
    const statusMap: { [key: string]: string } = {
      '0': 'Pending',
      '1': 'Approved',
      '2': 'Rejected',
      '3': 'Cancelled',
      'Pending': 'Pending',
      'Approved': 'Approved',
      'Rejected': 'Rejected',
      'Cancelled': 'Cancelled'
    };
    return statusMap[String(status)] || String(status);
  }

  getEmptyDays(): number {
    return this.firstDayOfMonth;
  }
}
