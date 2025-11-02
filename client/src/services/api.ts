import axios, { AxiosInstance, AxiosError } from 'axios';
import { logger } from '../utils/logger';
import type {
  Student,
  Course,
  Enrollment,
  CreateStudentRequest,
  UpdateStudentRequest,
  CreateCourseRequest,
  UpdateCourseRequest,
  CreateEnrollmentRequest,
  PagedResponse,
  ApiError,
  CourseEnrollmentReport,
  DashboardStats,
  ExportResponse
} from '../types';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5225/api/v1';

// Custom error type for network errors
export class NetworkError extends Error {
  constructor(message: string) {
    super(message);
    this.name = 'NetworkError';
  }
}

// Connection status callback type
type ConnectionStatusCallback = (connected: boolean, error?: string) => void;

class ApiService {
  private client: AxiosInstance;
  private connectionStatusCallback?: ConnectionStatusCallback;

  constructor() {
    logger.info(`Initializing API Service with base URL: ${API_BASE_URL}`);
    
    this.client = axios.create({
      baseURL: API_BASE_URL,
      headers: {
        'Content-Type': 'application/json',
      },
      timeout: 10000, // 10 second timeout
    });

    // Request interceptor for logging
    this.client.interceptors.request.use(
      (config) => {
        logger.apiRequest(config.method?.toUpperCase() || 'GET', config.url || '', config.data);
        return config;
      },
      (error) => {
        logger.error('Request interceptor error', error);
        return Promise.reject(error);
      }
    );

    // Response interceptor for logging and error handling
    this.client.interceptors.response.use(
      (response) => {
        logger.apiResponse(
          response.config.method?.toUpperCase() || 'GET',
          response.config.url || '',
          response.status,
          response.data
        );
        // Connection successful
        if (this.connectionStatusCallback) {
          this.connectionStatusCallback(true);
        }
        return response;
      },
      (error: AxiosError<ApiError>) => {
        const method = error.config?.method?.toUpperCase() || 'REQUEST';
        const url = error.config?.url || 'unknown';
        
        // Check for network errors (server not available)
        if (error.code === 'ERR_NETWORK' || error.code === 'ECONNREFUSED' || error.code === 'ECONNABORTED') {
          const networkError = new NetworkError(
            'Unable to connect to the server. Please check if the backend is running.'
          );
          logger.apiError(method, url, networkError.message);
          
          // Notify about connection failure
          if (this.connectionStatusCallback) {
            this.connectionStatusCallback(false, networkError.message);
          }
          
          throw networkError;
        }
        
        // Handle API errors (4xx, 5xx)
        if (error.response?.data) {
          logger.apiError(method, url, error.response.data);
          throw error.response.data;
        }
        
        // Handle other errors
        logger.apiError(method, url, error.message);
        throw new Error(error.message || 'An unexpected error occurred');
      }
    );
  }

  // Set connection status callback
  setConnectionStatusCallback(callback: ConnectionStatusCallback) {
    this.connectionStatusCallback = callback;
  }

  // Student endpoints
  async getStudents(pageNumber = 1, pageSize = 10): Promise<PagedResponse<Student>> {
    const response = await this.client.get<PagedResponse<Student>>('/students', {
      params: { page: pageNumber, pageSize }
    });
    return response.data;
  }

  async getStudent(id: string): Promise<Student> {
    const response = await this.client.get<Student>(`/students/${id}`);
    return response.data;
  }

  async createStudent(data: CreateStudentRequest): Promise<Student> {
    const response = await this.client.post<Student>('/students', data);
    return response.data;
  }

  async updateStudent(id: string, data: UpdateStudentRequest): Promise<Student> {
    const response = await this.client.put<Student>(`/students/${id}`, data);
    return response.data;
  }

  async deleteStudent(id: string): Promise<void> {
    await this.client.delete(`/students/${id}`);
  }

  // Course endpoints
  async getCourses(pageNumber = 1, pageSize = 10): Promise<PagedResponse<Course>> {
    const response = await this.client.get<PagedResponse<Course>>('/courses', {
      params: { page: pageNumber, pageSize }
    });
    return response.data;
  }

  async getCourse(id: string): Promise<Course> {
    const response = await this.client.get<Course>(`/courses/${id}`);
    return response.data;
  }

  async createCourse(data: CreateCourseRequest): Promise<Course> {
    const response = await this.client.post<Course>('/courses', data);
    return response.data;
  }

  async updateCourse(id: string, data: UpdateCourseRequest): Promise<Course> {
    const response = await this.client.put<Course>(`/courses/${id}`, data);
    return response.data;
  }

  async deleteCourse(id: string): Promise<void> {
    await this.client.delete(`/courses/${id}`);
  }

  // Enrollment endpoints
  async getEnrollments(pageNumber = 1, pageSize = 10): Promise<PagedResponse<Enrollment>> {
    const response = await this.client.get<PagedResponse<Enrollment>>('/enrollments', {
      params: { page: pageNumber, pageSize }
    });
    return response.data;
  }

  async getEnrollment(id: string): Promise<Enrollment> {
    const response = await this.client.get<Enrollment>(`/enrollments/${id}`);
    return response.data;
  }

  async createEnrollment(data: CreateEnrollmentRequest): Promise<Enrollment> {
    const response = await this.client.post<Enrollment>('/enrollments', data);
    return response.data;
  }

  async deleteEnrollment(id: string): Promise<void> {
    await this.client.delete(`/enrollments/${id}`);
  }

  async getStudentEnrollments(studentId: string): Promise<Enrollment[]> {
    const response = await this.client.get<Enrollment[]>(`/enrollments/student/${studentId}`);
    return response.data;
  }

  async getCourseEnrollments(courseId: string): Promise<Enrollment[]> {
    const response = await this.client.get<Enrollment[]>(`/enrollments/course/${courseId}`);
    return response.data;
  }

  // Reports endpoints
  async getDashboardStats(): Promise<DashboardStats> {
    const response = await this.client.get<DashboardStats>('/reports/dashboard-stats');
    return response.data;
  }

  async getStudentsPerCourse(): Promise<CourseEnrollmentReport[]> {
    const response = await this.client.get<CourseEnrollmentReport[]>('/reports/students-per-course');
    return response.data;
  }

  async exportStudentsPerCourse(): Promise<ExportResponse> {
    const response = await this.client.post<ExportResponse>('/reports/export/students-per-course');
    return response.data;
  }

  async listExports(): Promise<string[]> {
    const response = await this.client.get<string[]>('/reports/exports');
    return response.data;
  }

  async downloadExport(fileName: string): Promise<string> {
    const response = await this.client.get<string>(`/reports/exports/${fileName}`);
    return response.data;
  }
}

export const apiService = new ApiService();

