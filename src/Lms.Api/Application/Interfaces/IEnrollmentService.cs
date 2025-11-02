using Lms.Api.Common.Results;
using Lms.Api.Contracts.DTOs;
using Lms.Api.Presentation.Contracts;

namespace Lms.Api.Application.Interfaces;

/// <summary>
/// Application service for managing student enrollments in courses.
/// </summary>
public interface IEnrollmentService
{
  // Queries
  Task<Result<PagedResult<EnrollmentDto>>> QueryAsync(PagingQuery query, CancellationToken ct);
  Task<Result<IReadOnlyList<EnrollmentDto>>> GetAllAsync(CancellationToken ct);
  Task<Result<IReadOnlyList<EnrollmentDto>>> GetByCourseAsync(Guid courseId, CancellationToken ct);
  Task<Result<IReadOnlyList<EnrollmentDto>>> GetByStudentAsync(Guid studentId, CancellationToken ct);

  // Commands
  Task<Result<EnrollmentDto>> AssignAsync(Guid studentId, Guid courseId, CancellationToken ct);
  Task<Result> DeleteAsync(Guid enrollmentId, CancellationToken ct);
  Task<Result> DeleteByStudentAsync(Guid studentId, CancellationToken ct);
  Task<Result> DeleteByCourseAsync(Guid courseId, CancellationToken ct);
}
