import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  CurrentUser,
  Issue,
  IssueComment,
  IssueHistoryEntry,
  IssuePriority,
  IssueStatus,
  PagedResult,
  Project,
  ProjectMember,
  ProjectRole
} from '../models/workspace';

export interface IssueQuery {
  search?: string;
  status?: IssueStatus | '';
  priority?: IssuePriority | '';
  page?: number;
  pageSize?: number;
}

export interface CreateIssueRequest {
  title: string;
  description: string;
  priority: IssuePriority;
  dueDate: string | null;
  projectId: number;
  assignedToId: string | null;
}

export interface UpdateIssueRequest {
  title: string;
  description: string;
  status: IssueStatus;
  priority: IssuePriority;
  dueDate: string | null;
  assignedToId: string | null;
}

@Injectable({ providedIn: 'root' })
export class Api {
  private readonly http = inject(HttpClient);

  getCurrentUser(): Observable<CurrentUser> {
    return this.http.get<CurrentUser>('/api/users/me');
  }

  getProjects(): Observable<Project[]> {
    return this.http.get<Project[]>('/api/projects');
  }

  getProject(id: number): Observable<Project> {
    return this.http.get<Project>(`/api/projects/${id}`);
  }

  createProject(name: string, description: string): Observable<Project> {
    return this.http.post<Project>('/api/projects', { name, description });
  }

  updateProject(id: number, name: string, description: string): Observable<Project> {
    return this.http.put<Project>(`/api/projects/${id}`, { name, description });
  }

  deleteProject(id: number): Observable<void> {
    return this.http.delete<void>(`/api/projects/${id}`);
  }

  getProjectMembers(id: number): Observable<ProjectMember[]> {
    return this.http.get<ProjectMember[]>(`/api/projects/${id}/members`);
  }

  addProjectMember(id: number, email: string, role: ProjectRole): Observable<void> {
    return this.http.post<void>(`/api/projects/${id}/members`, { email, role });
  }

  removeProjectMember(id: number, userId: string): Observable<void> {
    return this.http.delete<void>(`/api/projects/${id}/members/${encodeURIComponent(userId)}`);
  }

  getIssues(projectId: number, query: IssueQuery = {}): Observable<PagedResult<Issue>> {
    let params = new HttpParams()
      .set('page', query.page ?? 1)
      .set('pageSize', query.pageSize ?? 100);
    if (query.search?.trim()) params = params.set('search', query.search.trim());
    if (query.status) params = params.set('status', query.status);
    if (query.priority) params = params.set('priority', query.priority);

    return this.http.get<PagedResult<Issue>>(`/api/issues/project/${projectId}`, { params });
  }

  getIssue(id: number): Observable<Issue> {
    return this.http.get<Issue>(`/api/issues/${id}`);
  }

  createIssue(issue: CreateIssueRequest): Observable<Issue> {
    return this.http.post<Issue>('/api/issues', issue);
  }

  updateIssue(id: number, issue: UpdateIssueRequest): Observable<Issue> {
    return this.http.put<Issue>(`/api/issues/${id}`, issue);
  }

  deleteIssue(id: number): Observable<void> {
    return this.http.delete<void>(`/api/issues/${id}`);
  }

  getComments(issueId: number): Observable<IssueComment[]> {
    return this.http.get<IssueComment[]>(`/api/issues/${issueId}/comments`);
  }

  addComment(issueId: number, content: string): Observable<IssueComment> {
    return this.http.post<IssueComment>(`/api/issues/${issueId}/comments`, { content });
  }

  getHistory(issueId: number): Observable<IssueHistoryEntry[]> {
    return this.http.get<IssueHistoryEntry[]>(`/api/issues/${issueId}/history`);
  }
}
