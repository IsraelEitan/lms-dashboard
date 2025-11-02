import { useState, useEffect } from 'react';
import type { Course, CreateCourseRequest, UpdateCourseRequest } from '../../types';

interface CourseFormProps {
  course?: Course;
  onSubmit: (data: CreateCourseRequest | UpdateCourseRequest) => Promise<void>;
  onCancel: () => void;
}

const CourseForm = ({ course, onSubmit, onCancel }: CourseFormProps) => {
  const [formData, setFormData] = useState({
    code: '',
    title: '',
    description: '',
  });
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    if (course) {
      setFormData({
        code: course.code,
        title: course.title,
        description: course.description || '',
      });
    }
  }, [course]);

  const validate = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (!formData.code.trim()) {
      newErrors.code = 'Course code is required';
    } else if (formData.code.length < 2) {
      newErrors.code = 'Course code must be at least 2 characters';
    }

    if (!formData.title.trim()) {
      newErrors.title = 'Title is required';
    } else if (formData.title.length < 2) {
      newErrors.title = 'Title must be at least 2 characters';
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

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>
  ) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
    // Clear error for this field
    if (errors[name]) {
      setErrors((prev) => ({ ...prev, [name]: '' }));
    }
  };

  return (
    <form onSubmit={handleSubmit} className="form">
      <div className="form-group">
        <label htmlFor="code" className="form-label">
          Course Code *
        </label>
        <input
          type="text"
          id="code"
          name="code"
          value={formData.code}
          onChange={handleChange}
          className="form-input"
          placeholder="e.g., CS101, MATH201"
          disabled={isSubmitting}
        />
        {errors.code && <span className="form-error">{errors.code}</span>}
      </div>

      <div className="form-group">
        <label htmlFor="title" className="form-label">
          Title *
        </label>
        <input
          type="text"
          id="title"
          name="title"
          value={formData.title}
          onChange={handleChange}
          className="form-input"
          placeholder="Enter course title"
          disabled={isSubmitting}
        />
        {errors.title && <span className="form-error">{errors.title}</span>}
      </div>

      <div className="form-group">
        <label htmlFor="description" className="form-label">
          Description
        </label>
        <textarea
          id="description"
          name="description"
          value={formData.description}
          onChange={handleChange}
          className="form-textarea"
          placeholder="Enter course description (optional)"
          disabled={isSubmitting}
        />
        {errors.description && (
          <span className="form-error">{errors.description}</span>
        )}
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
        <button type="submit" className="btn btn-primary" disabled={isSubmitting}>
          {isSubmitting ? 'Saving...' : course ? 'Update Course' : 'Create Course'}
        </button>
      </div>
    </form>
  );
};

export default CourseForm;

