import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserService, UserDto, TeamService, TeamDto } from '@app/core/services';
import { ToastService } from '@app/core/services/toast.service';
import { LoadingService } from '@app/core/services/loading.service';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './users.component.html',
  styleUrl: './users.component.css'
})
export class UsersComponent implements OnInit {
  users: UserDto[] = [];
  filteredUsers: UserDto[] = [];
  teams: TeamDto[] = [];
  searchTerm = '';
  filterTeamId = '';
  selectedUser: UserDto | null = null;
  selectedTeamIdForUser = '';
  isLoading = true;
  currentUser: UserDto | null = null;
  isAssigning = false;

  constructor(
    private userService: UserService,
    private teamService: TeamService,
    private toastService: ToastService,
    private loadingService: LoadingService
  ) {}

  ngOnInit(): void {
    this.loadCurrentUser();
    this.loadUsers();
    this.loadTeams();
  }

  loadCurrentUser(): void {
    this.userService.currentUser$.subscribe({
      next: (user) => {
        this.currentUser = user;
      },
      error: (error) => {
        console.error('Failed to load current user:', error);
      }
    });
  }

  loadUsers(): void {
    this.isLoading = true;
    this.userService.getAllUsers().subscribe({
      next: (users) => {
        this.users = users;
        this.applyFilters();
        this.isLoading = false;
      },
      error: (error) => {
        this.toastService.error(error.message || 'Failed to load users');
        this.isLoading = false;
      }
    });
  }

  loadTeams(): void {
    this.teamService.getAllTeams().subscribe({
      next: (teams) => {
        this.teams = teams;
      },
      error: () => {
        // Silently fail, teams are optional for filtering
      }
    });
  }

  onSearch(): void {
    this.applyFilters();
  }

  onFilterChange(): void {
    this.applyFilters();
  }

  private applyFilters(): void {
    this.filteredUsers = this.users.filter((user) => {
      const matchesSearch =
        user.displayName.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        user.email.toLowerCase().includes(this.searchTerm.toLowerCase());

      const matchesTeam = !this.filterTeamId || user.teamId === this.filterTeamId;

      return matchesSearch && matchesTeam;
    });
  }

  getTeamName(teamId: string): string {
    return this.teams.find((t) => t.id === teamId)?.name || 'Unknown Team';
  }

  viewUser(user: UserDto): void {
    this.selectedUser = user;
    this.selectedTeamIdForUser = user.teamId || '';
  }

  closeModal(): void {
    this.selectedUser = null;
    this.selectedTeamIdForUser = '';
  }

  assignUserToTeam(): void {
    if (!this.selectedUser || !this.selectedTeamIdForUser) {
      this.toastService.error('Please select a team');
      return;
    }

    this.isAssigning = true;
    this.userService.assignUserToTeam(this.selectedUser.id, this.selectedTeamIdForUser).subscribe({
      next: () => {
        this.toastService.success(`User assigned to ${this.getTeamName(this.selectedTeamIdForUser)}`);
        this.loadUsers();
        this.closeModal();
        this.isAssigning = false;
      },
      error: (error) => {
        this.toastService.error(error.message || 'Failed to assign user to team');
        this.isAssigning = false;
      }
    });
  }

  removeUserFromTeam(): void {
    if (!this.selectedUser) return;

    if (!confirm(`Remove ${this.selectedUser.displayName} from team?`)) {
      return;
    }

    this.isAssigning = true;
    this.userService.removeUserFromTeamAsManager(this.selectedUser.id).subscribe({
      next: () => {
        this.toastService.success('User removed from team');
        this.loadUsers();
        this.closeModal();
        this.isAssigning = false;
      },
      error: (error) => {
        this.toastService.error(error.message || 'Failed to remove user from team');
        this.isAssigning = false;
      }
    });
  }
}
