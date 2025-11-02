import { apiService } from '../services/api';
import type { Student } from '../types';
import Modal from '../components/Common/Modal';
import ConfirmDialog from '../components/Common/ConfirmDialog';
import StudentForm from '../components/Students/StudentForm';
import StudentList from '../components/Students/StudentList';
import Loading from '../components/Common/Loading';
import EmptyState from '../components/Common/EmptyState';
import Pagination from '../components/Common/Pagination';
import Alert from '../components/Common/Alert';
import { useCrudPage } from '../hooks/useCrudPage';

const Students = () => {
  const {
    items: students,
    loading,
    error,
    success,
    isModalOpen,
    selectedItem: selectedStudent,
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
  } = useCrudPage<Student>({
    loadItems: (page, pageSize) => apiService.getStudents(page, pageSize),
    createItem: (data) => apiService.createStudent(data),
    updateItem: (id, data) => apiService.updateStudent(id, data),
    deleteItem: (id) => apiService.deleteStudent(id),
  });

  if (loading && !students) {
    return <Loading />;
  }

  return (
    <div>
      <div className="card">
        <div className="card-header">
          <h1 className="card-title">Students</h1>
          <button className="btn btn-primary" onClick={handleCreate}>
            + Add Student
          </button>
        </div>

        {error && <Alert type="error" message={error} />}
        {success && <Alert type="success" message={success} />}

        {students && students.items.length > 0 ? (
          <>
            <StudentList
              students={students.items}
              onEdit={handleEdit}
              onDelete={handleDeleteClick}
              isDeleting={deletingId}
            />
            <Pagination
              currentPage={students.pageNumber}
              totalPages={students.totalPages}
              onPageChange={handlePageChange}
              hasNextPage={students.hasNextPage}
              hasPreviousPage={students.hasPreviousPage}
            />
          </>
        ) : (
          <EmptyState
            icon="👥"
            title="No students yet"
            description="Get started by adding your first student"
            action={
              <button className="btn btn-primary" onClick={handleCreate}>
                + Add Student
              </button>
            }
          />
        )}
      </div>

      <Modal
        isOpen={isModalOpen}
        onClose={handleCloseModal}
        title={selectedStudent ? 'Edit Student' : 'Add New Student'}
      >
        <StudentForm
          student={selectedStudent}
          onSubmit={(data) => handleSubmit(data, selectedStudent ? 'Student updated successfully' : 'Student created successfully')}
          onCancel={handleCloseModal}
        />
      </Modal>

      <ConfirmDialog
        isOpen={confirmDelete.isOpen}
        title="Delete Student"
        message={
          <span>
            Are you sure you want to delete <strong>{confirmDelete.item?.name}</strong>?
            <br />
            <br />
            This action will also remove all enrollments for this student and cannot be undone.
          </span>
        }
        confirmText="Delete Student"
        cancelText="Cancel"
        onConfirm={() => handleDeleteConfirm('Student deleted successfully')}
        onCancel={handleDeleteCancel}
        type="danger"
        isLoading={!!deletingId}
      />
    </div>
  );
};

export default Students;

