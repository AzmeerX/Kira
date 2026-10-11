import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { Api } from '../../services/api';
import { AuthService } from '../../services/auth';
import {
  Issue,
  IssueComment,
  IssueHistoryEntry,
  IssuePriority,
  IssueStatus,
  Project,
  ProjectMember,
  ProjectRole
} from '../../models/workspace';

type ProjectTab = 'issues' | 'members' | 'settings';
type IssuePanelTab = 'comments' | 'activity';

@Component({
  selector: 'app-project',
  imports: [DatePipe, ReactiveFormsModule, RouterLink],
  templateUrl: './project.html',
  styleUrl: './project.css'
})
export class ProjectPage implements OnInit {
  private readonly api = inject(Api);
  private readonly auth = inject(AuthService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private issueRequestVersion = 0;

  readonly statuses: IssueStatus[] = ['Todo', 'InProgress', 'InReview', 'Done'];
  readonly priorities: IssuePriority[] = ['Low', 'Medium', 'High', 'Urgent'];
  readonly project = signal<Project | null>(null);
  readonly members = signal<ProjectMember[]>([]);
  readonly issues = signal<Issue[]>([]);
  readonly issueTotal = signal(0);
  readonly issuePage = signal(1);
  readonly issueTotalPages = signal(0);
  readonly isLoadingIssues = signal(false);
  readonly selectedIssue = signal<Issue | null>(null);
  readonly comments = signal<IssueComment[]>([]);
  readonly history = signal<IssueHistoryEntry[]>([]);
  readonly currentUserId = signal('');
  readonly isManager = computed(() => this.members().some(member =>
    member.userId === this.currentUserId() && member.role === 'Manager'));
  readonly activeTab = signal<ProjectTab>('issues');
  readonly issuePanelTab = signal<IssuePanelTab>('comments');
  readonly isLoading = signal(true);
  readonly isSaving = signal(false);
  readonly deletingIssueId = signal<number | null>(null);
  readonly isAddingComment = signal(false);
  readonly removingMemberId = signal<string | null>(null);
  readonly pageError = signal('');
  readonly actionError = signal('');
  readonly issueSearch = signal('');
  readonly issueStatus = signal<IssueStatus | ''>('');
  readonly issuePriority = signal<IssuePriority | ''>('');
  readonly showIssueForm = signal(false);
  readonly projectId = signal(0);

  readonly issueForm = new FormGroup({
    title: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(150)] }),
    description: new FormControl('', { nonNullable: true, validators: [Validators.maxLength(2000)] }),
    status: new FormControl<IssueStatus>('Todo', { nonNullable: true }),
    priority: new FormControl<IssuePriority>('Medium', { nonNullable: true }),
    dueDate: new FormControl('', { nonNullable: true }),
    assignedToId: new FormControl('', { nonNullable: true })
  });
  readonly commentForm = new FormGroup({
    content: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(2000)] })
  });
  readonly memberForm = new FormGroup({
    email: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.email] }),
    role: new FormControl<ProjectRole>('Developer', { nonNullable: true })
  });
  readonly projectForm = new FormGroup({
    name: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(100)] }),
    description: new FormControl('', { nonNullable: true, validators: [Validators.maxLength(500)] })
  });

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const id = Number(params.get('projectId'));
      if (!Number.isInteger(id) || id < 1) {
        void this.router.navigate(['/dashboard']);
        return;
      }

      this.projectId.set(id);
      this.loadProject();
    });
  }

  loadProject(): void {
    this.isLoading.set(true);
    this.pageError.set('');
    forkJoin({
      project: this.api.getProject(this.projectId()),
      members: this.api.getProjectMembers(this.projectId()),
      me: this.api.getCurrentUser()
    }).subscribe({
      next: ({ project, members, me }) => {
        this.project.set(project);
        this.members.set(members);
        this.currentUserId.set(me.id);
        this.projectForm.reset({ name: project.name, description: project.description });
        this.isLoading.set(false);
        this.loadIssues();
      },
      error: () => {
        this.pageError.set('We could not open this project. It may have been removed or you may not have access.');
        this.isLoading.set(false);
      }
    });
  }

  loadIssues(): void {
    const requestVersion = ++this.issueRequestVersion;
    this.isLoadingIssues.set(true);
    this.api.getIssues(this.projectId(), {
      search: this.issueSearch(),
      status: this.issueStatus(),
      priority: this.issuePriority(),
      page: this.issuePage(),
      pageSize: 10
    }).subscribe({
      next: result => {
        if (requestVersion !== this.issueRequestVersion) return;
        this.issues.set(result.items);
        this.issueTotal.set(result.totalCount);
        this.issueTotalPages.set(result.totalPages);
        this.isLoadingIssues.set(false);
      },
      error: () => {
        if (requestVersion !== this.issueRequestVersion) return;
        this.actionError.set('Could not load issues. Try refreshing the page.');
        this.isLoadingIssues.set(false);
      }
    });
  }

  setSearch(value: string): void {
    this.issueSearch.set(value);
    this.issuePage.set(1);
    this.loadIssues();
  }

  setStatus(value: string): void {
    this.issueStatus.set(value as IssueStatus | '');
    this.issuePage.set(1);
    this.loadIssues();
  }

  setPriority(value: string): void {
    this.issuePriority.set(value as IssuePriority | '');
    this.issuePage.set(1);
    this.loadIssues();
  }

  changePage(page: number): void {
    if (page < 1 || page > this.issueTotalPages()) return;
    this.issuePage.set(page);
    this.loadIssues();
  }

  statusLabel(status: IssueStatus): string {
    return ({ Todo: 'To do', InProgress: 'In progress', InReview: 'In review', Done: 'Done' })[status];
  }

  memberName(userId: string | null): string {
    if (!userId) return 'Unassigned';
    return this.members().find(member => member.userId === userId)?.displayName ?? 'Project member';
  }

  startNewIssue(): void {
    this.selectedIssue.set(null);
    this.actionError.set('');
    this.issueForm.reset({ title: '', description: '', status: 'Todo', priority: 'Medium', dueDate: '', assignedToId: '' });
    this.showIssueForm.set(true);
  }

  openIssue(issue: Issue): void {
    this.actionError.set('');
    this.selectedIssue.set(issue);
    this.showIssueForm.set(false);
    this.issueForm.reset({
      title: issue.title,
      description: issue.description,
      status: issue.status,
      priority: issue.priority,
      dueDate: issue.dueDate?.slice(0, 10) ?? '',
      assignedToId: issue.assignedToId ?? ''
    });
    this.issuePanelTab.set('comments');
    forkJoin({ comments: this.api.getComments(issue.id), history: this.api.getHistory(issue.id) }).subscribe({
      next: ({ comments, history }) => {
        this.comments.set(comments);
        this.history.set(history);
      },
      error: () => this.actionError.set('Could not load this issue’s discussion.')
    });
  }

  closeIssue(): void {
    this.selectedIssue.set(null);
    this.showIssueForm.set(false);
    this.actionError.set('');
  }

  saveIssue(): void {
    if (this.issueForm.invalid || this.isSaving()) {
      this.issueForm.markAllAsTouched();
      return;
    }

    const project = this.project();
    if (!project) return;
    this.isSaving.set(true);
    this.actionError.set('');
    const value = this.issueForm.getRawValue();
    const selected = this.selectedIssue();

    if (selected) {
      this.api.updateIssue(selected.id, {
        title: value.title.trim(),
        description: value.description.trim(),
        status: value.status,
        priority: value.priority,
        dueDate: value.dueDate || null,
        assignedToId: value.assignedToId || null
      }).subscribe({
        next: issue => {
          this.issues.update(issues => issues.map(item => item.id === issue.id ? issue : item));
          this.selectedIssue.set(issue);
          this.isSaving.set(false);
          this.loadIssues();
          this.api.getHistory(issue.id).subscribe(entries => this.history.set(entries));
        },
        error: () => {
          this.actionError.set('Could not save this issue. Check the fields and try again.');
          this.isSaving.set(false);
        }
      });
      return;
    }

    this.api.createIssue({
      title: value.title.trim(),
      description: value.description.trim(),
      priority: value.priority,
      dueDate: value.dueDate || null,
      projectId: project.id,
      assignedToId: value.assignedToId || null
    }).subscribe({
      next: () => {
        this.isSaving.set(false);
        this.showIssueForm.set(false);
        this.loadIssues();
      },
      error: () => {
        this.actionError.set('Could not create the issue. Check the fields and try again.');
        this.isSaving.set(false);
      }
    });
  }

  addComment(): void {
    const issue = this.selectedIssue();
    const content = this.commentForm.controls.content.value.trim();
    if (!issue || !content || this.isAddingComment()) return;

    this.isAddingComment.set(true);
    this.api.addComment(issue.id, content).subscribe({
      next: () => {
        this.api.getComments(issue.id).subscribe(comments => this.comments.set(comments));
        this.commentForm.reset();
        this.isAddingComment.set(false);
      },
      error: () => {
        this.actionError.set('Could not add your comment. Try again.');
        this.isAddingComment.set(false);
      }
    });
  }

  deleteIssue(): void {
    const issue = this.selectedIssue();
    if (!issue || this.deletingIssueId() !== null) return;
    if (!window.confirm(`Delete “${issue.title}”? This also removes its comments and history.`)) return;

    this.deletingIssueId.set(issue.id);
    this.actionError.set('');
    this.api.deleteIssue(issue.id).subscribe({
      next: () => {
        this.deletingIssueId.set(null);
        this.closeIssue();
        this.loadIssues();
      },
      error: () => {
        this.actionError.set('Could not delete this issue. Try again.');
        this.deletingIssueId.set(null);
      }
    });
  }

  addMember(): void {
    if (this.memberForm.invalid) {
      this.memberForm.markAllAsTouched();
      return;
    }

    const { email, role } = this.memberForm.getRawValue();
    this.actionError.set('');
    this.api.addProjectMember(this.projectId(), email.trim(), role).subscribe({
      next: () => {
        this.memberForm.reset({ email: '', role: 'Developer' });
        this.api.getProjectMembers(this.projectId()).subscribe(members => this.members.set(members));
      },
      error: () => this.actionError.set('Could not add that member. Make sure the account exists and the email is correct.')
    });
  }

  removeMember(member: ProjectMember): void {
    if (this.removingMemberId() !== null) return;
    if (!window.confirm(`Remove ${member.displayName} from this project?`)) return;

    this.removingMemberId.set(member.userId);
    this.actionError.set('');
    this.api.removeProjectMember(this.projectId(), member.userId).subscribe({
      next: () => {
        if (member.userId === this.currentUserId()) {
          void this.router.navigate(['/dashboard']);
          return;
        }
        this.members.update(members => members.filter(item => item.userId !== member.userId));
        this.removingMemberId.set(null);
      },
      error: () => {
        this.actionError.set('Could not remove this member. The project must keep at least one manager.');
        this.removingMemberId.set(null);
      }
    });
  }

  saveProject(): void {
    if (this.projectForm.invalid || !this.project()) {
      this.projectForm.markAllAsTouched();
      return;
    }
    const { name, description } = this.projectForm.getRawValue();
    this.api.updateProject(this.projectId(), name.trim(), description.trim()).subscribe({
      next: project => this.project.set(project),
      error: () => this.actionError.set('Could not update the project details.')
    });
  }

  deleteProject(): void {
    const project = this.project();
    if (!project || !window.confirm(`Delete “${project.name}” and all of its issues?`)) return;
    this.api.deleteProject(project.id).subscribe({
      next: () => void this.router.navigate(['/dashboard']),
      error: () => this.actionError.set('Could not delete this project.')
    });
  }

  logout(): void {
    this.auth.logout();
    void this.router.navigate(['/login']);
  }
}
