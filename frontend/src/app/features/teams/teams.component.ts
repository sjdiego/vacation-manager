import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TeamService, TeamDto, CreateTeamDto, UpdateTeamDto, UserService, UserDto } from '@app/core/services';
import { ToastService } from '@app/core/services/toast.service';

@Component({
  selector: 'app-teams',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './teams.component.html',
  styleUrl: './teams.component.css'
})
export class TeamsComponent implements OnInit {
  teams: TeamDto[] = [];
  teamMembers: UserDto[] = [];
  allUsers: UserDto[] = [];
  teamForm: FormGroup;
  showModal = false;
  selectedTeam: TeamDto | null = null;
  editingTeam: TeamDto | null = null;
  isLoading = true;
  selectedUserToAdd = '';
  isAddingMember = false;

  constructor(
    private teamService: TeamService,
    private userService: UserService,
    private toastService: ToastService,
    private fb: FormBuilder
  ) {
    this.teamForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(2)]],
      description: ['']
    });
  }

  ngOnInit(): void {
    this.loadTeams();
    this.loadAllUsers();
  }

  loadTeams(): void {
    this.isLoading = true;
    this.teamService.getAllTeams().subscribe({
      next: (teams) => {
        this.teams = teams;
        this.isLoading = false;
      },
      error: (error) => {
        this.toastService.error(error.message || 'Failed to load teams');
        this.isLoading = false;
      }
    });
  }

  loadAllUsers(): void {
    this.userService.getAllUsers().subscribe({
      next: (users) => {
        this.allUsers = users;
      },
      error: () => {
        // Silently fail, this is optional
      }
    });
  }

  openCreateModal(): void {
    this.editingTeam = null;
    this.teamForm.reset();
    this.showModal = true;
  }

  openEditModal(team: TeamDto): void {
    this.editingTeam = team;
    this.teamForm.patchValue({
      name: team.name,
      description: team.description
    });
    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.editingTeam = null;
    this.teamForm.reset();
  }

  onSubmit(): void {
    if (this.teamForm.invalid) {
      return;
    }

    const formData = this.teamForm.value;

    if (this.editingTeam) {
      this.updateTeam(formData);
    } else {
      this.createTeam(formData);
    }
  }

  private createTeam(data: CreateTeamDto): void {
    this.teamService.createTeam(data).subscribe({
      next: () => {
        this.toastService.success('Team created successfully!');
        this.closeModal();
        this.loadTeams();
      },
      error: (error) => {
        this.toastService.error(error.message || 'Failed to create team');
      }
    });
  }

  private updateTeam(data: UpdateTeamDto): void {
    if (!this.editingTeam) return;

    this.teamService.updateTeam(this.editingTeam.id, data).subscribe({
      next: () => {
        this.toastService.success('Team updated successfully!');
        this.closeModal();
        this.loadTeams();
      },
      error: (error) => {
        this.toastService.error(error.message || 'Failed to update team');
      }
    });
  }

  deleteTeam(teamId: string): void {
    if (!confirm('Are you sure you want to delete this team?')) {
      return;
    }

    this.teamService.deleteTeam(teamId).subscribe({
      next: () => {
        this.toastService.success('Team deleted successfully!');
        this.loadTeams();
      },
      error: (error) => {
        this.toastService.error(error.message || 'Failed to delete team');
      }
    });
  }

  viewTeamDetails(team: TeamDto): void {
    this.selectedTeam = team;
    this.selectedUserToAdd = '';
    this.loadTeamMembers(team.id);
  }

  private loadTeamMembers(teamId: string): void {
    this.userService.getUsersByTeam(teamId).subscribe({
      next: (members) => {
        this.teamMembers = members;
      },
      error: () => {
        this.teamMembers = [];
      }
    });
  }

  closeDetailsModal(): void {
    this.selectedTeam = null;
    this.teamMembers = [];
    this.selectedUserToAdd = '';
  }

  getAvailableUsersForTeam(): UserDto[] {
    if (!this.selectedTeam) return [];
    return this.allUsers.filter(user => user.teamId !== this.selectedTeam?.id);
  }

  addMemberToTeam(): void {
    if (!this.selectedTeam || !this.selectedUserToAdd) {
      this.toastService.error('Please select a user');
      return;
    }

    this.isAddingMember = true;
    this.userService.assignUserToTeam(this.selectedUserToAdd, this.selectedTeam.id).subscribe({
      next: () => {
        this.toastService.success('Member added to team');
        this.loadTeamMembers(this.selectedTeam!.id);
        this.selectedUserToAdd = '';
        this.isAddingMember = false;
      },
      error: (error) => {
        this.toastService.error(error.message || 'Failed to add member');
        this.isAddingMember = false;
      }
    });
  }

  removeMemberFromTeam(memberId: string): void {
    const member = this.teamMembers.find(m => m.id === memberId);
    if (!member || !confirm(`Remove ${member.displayName} from team?`)) {
      return;
    }

    this.userService.removeUserFromTeamAsManager(memberId).subscribe({
      next: () => {
        this.toastService.success('Member removed from team');
        this.loadTeamMembers(this.selectedTeam!.id);
      },
      error: (error) => {
        this.toastService.error(error.message || 'Failed to remove member');
      }
    });
  }
}
