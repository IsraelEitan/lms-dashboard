import { ReactNode } from 'react';

interface AlertProps {
  type: 'success' | 'error' | 'warning' | 'info';
  message: string | ReactNode;
  onClose?: () => void;
}

const Alert = ({ type, message, onClose }: AlertProps) => {
  const icons = {
    success: '✓',
    error: '✗',
    warning: '⚠',
    info: 'ℹ',
  };

  return (
    <div className={`alert alert-${type}`}>
      <span>{icons[type]}</span>
      <span>{message}</span>
      {onClose && (
        <button
          onClick={onClose}
          style={{
            marginLeft: 'auto',
            background: 'none',
            border: 'none',
            color: 'inherit',
            cursor: 'pointer',
            fontSize: '1.2rem',
            padding: '0 0.5rem',
            opacity: 0.7,
          }}
          aria-label="Close"
        >
          ×
        </button>
      )}
    </div>
  );
};

export default Alert;

