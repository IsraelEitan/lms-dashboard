import { useState, useEffect } from 'react';
import { apiService } from '../../services/api';
import type { Student, Course, CreateEnrollmentRequest } from '../../types';

interface EnrollmentFormProps {
  onSubmit: (data: CreateEnrollmentRequest) => Promise<void>;
  onCancel: () => void;
}

const EnrollmentForm = ({ onSubmit, onCancel }: EnrollmentFormProps) => {
  const [formData, setFormData] = useState({
    studentId: '',
    courseId: '',
  });
  const [students, setStudents] = useState<Student[]>([]);
  const [courses, setCourses] = useState<Course[]>([]);
  const [loading, setLoading] = useState(true);
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      setLoading(true);
      const [studentsData, coursesData] = await Promise.all([
        apiService.getStudents(1, 100),
        apiService.getCourses(1, 100),
      ]);
      setStudents(studentsData.items);
      setCourses(coursesData.items);
    } catch (error) {
      console.error('Failed to load data:', error);
    } finally {
      setLoading(false);
    }
  };

  const validate = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (!formData.studentId) {
      newErrors.studentId = 'Please select a student';
    }

    if (!formData.courseId) {
      newErrors.courseId = 'Please select a course';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!validate()) return;

    setIsSubmitting(true);
    try {
      await onSubmit(formData);
    } catch (error) {
      console.error('Form submission error:', error);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
    // Clear error for this field
    if (errors[name]) {
      setErrors((prev) => ({ ...prev, [name]: '' }));
    }
  };

  if (loading) {
    return <div className="loading">Loading...</div>;
  }

  if (students.length === 0 || courses.length === 0) {
    return (
      <div className="alert alert-warning">
        <span>⚠</span>
        <span>
          {students.length === 0 && courses.length === 0
            ? 'Please create students and courses first before creating enrollments.'
            : students.length === 0
            ? 'Please create students first before creating enrollments.'
            : 'Please create courses first before creating enrollments.'}
        </span>
      </div>
    );
  }

  return (
    <form onSubmit={handleSubmit} className="form">
      <div className="form-group">
        <label htmlFor="studentId" className="form-label">
          Student *
        </label>
        <select
          id="studentId"
          name="studentId"
          value={formData.studentId}
          onChange={handleChange}
          className="form-select"
          disabled={isSubmitting}
        >
          <option value="">Select a student</option>
          {students.map((student) => (
            <option key={student.id} value={student.id}>
              {student.name} ({student.email})
            </option>
          ))}
        </select>
        {errors.studentId && <span className="form-error">{errors.studentId}</span>}
      </div>

      <div className="form-group">
        <label htmlFor="courseId" className="form-label">
          Course *
        </label>
        <select
          id="courseId"
          name="courseId"
          value={formData.courseId}
          onChange={handleChange}
          className="form-select"
          disabled={isSubmitting}
        >
          <option value="">Select a course</option>
          {courses.map((course) => (
            <option key={course.id} value={course.id}>
              {course.code} - {course.title}
            </option>
          ))}
        </select>
        {errors.courseId && <span className="form-error">{errors.courseId}</span>}
      </div>

      <div className="btn-group">
        <button
          type="button"
          onClick={onCancel}
          className="btn btn-outline"
          disabled={isSubmitting}
        >
          Cancel
        </button>
        <button type="submit" className="btn btn-success" disabled={isSubmitting}>
          {isSubmitting ? 'Enrolling...' : 'Enroll Student'}
        </button>
      </div>
    </form>
  );
};

export default EnrollmentForm;

