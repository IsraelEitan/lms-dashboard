import { useEffect, useState } from 'react';

interface ServerErrorPageProps {
  onRetry?: () => void;
  autoRetry?: boolean;
  retryInterval?: number;
}

const ServerErrorPage = ({ 
  onRetry, 
  autoRetry = true, 
  retryInterval = 5000 
}: ServerErrorPageProps) => {
  const [countdown, setCountdown] = useState(retryInterval / 1000);
  const [isRetrying, setIsRetrying] = useState(false);

  useEffect(() => {
    if (!autoRetry) return;

    const countdownTimer = setInterval(() => {
      setCountdown((prev) => {
        if (prev <= 1) {
          handleRetry();
          return retryInterval / 1000;
        }
        return prev - 1;
      });
    }, 1000);

    return () => clearInterval(countdownTimer);
  }, [autoRetry, retryInterval]);

  const handleRetry = async () => {
    setIsRetrying(true);
    try {
      if (onRetry) {
        await onRetry();
      } else {
        window.location.reload();
      }
    } finally {
      setIsRetrying(false);
    }
  };

  return (
    <div
      style={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        minHeight: '100vh',
        padding: '2rem',
        backgroundColor: 'var(--bg-primary)',
      }}
    >
      <div className="card" style={{ maxWidth: '600px', textAlign: 'center' }}>
        <div style={{ fontSize: '4rem', marginBottom: '1rem' }}>🔌</div>
        <h1 style={{ color: 'var(--danger-color)', marginBottom: '1rem' }}>
          Server Connection Error
        </h1>
        <p style={{ color: 'var(--text-secondary)', marginBottom: '1rem' }}>
          Unable to connect to the backend server. This could be due to:
        </p>
        <ul
          style={{
            textAlign: 'left',
            color: 'var(--text-secondary)',
            marginBottom: '2rem',
            paddingLeft: '2rem',
          }}
        >
          <li>The server is not running</li>
          <li>Network connectivity issues</li>
          <li>The server is temporarily unavailable</li>
          <li>Incorrect API configuration</li>
        </ul>

        {autoRetry && (
          <p style={{ color: 'var(--text-secondary)', marginBottom: '1rem' }}>
            Automatically retrying in <strong>{countdown}</strong> seconds...
          </p>
        )}

        <div style={{ display: 'flex', gap: '1rem', justifyContent: 'center' }}>
          <button
            className="btn btn-primary"
            onClick={handleRetry}
            disabled={isRetrying}
          >
            {isRetrying ? 'Retrying...' : 'Retry Now'}
          </button>
          <button
            className="btn"
            onClick={() => window.location.reload()}
            disabled={isRetrying}
          >
            Refresh Page
          </button>
        </div>

        <details
          style={{
            marginTop: '2rem',
            textAlign: 'left',
            color: 'var(--text-secondary)',
          }}
        >
          <summary style={{ cursor: 'pointer', marginBottom: '0.5rem' }}>
            Technical Information
          </summary>
          <div
            style={{
              backgroundColor: 'var(--bg-secondary)',
              padding: '1rem',
              borderRadius: '0.5rem',
              fontSize: '0.875rem',
            }}
          >
            <p>
              <strong>API URL:</strong>{' '}
              {import.meta.env.VITE_API_URL || 'http://localhost:5225/api/v1'}
            </p>
            <p style={{ marginTop: '0.5rem' }}>
              <strong>Troubleshooting:</strong>
            </p>
            <ol style={{ paddingLeft: '1.5rem', marginTop: '0.5rem' }}>
              <li>Ensure the backend server is running (dotnet run)</li>
              <li>Check if the API is accessible at the URL above</li>
              <li>Verify CORS settings if running on different ports</li>
              <li>Check browser console for detailed error messages</li>
            </ol>
          </div>
        </details>
      </div>
    </div>
  );
};

export default ServerErrorPage;

