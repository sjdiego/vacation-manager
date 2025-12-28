import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { VacationService, VacationDto, VacationStatus } from '../../core/services/vacation.service';
import { UserService, UserDto } from '../../core/services/user.service';
import { LoadingService } from '../../core/services/loading.service';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-approvals',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './approvals.component.html',
  styleUrls: ['./approvals.component.css']
})
export class ApprovalsComponent implements OnInit, OnDestroy {
  pendingVacations: VacationDto[] = [];
  currentUser: UserDto | null = null;
  selectedVacation: VacationDto | null = null;
  rejectReason = '';
  private destroy$ = new Subject<void>();

  constructor(
    private vacationService: VacationService,
    private userService: UserService,
    private loadingService: LoadingService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.loadCurrentUser();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadCurrentUser(): void {
    this.userService.currentUser$
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (user) => {
          this.currentUser = user;
          if (user && user.isManager) {
            this.loadPendingVacations();
          } else {
            this.toastService.error('Only managers can approve vacations');
          }
        },
        error: () => {
          this.toastService.error('Failed to load current user');
        }
      });
  }

  private loadPendingVacations(): void {
    this.loadingService.setLoading(true);
    this.vacationService.getTeamPendingVacations()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (vacations) => {
          this.pendingVacations = vacations;
          this.loadingService.setLoading(false);
        },
        error: () => {
          this.toastService.error('Failed to load pending vacations');
          this.loadingService.setLoading(false);
        }
      });
  }

  selectVacation(vacation: VacationDto): void {
    this.selectedVacation = vacation;
    this.rejectReason = '';
  }

  approve(): void {
    if (!this.selectedVacation) return;

    this.loadingService.setLoading(true);
    this.vacationService.approveVacation(this.selectedVacation.id, true)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.toastService.success('Vacation approved');
          this.selectedVacation = null;
          this.loadPendingVacations();
        },
        error: () => {
          this.toastService.error('Failed to approve vacation');
          this.loadingService.setLoading(false);
        }
      });
  }

  reject(): void {
    if (!this.selectedVacation) return;

    this.loadingService.setLoading(true);
    this.vacationService.approveVacation(this.selectedVacation.id, false, this.rejectReason)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.toastService.success('Vacation rejected');
          this.selectedVacation = null;
          this.rejectReason = '';
          this.loadPendingVacations();
        },
        error: () => {
          this.toastService.error('Failed to reject vacation');
          this.loadingService.setLoading(false);
        }
      });
  }

  getStatusLabel(status: VacationStatus): string {
    const labels: Record<VacationStatus, string> = {
      [VacationStatus.Pending]: 'Pending',
      [VacationStatus.Approved]: 'Approved',
      [VacationStatus.Rejected]: 'Rejected',
      [VacationStatus.Cancelled]: 'Cancelled'
    };
    return labels[status] || 'Unknown';
  }

  formatDate(date: Date): string {
    return new Date(date).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }
}
