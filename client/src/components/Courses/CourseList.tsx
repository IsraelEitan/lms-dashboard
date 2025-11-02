import type { Course } from '../../types';

interface CourseListProps {
  courses: Course[];
  onEdit: (course: Course) => void;
  onDelete: (course: Course) => void;
  isDeleting?: string;
}

const CourseList = ({ courses, onEdit, onDelete, isDeleting }: CourseListProps) => {
  return (
    <div className="table-container">
      <table className="table">
        <thead>
          <tr>
            <th>Code</th>
            <th>Title</th>
            <th>Description</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {courses.map((course) => (
            <tr key={course.id}>
              <td>
                <span className="badge badge-primary">{course.code}</span>
              </td>
              <td>
                <strong>{course.title}</strong>
              </td>
              <td>{course.description || '-'}</td>
              <td>
                <div className="table-actions">
                  <button
                    className="btn btn-sm btn-primary"
                    onClick={() => onEdit(course)}
                    disabled={isDeleting === course.id}
                  >
                    Edit
                  </button>
                  <button
                    className="btn btn-sm btn-danger"
                    onClick={() => onDelete(course)}
                    disabled={isDeleting === course.id}
                  >
                    {isDeleting === course.id ? 'Deleting...' : 'Delete'}
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

export default CourseList;

