import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'dashboard',
    pathMatch: 'full'
  },
  {
    path: 'login',
    loadComponent: () => import('./pages/login/login').then((module) => module.Login)
  },
  {
    path: 'dashboard',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/dashboard/dashboard').then((module) => module.Dashboard)
  },
  {
    path: 'projects/:projectId',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/project/project').then((module) => module.ProjectPage)
  },
  {
    path: '**',
    redirectTo: 'dashboard'
  }
];
