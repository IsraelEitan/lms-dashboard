import { NavLink, Link } from 'react-router-dom';

const Sidebar = () => {
  return (
    <nav className="nav-sidebar">
      <div className="nav-header">
        <Link to="/" className="nav-logo" style={{ textDecoration: 'none', color: 'inherit', cursor: 'pointer' }}>
          🎓 LMS Dashboard
        </Link>
      </div>
      <ul className="nav-menu">
        <li className="nav-item">
          <NavLink to="/" className="nav-link">
            <span className="nav-icon">📊</span>
            Dashboard
          </NavLink>
        </li>
        <li className="nav-item">
          <NavLink to="/courses" className="nav-link">
            <span className="nav-icon">📚</span>
            Courses
          </NavLink>
        </li>
        <li className="nav-item">
          <NavLink to="/students" className="nav-link">
            <span className="nav-icon">👥</span>
            Students
          </NavLink>
        </li>
        <li className="nav-item">
          <NavLink to="/enrollments" className="nav-link">
            <span className="nav-icon">✅</span>
            Enrollments
          </NavLink>
        </li>
        <li className="nav-item">
          <NavLink to="/reports" className="nav-link">
            <span className="nav-icon">📈</span>
            Reports
          </NavLink>
        </li>
      </ul>
    </nav>
  );
};

export default Sidebar;

