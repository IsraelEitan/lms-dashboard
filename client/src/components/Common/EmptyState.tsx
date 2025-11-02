interface EmptyStateProps {
  icon?: string;
  title: string;
  description?: string;
  action?: React.ReactNode;
}

const EmptyState = ({ icon = '📦', title, description, action }: EmptyStateProps) => {
  return (
    <div className="empty-state">
      <div className="empty-state-icon">{icon}</div>
      <h3 className="empty-state-title">{title}</h3>
      {description && <p className="empty-state-description">{description}</p>}
      {action && <div>{action}</div>}
    </div>
  );
};

export default EmptyState;

