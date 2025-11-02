import { useState, useEffect } from 'react';
import { apiService } from '../services/api';
import type { Student, PagedResponse, ApiError } from '../types';
import Modal from '../components/Common/Modal';
import ConfirmDialog from '../components/Common/ConfirmDialog';
import StudentForm from '../components/Students/StudentForm';
import StudentList from '../components/Students/StudentList';
import Loading from '../components/Common/Loading';
import EmptyState from '../components/Common/EmptyState';
import Pagination from '../components/Common/Pagination';
import Alert from '../components/Common/Alert';

const Students = () => {
  const [students, setStudents] = useState<PagedResponse<Student> | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [currentPage, setCurrentPage] = useState(1);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedStudent, setSelectedStudent] = useState<Student | undefined>(undefined);
  const [deletingId, setDeletingId] = useState<string | undefined>(undefined);
  const [confirmDelete, setConfirmDelete] = useState<{ isOpen: boolean; student: Student | null }>({
    isOpen: false,
    student: null,
  });

  const loadStudents = async (page: number = currentPage) => {
    try {
      setLoading(true);
      setError(null);
      const data = await apiService.getStudents(page, 10);
      setStudents(data);
    } catch (err) {
      const apiError = err as ApiError;
      setError(apiError.title || 'Failed to load students');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadStudents();
  }, [currentPage]);

  const handleCreate = () => {
    setSelectedStudent(undefined);
    setIsModalOpen(true);
  };

  const handleEdit = (student: Student) => {
    setSelectedStudent(student);
    setIsModalOpen(true);
  };

  const handleSubmit = async (data: any) => {
    try {
      setError(null);
      if (selectedStudent) {
        await apiService.updateStudent(selectedStudent.id, data);
        setSuccess('Student updated successfully');
        // Stay on current page for updates
        loadStudents();
      } else {
        await apiService.createStudent(data);
        setSuccess('Student created successfully');
        // Reset to page 1 for new students
        setCurrentPage(1);
        await loadStudents(1);
      }
      setIsModalOpen(false);
      setTimeout(() => setSuccess(null), 3000);
    } catch (err) {
      const apiError = err as ApiError;
      setError(apiError.title || 'Failed to save student');
      throw err;
    }
  };

  const handleDeleteClick = (student: Student) => {
    setConfirmDelete({ isOpen: true, student });
  };

  const handleDeleteConfirm = async () => {
    if (!confirmDelete.student) return;

    try {
      setDeletingId(confirmDelete.student.id);
      setError(null);
      await apiService.deleteStudent(confirmDelete.student.id);
      setSuccess('Student deleted successfully');
      setConfirmDelete({ isOpen: false, student: null });
      loadStudents();
      setTimeout(() => setSuccess(null), 3000);
    } catch (err) {
      const apiError = err as ApiError;
      setError(apiError.title || 'Failed to delete student');
      setConfirmDelete({ isOpen: false, student: null });
    } finally {
      setDeletingId(undefined);
    }
  };

  const handleDeleteCancel = () => {
    setConfirmDelete({ isOpen: false, student: null });
  };

  const handlePageChange = (page: number) => {
    setCurrentPage(page);
  };

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
        onClose={() => setIsModalOpen(false)}
        title={selectedStudent ? 'Edit Student' : 'Add New Student'}
      >
        <StudentForm
          student={selectedStudent}
          onSubmit={handleSubmit}
          onCancel={() => setIsModalOpen(false)}
        />
      </Modal>

      <ConfirmDialog
        isOpen={confirmDelete.isOpen}
        title="Delete Student"
        message={
          <span>
            Are you sure you want to delete <strong>{confirmDelete.student?.name}</strong>?
            <br />
            <br />
            This action will also remove all enrollments for this student and cannot be undone.
          </span>
        }
        confirmText="Delete Student"
        cancelText="Cancel"
        onConfirm={handleDeleteConfirm}
        onCancel={handleDeleteCancel}
        type="danger"
        isLoading={!!deletingId}
      />
    </div>
  );
};

export default Students;

