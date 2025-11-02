import { useState, useEffect } from 'react';
import { apiService } from '../services/api';
import type { Enrollment, PagedResponse, ApiError } from '../types';
import Modal from '../components/Common/Modal';
import ConfirmDialog from '../components/Common/ConfirmDialog';
import EnrollmentForm from '../components/Enrollments/EnrollmentForm';
import EnrollmentList from '../components/Enrollments/EnrollmentList';
import Loading from '../components/Common/Loading';
import EmptyState from '../components/Common/EmptyState';
import Pagination from '../components/Common/Pagination';
import Alert from '../components/Common/Alert';

const Enrollments = () => {
  const [enrollments, setEnrollments] = useState<PagedResponse<Enrollment> | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [currentPage, setCurrentPage] = useState(1);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [deletingId, setDeletingId] = useState<string | undefined>(undefined);
  const [confirmDelete, setConfirmDelete] = useState<{ isOpen: boolean; enrollment: Enrollment | null }>({
    isOpen: false,
    enrollment: null,
  });

  const loadEnrollments = async (page: number = currentPage) => {
    try {
      setLoading(true);
      setError(null);
      const data = await apiService.getEnrollments(page, 10);
      setEnrollments(data);
    } catch (err) {
      const apiError = err as ApiError;
      setError(apiError.title || 'Failed to load enrollments');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadEnrollments();
  }, [currentPage]);

  const handleCreate = () => {
    setIsModalOpen(true);
  };

  const handleSubmit = async (data: any) => {
    try {
      setError(null);
      await apiService.createEnrollment(data);
      setSuccess('Student enrolled successfully');
      setIsModalOpen(false);
      // Reset to page 1 for new enrollments
      setCurrentPage(1);
      await loadEnrollments(1);
      setTimeout(() => setSuccess(null), 3000);
    } catch (err) {
      const apiError = err as ApiError;
      setError(apiError.title || 'Failed to create enrollment');
      throw err;
    }
  };

  const handleDeleteClick = (enrollment: Enrollment) => {
    setConfirmDelete({ isOpen: true, enrollment });
  };

  const handleDeleteConfirm = async () => {
    if (!confirmDelete.enrollment) return;

    try {
      setDeletingId(confirmDelete.enrollment.id);
      setError(null);
      await apiService.deleteEnrollment(confirmDelete.enrollment.id);
      setSuccess('Enrollment removed successfully');
      setConfirmDelete({ isOpen: false, enrollment: null });
      loadEnrollments();
      setTimeout(() => setSuccess(null), 3000);
    } catch (err) {
      const apiError = err as ApiError;
      setError(apiError.title || 'Failed to delete enrollment');
      setConfirmDelete({ isOpen: false, enrollment: null });
    } finally {
      setDeletingId(undefined);
    }
  };

  const handleDeleteCancel = () => {
    setConfirmDelete({ isOpen: false, enrollment: null });
  };

  const handlePageChange = (page: number) => {
    setCurrentPage(page);
  };

  if (loading && !enrollments) {
    return <Loading />;
  }

  return (
    <div>
      <div className="card">
        <div className="card-header">
          <h1 className="card-title">Enrollments</h1>
          <button className="btn btn-success" onClick={handleCreate}>
            + Enroll Student
          </button>
        </div>

        {error && <Alert type="error" message={error} />}
        {success && <Alert type="success" message={success} />}

        {enrollments && enrollments.items.length > 0 ? (
          <>
            <EnrollmentList
              enrollments={enrollments.items}
              onDelete={handleDeleteClick}
              isDeleting={deletingId}
            />
            <Pagination
              currentPage={enrollments.pageNumber}
              totalPages={enrollments.totalPages}
              onPageChange={handlePageChange}
              hasNextPage={enrollments.hasNextPage}
              hasPreviousPage={enrollments.hasPreviousPage}
            />
          </>
        ) : (
          <EmptyState
            icon="✅"
            title="No enrollments yet"
            description="Get started by enrolling students in courses"
            action={
              <button className="btn btn-success" onClick={handleCreate}>
                + Enroll Student
              </button>
            }
          />
        )}
      </div>

      <Modal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title="Enroll Student in Course"
      >
        <EnrollmentForm
          onSubmit={handleSubmit}
          onCancel={() => setIsModalOpen(false)}
        />
      </Modal>

      <ConfirmDialog
        isOpen={confirmDelete.isOpen}
        title="Remove Enrollment"
        message={
          <span>
            Are you sure you want to remove the enrollment for{' '}
            <strong>{confirmDelete.enrollment?.studentName || 'this student'}</strong> from{' '}
            <strong>{confirmDelete.enrollment?.courseTitle || 'this course'}</strong>?
            <br />
            <br />
            This action cannot be undone.
          </span>
        }
        confirmText="Remove Enrollment"
        cancelText="Cancel"
        onConfirm={handleDeleteConfirm}
        onCancel={handleDeleteCancel}
        type="danger"
        isLoading={!!deletingId}
      />
    </div>
  );
};

export default Enrollments;

