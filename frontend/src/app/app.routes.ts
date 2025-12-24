import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: '',
    canActivate: [authGuard],
    children: [
      {
        path: '',
        redirectTo: 'vacations',
        pathMatch: 'full'
      },
      {
        path: 'vacations',
        loadComponent: () => import('./features/vacations/vacations.component').then(m => m.VacationsComponent)
      },
      {
        path: 'approvals',
        loadComponent: () => import('./features/approvals/approvals.component').then(m => m.ApprovalsComponent)
      },
      {
        path: 'team-calendar',
        loadComponent: () => import('./features/team-calendar/team-calendar.component').then(m => m.TeamCalendarComponent)
      },
      {
        path: 'users',
        loadComponent: () => import('./features/users/users.component').then(m => m.UsersComponent)
      },
      {
        path: 'teams',
        loadComponent: () => import('./features/teams/teams.component').then(m => m.TeamsComponent)
      }
    ]
  },
  {
    path: '**',
    redirectTo: 'login'
  }
];
