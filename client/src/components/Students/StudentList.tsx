import type { Student } from '../../types';

interface StudentListProps {
  students: Student[];
  onEdit: (student: Student) => void;
  onDelete: (student: Student) => void;
  isDeleting?: string;
}

const StudentList = ({ students, onEdit, onDelete, isDeleting }: StudentListProps) => {
  return (
    <div className="table-container">
      <table className="table">
        <thead>
          <tr>
            <th>Name</th>
            <th>Email</th>
            <th>Student ID</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {students.map((student) => (
            <tr key={student.id}>
              <td>
                <strong>{student.name}</strong>
              </td>
              <td>{student.email}</td>
              <td>
                <span className="badge badge-primary">{student.id}</span>
              </td>
              <td>
                <div className="table-actions">
                  <button
                    className="btn btn-sm btn-primary"
                    onClick={() => onEdit(student)}
                    disabled={isDeleting === student.id}
                  >
                    Edit
                  </button>
                  <button
                    className="btn btn-sm btn-danger"
                    onClick={() => onDelete(student)}
                    disabled={isDeleting === student.id}
                  >
                    {isDeleting === student.id ? 'Deleting...' : 'Delete'}
                  </button>
                </div>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default StudentList;

