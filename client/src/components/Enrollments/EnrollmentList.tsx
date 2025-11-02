import type { Enrollment } from '../../types';

interface EnrollmentListProps {
  enrollments: Enrollment[];
  onDelete: (enrollment: Enrollment) => void;
  isDeleting?: string;
}

const EnrollmentList = ({ enrollments, onDelete, isDeleting }: EnrollmentListProps) => {
  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  };

  return (
    <div className="table-container">
      <table className="table">
        <thead>
          <tr>
            <th>Student</th>
            <th>Email</th>
            <th>Course Code</th>
            <th>Course Title</th>
            <th>Enrollment Date</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {enrollments.map((enrollment) => (
            <tr key={enrollment.id}>
              <td>
                <strong>{enrollment.studentName || 'N/A'}</strong>
              </td>
              <td>{enrollment.studentEmail || 'N/A'}</td>
              <td>
                <span className="badge badge-primary">{enrollment.courseCode || 'N/A'}</span>
              </td>
              <td>{enrollment.courseTitle || 'N/A'}</td>
              <td>
                <span className="badge badge-success">
                  {formatDate(enrollment.enrollmentDate)}
                </span>
              </td>
              <td>
                <button
                  className="btn btn-sm btn-danger"
                  onClick={() => onDelete(enrollment)}
                  disabled={isDeleting === enrollment.id}
                >
                  {isDeleting === enrollment.id ? 'Removing...' : 'Remove'}
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default EnrollmentList;

