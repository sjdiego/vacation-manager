import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { VacationService, VacationDto, CreateVacationDto, UpdateVacationDto, VacationType, VacationStatus } from '@app/core/services';
import { UserService } from '@app/core/services/user.service';
import { ToastService } from '@app/core/services/toast.service';

@Component({
  selector: 'app-vacations',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './vacations.component.html',
  styleUrls: ['./vacations.component.css']
})
export class VacationsComponent implements OnInit {
  vacations: VacationDto[] = [];
  vacationForm: FormGroup;
  showModal = false;
  editingVacation: VacationDto | null = null;
  isLoading = true;
  canCreateVacation = false;
  
  VacationType = VacationType;
  VacationStatus = VacationStatus;

  constructor(
    private vacationService: VacationService,
    private userService: UserService,
    private toastService: ToastService,
    private fb: FormBuilder
  ) {
    this.vacationForm = this.fb.group({
      startDate: ['', Validators.required],
      endDate: ['', Validators.required],
      type: ['', Validators.required],
      notes: ['']
    });
  }

  ngOnInit(): void {
    this.loadVacations();
    this.checkUserPermissions();
  }

  checkUserPermissions(): void {
    this.canCreateVacation = this.userService.isUserInTeam();
  }

  loadVacations(): void {
    this.isLoading = true;
    this.vacationService.getMyVacations().subscribe({
      next: (vacations) => {
        this.vacations = vacations;
        this.isLoading = false;
      },
      error: (error) => {
        this.toastService.error(error.message || 'Failed to load vacations');
        this.isLoading = false;
      }
    });
  }

  openCreateModal(): void {
    this.editingVacation = null;
    this.vacationForm.reset();
    this.showModal = true;
  }

  editVacation(vacation: VacationDto): void {
    this.editingVacation = vacation;
    this.vacationForm.patchValue({
      startDate: this.formatDateForInput(vacation.startDate),
      endDate: this.formatDateForInput(vacation.endDate),
      type: vacation.type,
      notes: vacation.notes
    });
    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.editingVacation = null;
    this.vacationForm.reset();
  }

  getTypeName(type: number | VacationType): string {
    const typeMap: { [key: number]: string } = {
      0: 'Vacation',
      1: 'Sick Leave',
      2: 'Personal Day',
      3: 'Compensatory Time',
      4: 'Other'
    };
    return typeMap[type] || 'Unknown';
  }

  getStatusName(status: string | number | VacationStatus): string {
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

  onSubmit(): void {
    if (this.vacationForm.invalid) {
      return;
    }

    const formData = {
      startDate: new Date(this.vacationForm.value.startDate),
      endDate: new Date(this.vacationForm.value.endDate),
      type: Number(this.vacationForm.value.type),
      notes: this.vacationForm.value.notes
    };

    if (this.editingVacation) {
      this.updateVacation(formData);
    } else {
      this.createVacation(formData);
    }
  }

  private createVacation(data: CreateVacationDto): void {
    this.vacationService.createVacation(data).subscribe({
      next: () => {
        this.toastService.success('Vacation request created successfully!');
        this.closeModal();
        this.loadVacations();
      },
      error: (error) => {
        this.toastService.error(error.message || 'Failed to create vacation request');
      }
    });
  }

  private updateVacation(data: UpdateVacationDto): void {
    if (!this.editingVacation) return;

    this.vacationService.updateVacation(this.editingVacation.id, data).subscribe({
      next: () => {
        this.toastService.success('Vacation request updated successfully!');
        this.closeModal();
        this.loadVacations();
      },
      error: (error) => {
        this.toastService.error(error.message || 'Failed to update vacation request');
      }
    });
  }

  deleteVacation(vacationId: string): void {
    if (!confirm('Are you sure you want to delete this vacation request?')) {
      return;
    }

    this.vacationService.deleteVacation(vacationId).subscribe({
      next: () => {
        this.toastService.success('Vacation request deleted successfully!');
        this.loadVacations();
      },
      error: (error) => {
        this.toastService.error(error.message || 'Failed to delete vacation request');
      }
    });
  }

  private formatDateForInput(date: Date | string): string {
    const d = new Date(date);
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${d.getFullYear()}-${month}-${day}`;
  }
}

