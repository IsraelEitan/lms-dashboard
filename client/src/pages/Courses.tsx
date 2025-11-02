import { apiService } from '../services/api';
import type { Course } from '../types';
import Modal from '../components/Common/Modal';
import ConfirmDialog from '../components/Common/ConfirmDialog';
import CourseForm from '../components/Courses/CourseForm';
import CourseList from '../components/Courses/CourseList';
import Loading from '../components/Common/Loading';
import EmptyState from '../components/Common/EmptyState';
import Pagination from '../components/Common/Pagination';
import Alert from '../components/Common/Alert';
import { useCrudPage } from '../hooks/useCrudPage';

const Courses = () => {
  const {
    items: courses,
    loading,
    error,
    success,
    isModalOpen,
    selectedItem: selectedCourse,
    deletingId,
    confirmDelete,
    handleCreate,
    handleEdit,
    handleSubmit,
    handleDeleteClick,
    handleDeleteConfirm,
    handleDeleteCancel,
    handlePageChange,
    handleCloseModal,
  } = useCrudPage<Course>({
    loadItems: (page, pageSize) => apiService.getCourses(page, pageSize),
    createItem: (data) => apiService.createCourse(data),
    updateItem: (id, data) => apiService.updateCourse(id, data),
    deleteItem: (id) => apiService.deleteCourse(id),
  });

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
        onClose={handleCloseModal}
        title={selectedCourse ? 'Edit Course' : 'Add New Course'}
      >
        <CourseForm
          course={selectedCourse}
          onSubmit={(data) => handleSubmit(data, selectedCourse ? 'Course updated successfully' : 'Course created successfully')}
          onCancel={handleCloseModal}
        />
      </Modal>

      <ConfirmDialog
        isOpen={confirmDelete.isOpen}
        title="Delete Course"
        message={
          <span>
            Are you sure you want to delete <strong>{confirmDelete.item?.code} - {confirmDelete.item?.title}</strong>?
            <br />
            <br />
            This action will also remove all enrollments for this course and cannot be undone.
          </span>
        }
        confirmText="Delete Course"
        cancelText="Cancel"
        onConfirm={() => handleDeleteConfirm('Course deleted successfully')}
        onCancel={handleDeleteCancel}
        type="danger"
        isLoading={!!deletingId}
      />
    </div>
  );
};

export default Courses;

