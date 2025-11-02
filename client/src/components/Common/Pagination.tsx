interface PaginationProps {
  currentPage: number;
  totalPages: number;
  onPageChange: (page: number) => void;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

const Pagination = ({
  currentPage,
  totalPages,
  onPageChange,
  hasNextPage,
  hasPreviousPage,
}: PaginationProps) => {
  return (
    <div className="pagination">
      <button
        className="btn btn-outline btn-sm"
        onClick={() => onPageChange(currentPage - 1)}
        disabled={!hasPreviousPage}
      >
        ← Previous
      </button>
      <span className="pagination-info">
        Page {currentPage} of {totalPages}
      </span>
      <button
        className="btn btn-outline btn-sm"
        onClick={() => onPageChange(currentPage + 1)}
        disabled={!hasNextPage}
      >
        Next →
      </button>
    </div>
  );
};

export default Pagination;

