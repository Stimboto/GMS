export interface AuthResponse {
  token: string;
}

export interface User {
  id: number;
  fullName: string;
  email: string;
  role: string;
  profileImageUrl?: string;
  emailNotificationsEnabled?: boolean;
}

export interface Grievance {
  id: number;
  trackingId: string;
  title: string;
  description: string;
  summary?: string;
  status: string;
  priority: string;
  category: string;
  departmentId: number;
  departmentName?: string;
  submittedByUserId?: number;
  submittedByName?: string;
  assignedOfficerId?: number;
  assignedOfficerName?: string;
  createdAt: string;
  updatedAt?: string;
  satisfactionRating?: number;
  feedbackRemarks?: string;
  attachments?: Attachment[];
  statusHistories?: StatusHistory[];
}

export interface Attachment {
  id: number;
  fileName: string;
  filePath: string;
  createdAt: string;
}

export interface StatusHistory {
  id: number;
  status: string;
  remarks: string;
  imageUrl?: string;
  changedAt: string;
  changedByUserId: number;
  changedByUserName?: string;
}

export interface Notification {
  id: number;
  message: string;
  isRead: boolean;
  createdAt: string;
  userId: number;
}

export interface DashboardStats {
  // Existing fields
  totalGrievances: number;
  pendingGrievances?: number;
  resolvedGrievances?: number;
  
  // Admin fields
  totalUsers?: number;
  citizens?: number;
  officers?: number;
  admins?: number;
  activeUsers?: number;
  inactiveUsers?: number;
  
  submitted?: number;
  assigned?: number;
  inReview?: number;
  resolved?: number;
  closed?: number;
  
  todaysNew?: number;
  todaysResolved?: number;
  todaysClosed?: number;

  departmentStatistics?: any[];
  officerStatistics?: any[];
}

export interface ChartData {
  labels: string[];
  values: number[];
}

export interface MonthlyGrievance {
  month: string;
  count: number;
}
