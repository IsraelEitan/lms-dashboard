import { useState, useEffect } from 'react';
import { apiService } from '../services/api';
import type { Course, PagedResponse, ApiError } from '../types';
import Modal from '../components/Common/Modal';
import ConfirmDialog from '../components/Common/ConfirmDialog';
import CourseForm from '../components/Courses/CourseForm';
import CourseList from '../components/Courses/CourseList';
import Loading from '../components/Common/Loading';
import EmptyState from '../components/Common/EmptyState';
import Pagination from '../components/Common/Pagination';
import Alert from '../components/Common/Alert';

const Courses = () => {
  const [courses, setCourses] = useState<PagedResponse<Course> | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [currentPage, setCurrentPage] = useState(1);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedCourse, setSelectedCourse] = useState<Course | undefined>(undefined);
  const [deletingId, setDeletingId] = useState<string | undefined>(undefined);
  const [confirmDelete, setConfirmDelete] = useState<{ isOpen: boolean; course: Course | null }>({
    isOpen: false,
    course: null,
  });

  const loadCourses = async (page: number = currentPage) => {
    try {
      setLoading(true);
      setError(null);
      const data = await apiService.getCourses(page, 10);
      setCourses(data);
    } catch (err) {
      const apiError = err as ApiError;
      setError(apiError.title || 'Failed to load courses');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadCourses();
  }, [currentPage]);

  const handleCreate = () => {
    setSelectedCourse(undefined);
    setIsModalOpen(true);
  };

  const handleEdit = (course: Course) => {
    setSelectedCourse(course);
    setIsModalOpen(true);
  };

  const handleSubmit = async (data: any) => {
    try {
      setError(null);
      if (selectedCourse) {
        await apiService.updateCourse(selectedCourse.id, data);
        setSuccess('Course updated successfully');
        // Stay on current page for updates
        loadCourses();
      } else {
        await apiService.createCourse(data);
        setSuccess('Course created successfully');
        // Reset to page 1 for new courses
        setCurrentPage(1);
        await loadCourses(1);
      }
      setIsModalOpen(false);
      setTimeout(() => setSuccess(null), 3000);
    } catch (err) {
      const apiError = err as ApiError;
      setError(apiError.title || 'Failed to save course');
      throw err;
    }
  };

  const handleDeleteClick = (course: Course) => {
    setConfirmDelete({ isOpen: true, course });
  };

  const handleDeleteConfirm = async () => {
    if (!confirmDelete.course) return;

    try {
      setDeletingId(confirmDelete.course.id);
      setError(null);
      await apiService.deleteCourse(confirmDelete.course.id);
      setSuccess('Course deleted successfully');
      setConfirmDelete({ isOpen: false, course: null });
      loadCourses();
      setTimeout(() => setSuccess(null), 3000);
    } catch (err) {
      const apiError = err as ApiError;
      setError(apiError.title || 'Failed to delete course');
      setConfirmDelete({ isOpen: false, course: null });
    } finally {
      setDeletingId(undefined);
    }
  };

  const handleDeleteCancel = () => {
    setConfirmDelete({ isOpen: false, course: null });
  };

  const handlePageChange = (page: number) => {
    setCurrentPage(page);
  };

  if (loading && !courses) {
    return <Loading />;
  }

  return (
    <div>
      <div className="card">
        <div className="card-header">
          <h1 className="card-title">Courses</h1>
          <button className="btn btn-primary" onClick={handleCreate}>
            + Add Course
          </button>
        </div>

        {error && <Alert type="error" message={error} />}
        {success && <Alert type="success" message={success} />}

        {courses && courses.items.length > 0 ? (
          <>
            <CourseList
              courses={courses.items}
              onEdit={handleEdit}
              onDelete={handleDeleteClick}
              isDeleting={deletingId}
            />
            <Pagination
              currentPage={courses.pageNumber}
              totalPages={courses.totalPages}
              onPageChange={handlePageChange}
              hasNextPage={courses.hasNextPage}
              hasPreviousPage={courses.hasPreviousPage}
            />
          </>
        ) : (
          <EmptyState
            icon="📚"
            title="No courses yet"
            description="Get started by creating your first course"
            action={
              <button className="btn btn-primary" onClick={handleCreate}>
                + Add Course
              </button>
            }
          />
        )}
      </div>

      <Modal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title={selectedCourse ? 'Edit Course' : 'Add New Course'}
      >
        <CourseForm
          course={selectedCourse}
          onSubmit={handleSubmit}
          onCancel={() => setIsModalOpen(false)}
        />
      </Modal>

      <ConfirmDialog
        isOpen={confirmDelete.isOpen}
        title="Delete Course"
        message={
          <span>
            Are you sure you want to delete <strong>{confirmDelete.course?.code} - {confirmDelete.course?.title}</strong>?
            <br />
            <br />
            This action will also remove all enrollments for this course and cannot be undone.
          </span>
        }
        confirmText="Delete Course"
        cancelText="Cancel"
        onConfirm={handleDeleteConfirm}
        onCancel={handleDeleteCancel}
        type="danger"
        isLoading={!!deletingId}
      />
    </div>
  );
};

export default Courses;

