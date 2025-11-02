import { useEffect } from 'react';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import Layout from './components/Layout/Layout';
import Dashboard from './pages/Dashboard';
import Courses from './pages/Courses';
import Students from './pages/Students';
import Enrollments from './pages/Enrollments';
import Reports from './pages/Reports';
import ErrorBoundary from './components/Common/ErrorBoundary';
import ServerErrorPage from './components/Common/ServerErrorPage';
import { ApiConnectionProvider, useApiConnection } from './context/ApiConnectionContext';
import { apiService } from './services/api';
import './styles/index.css';

function AppContent() {
  const { isConnected, setConnectionStatus, checkConnection } = useApiConnection();

  useEffect(() => {
    // Connect API service to the connection context
    apiService.setConnectionStatusCallback(setConnectionStatus);
  }, [setConnectionStatus]);

  // If not connected, show the server error page
  if (!isConnected) {
    return <ServerErrorPage onRetry={checkConnection} />;
  }

  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Layout />}>
          <Route index element={<Dashboard />} />
          <Route path="courses" element={<Courses />} />
          <Route path="students" element={<Students />} />
          <Route path="enrollments" element={<Enrollments />} />
          <Route path="reports" element={<Reports />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}

function App() {
  return (
    <ErrorBoundary>
      <ApiConnectionProvider>
        <AppContent />
      </ApiConnectionProvider>
    </ErrorBoundary>
  );
}

export default App;

