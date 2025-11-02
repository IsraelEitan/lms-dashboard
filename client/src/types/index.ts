// Domain Models
export interface Student {
  id: string;
  name: string;
  email: string;
}

export interface Course {
  id: string;
  code: string;
  title: string;
  description?: string;
}

export interface Enrollment {
  id: string;
  studentId: string;
  courseId: string;
  enrollmentDate: string;
  studentName?: string;
  studentEmail?: string;
  courseTitle?: string;
  courseCode?: string;
}

// API Request/Response Types
export interface CreateStudentRequest {
  name: string;
  email: string;
}

export interface UpdateStudentRequest {
  name: string;
  email: string;
}

export interface CreateCourseRequest {
  code: string;
  title: string;
  description?: string;
}

export interface UpdateCourseRequest {
  code: string;
  title: string;
  description?: string;
}

export interface CreateEnrollmentRequest {
  studentId: string;
  courseId: string;
}

export interface PagedResponse<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  totalCount: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface ApiError {
  title: string;
  status: number;
  errors?: Record<string, string[]>;
}

// Reports
export interface CourseEnrollmentReport {
  courseId: string;
  courseCode: string;
  courseTitle: string;
  enrolledStudents: number;
}

export interface DashboardStats {
  totalStudents: number;
  totalCourses: number;
  totalEnrollments: number;
}

export interface ExportResponse {
  fileName: string;
  s3Url: string;
  exportedAt: string;
  recordCount: number;
}

