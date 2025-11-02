import { useState, useEffect } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { apiService } from '../services/api';
import { logger } from '../utils/logger';
import type { DashboardStats } from '../types';
import Loading from '../components/Common/Loading';

const Dashboard = () => {
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [loading, setLoading] = useState(true);
  const location = useLocation();

  useEffect(() => {
    logger.componentMount('Dashboard');
    // Reload stats whenever we navigate to the dashboard
    loadStats();
    
    return () => {
      logger.componentUnmount('Dashboard');
    };
  }, [location.pathname]);

  const loadStats = async () => {
    try {
      setLoading(true);
      logger.info('Loading dashboard statistics...');
      
      // Use the new Reports API endpoint for better performance
      const data = await apiService.getDashboardStats();
      
      logger.success(`Dashboard loaded: ${data.totalStudents} students, ${data.totalCourses} courses, ${data.totalEnrollments} enrollments`);
      setStats(data);
    } catch (error) {
      logger.error('Failed to load dashboard stats', error);
      // Network errors are handled by ApiConnectionContext and will show ServerErrorPage
      // Other errors will be logged for debugging
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return <Loading />;
  }

  return (
    <div>
      <div className="card">
        <h1 className="card-title" style={{ marginBottom: '2rem' }}>
          Welcome to LMS Dashboard
        </h1>
        <p style={{ color: 'var(--text-secondary)', marginBottom: '2rem' }}>
          Manage your learning management system with ease. View and manage courses,
          students, and enrollments all in one place.
        </p>

        <div
          style={{
            display: 'grid',
            gridTemplateColumns: 'repeat(auto-fit, minmax(250px, 1fr))',
            gap: '1.5rem',
            marginBottom: '2rem',
          }}
        >
          <Link
            to="/courses"
            style={{
              textDecoration: 'none',
              padding: '2rem',
              background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
              borderRadius: '0.75rem',
              color: 'white',
            }}
          >
            <div style={{ fontSize: '2.5rem', marginBottom: '0.5rem' }}>📚</div>
            <div style={{ fontSize: '2rem', fontWeight: 'bold', marginBottom: '0.5rem' }}>
              {stats?.totalCourses || 0}
            </div>
            <div style={{ fontSize: '1rem', opacity: 0.9 }}>Total Courses</div>
          </Link>

          <Link
            to="/students"
            style={{
              textDecoration: 'none',
              padding: '2rem',
              background: 'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)',
              borderRadius: '0.75rem',
              color: 'white',
            }}
          >
            <div style={{ fontSize: '2.5rem', marginBottom: '0.5rem' }}>👥</div>
            <div style={{ fontSize: '2rem', fontWeight: 'bold', marginBottom: '0.5rem' }}>
              {stats?.totalStudents || 0}
            </div>
            <div style={{ fontSize: '1rem', opacity: 0.9 }}>Total Students</div>
          </Link>

          <Link
            to="/enrollments"
            style={{
              textDecoration: 'none',
              padding: '2rem',
              background: 'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)',
              borderRadius: '0.75rem',
              color: 'white',
            }}
          >
            <div style={{ fontSize: '2.5rem', marginBottom: '0.5rem' }}>✅</div>
            <div style={{ fontSize: '2rem', fontWeight: 'bold', marginBottom: '0.5rem' }}>
              {stats?.totalEnrollments || 0}
            </div>
            <div style={{ fontSize: '1rem', opacity: 0.9 }}>Total Enrollments</div>
          </Link>
        </div>
      </div>

      <div className="card">
        <h2 className="card-title">Quick Actions</h2>
        <div style={{ display: 'flex', gap: '1rem', flexWrap: 'wrap' }}>
          <Link to="/courses" className="btn btn-primary">
            📚 Manage Courses
          </Link>
          <Link to="/students" className="btn btn-primary">
            👥 Manage Students
          </Link>
          <Link to="/enrollments" className="btn btn-success">
            ✅ Enroll Student
          </Link>
        </div>
      </div>
    </div>
  );
};

export default Dashboard;

