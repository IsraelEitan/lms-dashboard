import { apiService } from '../services/api';
import type { Enrollment } from '../types';
import Modal from '../components/Common/Modal';
import ConfirmDialog from '../components/Common/ConfirmDialog';
import EnrollmentForm from '../components/Enrollments/EnrollmentForm';
import EnrollmentList from '../components/Enrollments/EnrollmentList';
import Loading from '../components/Common/Loading';
import EmptyState from '../components/Common/EmptyState';
import Pagination from '../components/Common/Pagination';
import Alert from '../components/Common/Alert';
import { useCrudPage } from '../hooks/useCrudPage';

const Enrollments = () => {
  const {
    items: enrollments,
    loading,
    error,
    success,
    isModalOpen,
    deletingId,
    confirmDelete,
    handleCreate,
    handleSubmit,
    handleDeleteClick,
    handleDeleteConfirm,
    handleDeleteCancel,
    handlePageChange,
    handleCloseModal,
  } = useCrudPage<Enrollment>({
    loadItems: (page, pageSize) => apiService.getEnrollments(page, pageSize),
    createItem: (data) => apiService.createEnrollment(data),
    deleteItem: (id) => apiService.deleteEnrollment(id),
  });

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
        onClose={handleCloseModal}
        title="Enroll Student in Course"
      >
        <EnrollmentForm
          onSubmit={(data) => handleSubmit(data, 'Student enrolled successfully')}
          onCancel={handleCloseModal}
        />
      </Modal>

      <ConfirmDialog
        isOpen={confirmDelete.isOpen}
        title="Remove Enrollment"
        message={
          <span>
            Are you sure you want to remove the enrollment for{' '}
            <strong>{confirmDelete.item?.studentName || 'this student'}</strong> from{' '}
            <strong>{confirmDelete.item?.courseTitle || 'this course'}</strong>?
            <br />
            <br />
            This action cannot be undone.
          </span>
        }
        confirmText="Remove Enrollment"
        cancelText="Cancel"
        onConfirm={() => handleDeleteConfirm('Enrollment removed successfully')}
        onCancel={handleDeleteCancel}
        type="danger"
        isLoading={!!deletingId}
      />
    </div>
  );
};

export default Enrollments;

