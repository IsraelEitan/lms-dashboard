
using Lms.Api.Common.Results;
using Lms.Api.Contracts.DTOs;
using Lms.Api.Presentation.Contracts;

namespace Lms.Api.Application.Interfaces;

public interface IStudentService
{
  // 🔹 Queries
  Task<Result<PagedResult<StudentDto>>> QueryAsync(PagingQuery query, CancellationToken ct);
  Task<Result<IReadOnlyList<StudentDto>>> GetAllAsync(CancellationToken ct);
  Task<Result<StudentDto>> GetByIdAsync(Guid id, CancellationToken ct);

  // 🔹 Commands
  Task<Result<StudentDto>> CreateAsync(CreateStudentDto dto, CancellationToken ct);
  Task<Result<StudentDto>> UpdateAsync(Guid id, UpdateStudentDto dto, CancellationToken ct);
  Task<Result> DeleteAsync(Guid id, CancellationToken ct);
}
