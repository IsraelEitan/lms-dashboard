import { Component, ErrorInfo, ReactNode } from 'react';

interface Props {
  children: ReactNode;
}

interface State {
  hasError: boolean;
  error: Error | null;
}

class ErrorBoundary extends Component<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = {
      hasError: false,
      error: null,
    };
  }

  static getDerivedStateFromError(error: Error): State {
    return {
      hasError: true,
      error,
    };
  }

  componentDidCatch(error: Error, errorInfo: ErrorInfo): void {
    console.error('ErrorBoundary caught an error:', error, errorInfo);
  }

  render() {
    if (this.state.hasError) {
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
            <div style={{ fontSize: '4rem', marginBottom: '1rem' }}>⚠️</div>
            <h1 style={{ color: 'var(--danger-color)', marginBottom: '1rem' }}>
              Something Went Wrong
            </h1>
            <p style={{ color: 'var(--text-secondary)', marginBottom: '2rem' }}>
              An unexpected error occurred in the application. Please try refreshing the page.
            </p>
            {this.state.error && (
              <details style={{ marginBottom: '2rem', textAlign: 'left' }}>
                <summary style={{ cursor: 'pointer', marginBottom: '0.5rem' }}>
                  Error Details
                </summary>
                <pre
                  style={{
                    backgroundColor: 'var(--bg-secondary)',
                    padding: '1rem',
                    borderRadius: '0.5rem',
                    overflow: 'auto',
                    fontSize: '0.875rem',
                  }}
                >
                  {this.state.error.toString()}
                </pre>
              </details>
            )}
            <button
              className="btn btn-primary"
              onClick={() => window.location.reload()}
            >
              Refresh Page
            </button>
          </div>
        </div>
      );
    }

    return this.props.children;
  }
}

export default ErrorBoundary;

