export type IssueStatus = 'Todo' | 'InProgress' | 'InReview' | 'Done';
export type IssuePriority = 'Low' | 'Medium' | 'High' | 'Urgent';
export type ProjectRole = 'Manager' | 'Developer';

export interface Project {
  id: number;
  name: string;
  description: string;
  createdAt: string;
}

export interface Person {
  id: string;
  displayName: string;
  email: string | null;
}

export interface ProjectMember {
  userId: string;
  displayName: string;
  email: string | null;
  role: ProjectRole;
}

export interface Issue {
  id: number;
  title: string;
  description: string;
  status: IssueStatus;
  priority: IssuePriority;
  dueDate: string | null;
  createdAt: string;
  updatedAt: string;
  projectId: number;
  createdById: string;
  createdBy?: Person;
  assignedToId: string | null;
  assignedTo?: Person | null;
}

export interface PagedResult<T> {
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  items: T[];
}

export interface IssueComment {
  id: number;
  content: string;
  createdAt: string;
  issueId: number;
  userId: string;
  user: Person;
}

export interface IssueHistoryEntry {
  id: number;
  action: string;
  oldValue: string | null;
  newValue: string | null;
  createdAt: string;
  issueId: number;
  userId: string;
  user: Person;
}

export interface CurrentUser extends Person {
  role: string;
}
