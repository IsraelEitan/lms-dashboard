import { ReactNode } from 'react';

interface ConfirmDialogProps {
  isOpen: boolean;
  title: string;
  message: string | ReactNode;
  confirmText?: string;
  cancelText?: string;
  onConfirm: () => void;
  onCancel: () => void;
  type?: 'danger' | 'warning' | 'info';
  isLoading?: boolean;
}

const ConfirmDialog = ({
  isOpen,
  title,
  message,
  confirmText = 'Confirm',
  cancelText = 'Cancel',
  onConfirm,
  onCancel,
  type = 'danger',
  isLoading = false,
}: ConfirmDialogProps) => {
  if (!isOpen) return null;

  const getIcon = () => {
    switch (type) {
      case 'danger':
        return '⚠️';
      case 'warning':
        return '⚡';
      case 'info':
        return 'ℹ️';
      default:
        return '❓';
    }
  };

  const getButtonClass = () => {
    switch (type) {
      case 'danger':
        return 'btn-danger';
      case 'warning':
        return 'btn-warning';
      case 'info':
        return 'btn-primary';
      default:
        return 'btn-danger';
    }
  };

  return (
    <div className="modal-overlay" onClick={onCancel}>
      <div
        className="modal"
        onClick={(e) => e.stopPropagation()}
        style={{
          maxWidth: '500px',
          animation: 'modalSlideIn 0.3s ease-out',
        }}
      >
        <div className="modal-header" style={{ borderBottom: 'none', justifyContent: 'center', flexDirection: 'column', gap: '1rem', paddingTop: '2rem' }}>
          <div
            style={{
              fontSize: '3rem',
              animation: 'iconPulse 0.5s ease-in-out',
            }}
          >
            {getIcon()}
          </div>
          <h2 className="modal-title" style={{ textAlign: 'center' }}>
            {title}
          </h2>
        </div>
        
        <div className="modal-body" style={{ textAlign: 'center', paddingTop: '0' }}>
          <div
            style={{
              fontSize: '1rem',
              color: 'var(--text-secondary)',
              lineHeight: '1.6',
            }}
          >
            {message}
          </div>
        </div>

        <div className="modal-footer">
          <button
            type="button"
            onClick={onCancel}
            className="btn btn-outline"
            disabled={isLoading}
          >
            {cancelText}
          </button>
          <button
            type="button"
            onClick={onConfirm}
            className={`btn ${getButtonClass()}`}
            disabled={isLoading}
          >
            {isLoading ? 'Processing...' : confirmText}
          </button>
        </div>
      </div>
    </div>
  );
};

export default ConfirmDialog;

