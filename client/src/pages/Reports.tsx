import { useState, useEffect } from 'react';
import { apiService } from '../services/api';
import type { CourseEnrollmentReport, ExportResponse, ApiError } from '../types';
import Loading from '../components/Common/Loading';
import Alert from '../components/Common/Alert';

const Reports = () => {
  const [report, setReport] = useState<CourseEnrollmentReport[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [exporting, setExporting] = useState(false);
  const [exportSuccess, setExportSuccess] = useState<ExportResponse | null>(null);

  useEffect(() => {
    loadReport();
  }, []);

  const loadReport = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await apiService.getStudentsPerCourse();
      setReport(data);
    } catch (err) {
      const apiError = err as ApiError;
      setError(apiError.title || 'Failed to load report');
    } finally {
      setLoading(false);
    }
  };

  const handleExport = async () => {
    try {
      setExporting(true);
      setError(null);
      setExportSuccess(null);
      const response = await apiService.exportStudentsPerCourse();
      setExportSuccess(response);
      setTimeout(() => setExportSuccess(null), 5000);
    } catch (err) {
      const apiError = err as ApiError;
      setError(apiError.title || 'Failed to export report');
    } finally {
      setExporting(false);
    }
  };

  if (loading) return <Loading />;

  const totalEnrollments = report.reduce((sum, course) => sum + course.enrolledStudents, 0);
  const avgEnrollmentsPerCourse = report.length > 0 
    ? (totalEnrollments / report.length).toFixed(1) 
    : '0';

  return (
    <div>
      <div className="page-header">
        <div>
          <h1>Reports</h1>
          <p className="text-secondary">Analyze course enrollment statistics</p>
        </div>
        <button 
          className="btn btn-primary"
          onClick={handleExport}
          disabled={exporting || report.length === 0}
        >
          {exporting ? (
            <>
              <span className="spinner-sm"></span>
              Exporting...
            </>
          ) : (
            <>
              <span style={{ marginRight: '0.5rem' }}>📤</span>
              Export to S3
            </>
          )}
        </button>
      </div>

      {error && <Alert type="error" message={error} onClose={() => setError(null)} />}
      {exportSuccess && (
        <Alert 
          type="success" 
          message={
            <div>
              <strong>Report exported successfully!</strong>
              <br />
              <small>File: {exportSuccess.fileName}</small>
              <br />
              <small>Records: {exportSuccess.recordCount}</small>
              <br />
              <small style={{ wordBreak: 'break-all' }}>URL: {exportSuccess.s3Url}</small>
            </div>
          }
          onClose={() => setExportSuccess(null)} 
        />
      )}

      {/* Summary Cards */}
      <div style={{ 
        display: 'grid', 
        gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', 
        gap: '1rem', 
        marginBottom: '2rem' 
      }}>
        <div className="card" style={{ textAlign: 'center', padding: '1.5rem' }}>
          <div style={{ fontSize: '2rem', fontWeight: 'bold', color: 'var(--primary-color)' }}>
            {report.length}
          </div>
          <div className="text-secondary">Total Courses</div>
        </div>
        <div className="card" style={{ textAlign: 'center', padding: '1.5rem' }}>
          <div style={{ fontSize: '2rem', fontWeight: 'bold', color: 'var(--success-color)' }}>
            {totalEnrollments}
          </div>
          <div className="text-secondary">Total Enrollments</div>
        </div>
        <div className="card" style={{ textAlign: 'center', padding: '1.5rem' }}>
          <div style={{ fontSize: '2rem', fontWeight: 'bold', color: 'var(--info-color)' }}>
            {avgEnrollmentsPerCourse}
          </div>
          <div className="text-secondary">Avg per Course</div>
        </div>
      </div>

      {/* Students Per Course Report */}
      <div className="card">
        <div className="card-header">
          <h2 className="card-title">Students Per Course</h2>
        </div>
        <div className="card-body">
          {report.length === 0 ? (
            <div style={{ textAlign: 'center', padding: '3rem', color: 'var(--text-secondary)' }}>
              <div style={{ fontSize: '3rem', marginBottom: '1rem' }}>📊</div>
              <p>No courses found</p>
              <p style={{ fontSize: '0.9rem' }}>Add some courses to see the report</p>
            </div>
          ) : (
            <div className="table-responsive">
              <table className="table">
                <thead>
                  <tr>
                    <th style={{ width: '15%' }}>Course Code</th>
                    <th style={{ width: '45%' }}>Course Title</th>
                    <th style={{ width: '20%', textAlign: 'center' }}>Enrolled Students</th>
                    <th style={{ width: '20%' }}>Status</th>
                  </tr>
                </thead>
                <tbody>
                  {report.map((course) => {
                    const enrollmentLevel = 
                      course.enrolledStudents === 0 ? 'empty' :
                      course.enrolledStudents <= 3 ? 'low' :
                      course.enrolledStudents <= 6 ? 'medium' : 'high';

                    const statusColor = {
                      empty: '#6b7280',
                      low: '#ef4444',
                      medium: '#f59e0b',
                      high: '#10b981'
                    }[enrollmentLevel];

                    const statusText = {
                      empty: 'No Enrollments',
                      low: 'Low Enrollment',
                      medium: 'Medium Enrollment',
                      high: 'High Enrollment'
                    }[enrollmentLevel];

                    return (
                      <tr key={course.courseId}>
                        <td>
                          <span className="badge" style={{ 
                            backgroundColor: 'var(--primary-light)', 
                            color: 'var(--primary-color)',
                            fontFamily: 'monospace'
                          }}>
                            {course.courseCode}
                          </span>
                        </td>
                        <td>
                          <strong>{course.courseTitle}</strong>
                        </td>
                        <td style={{ textAlign: 'center' }}>
                          <span style={{
                            display: 'inline-flex',
                            alignItems: 'center',
                            justifyContent: 'center',
                            minWidth: '40px',
                            height: '40px',
                            borderRadius: '50%',
                            backgroundColor: statusColor + '20',
                            color: statusColor,
                            fontWeight: 'bold',
                            fontSize: '1.1rem'
                          }}>
                            {course.enrolledStudents}
                          </span>
                        </td>
                        <td>
                          <span className="badge" style={{
                            backgroundColor: statusColor + '20',
                            color: statusColor,
                            padding: '0.4rem 0.8rem'
                          }}>
                            {statusText}
                          </span>
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default Reports;

