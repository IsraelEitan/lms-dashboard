
using Lms.Api.Common.Results;
using Lms.Api.Contracts.DTOs;
using Lms.Api.Presentation.Contracts;

namespace Lms.Api.Application.Interfaces;

public interface ICourseService
{
  // 🔹 Queries
  Task<Result<PagedResult<CourseDto>>> QueryAsync(PagingQuery query, CancellationToken ct);
  Task<Result<IReadOnlyList<CourseDto>>> GetAllAsync(CancellationToken ct);
  Task<Result<CourseDto>> GetByIdAsync(Guid id, CancellationToken ct);

  // 🔹 Commands
  Task<Result<CourseDto>> CreateAsync(CreateUpdateCourseDto dto, CancellationToken ct);
  Task<Result> UpdateAsync(Guid id, CreateUpdateCourseDto dto, CancellationToken ct);
  Task<Result> DeleteAsync(Guid id, CancellationToken ct);
}
