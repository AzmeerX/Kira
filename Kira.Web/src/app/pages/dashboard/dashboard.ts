import { Component, OnInit, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth';
import { Api } from '../../services/api';
import { CurrentUser, Project } from '../../models/workspace';

@Component({
  selector: 'app-dashboard',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class Dashboard implements OnInit {
  private readonly api = inject(Api);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly projects = signal<Project[]>([]);
  readonly currentUser = signal<CurrentUser | null>(null);
  readonly isLoading = signal(true);
  readonly hasError = signal(false);
  readonly showCreateForm = signal(false);
  readonly isCreating = signal(false);
  readonly createError = signal('');
  readonly projectForm = new FormGroup({
    name: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(100)] }),
    description: new FormControl('', { nonNullable: true, validators: [Validators.maxLength(500)] })
  });

  ngOnInit(): void {
    this.loadProjects();
    this.api.getCurrentUser().subscribe({ next: user => this.currentUser.set(user) });
  }

  loadProjects(): void {
    this.isLoading.set(true);
    this.hasError.set(false);
    this.api.getProjects().subscribe({
      next: (projects) => {
        this.projects.set(projects);
        this.isLoading.set(false);
      },
      error: () => {
        this.hasError.set(true);
        this.isLoading.set(false);
      }
    });
  }

  createProject(): void {
    if (this.projectForm.invalid || this.isCreating()) {
      this.projectForm.markAllAsTouched();
      return;
    }

    this.isCreating.set(true);
    this.createError.set('');
    const { name, description } = this.projectForm.getRawValue();
    this.api.createProject(name.trim(), description.trim()).subscribe({
      next: (project) => {
        this.projects.update((projects) => [...projects, project]);
        this.projectForm.reset();
        this.showCreateForm.set(false);
        this.isCreating.set(false);
      },
      error: () => {
        this.createError.set('Could not create the project. Please try again.');
        this.isCreating.set(false);
      }
    });
  }

  toggleCreateForm(): void {
    this.createError.set('');
    this.showCreateForm.update((visible) => !visible);
  }

  logout(): void {
    this.auth.logout();
    void this.router.navigate(['/login']);
  }
}
