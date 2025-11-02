using Lms.Api.Common.Results;

namespace Lms.Api.Application.Interfaces;

/// <summary>
/// Interface for S3-like storage operations.
/// Follows Dependency Inversion Principle - depends on abstraction, not implementation.
/// </summary>
public interface IS3Service
{
  /// <summary>
  /// Uploads a file to the specified bucket.
  /// </summary>
  /// <param name="bucketName">The name of the bucket.</param>
  /// <param name="key">The key (path) of the file.</param>
  /// <param name="content">The content of the file as a string.</param>
  /// <param name="contentType">The content type of the file (e.g., "application/json").</param>
  /// <param name="ct">Cancellation token.</param>
  /// <returns>A result containing the S3 URL or an error.</returns>
  Task<Result<string>> UploadFileAsync(string bucketName, string key, string content, string contentType, CancellationToken ct = default);

  /// <summary>
  /// Downloads a file from the specified bucket.
  /// </summary>
  /// <param name="bucketName">The name of the bucket.</param>
  /// <param name="key">The key (path) of the file.</param>
  /// <param name="ct">Cancellation token.</param>
  /// <returns>A result containing the file content as a string or an error if not found.</returns>
  Task<Result<string>> DownloadFileAsync(string bucketName, string key, CancellationToken ct = default);

  /// <summary>
  /// Deletes a file from the specified bucket.
  /// </summary>
  /// <param name="bucketName">The name of the bucket.</param>
  /// <param name="key">The key (path) of the file.</param>
  /// <param name="ct">Cancellation token.</param>
  /// <returns>A result indicating success or failure.</returns>
  Task<Result> DeleteFileAsync(string bucketName, string key, CancellationToken ct = default);

  /// <summary>
  /// Lists files in a specified bucket with an optional prefix.
  /// </summary>
  /// <param name="bucketName">The name of the bucket.</param>
  /// <param name="prefix">Optional prefix to filter files.</param>
  /// <param name="ct">Cancellation token.</param>
  /// <returns>A result containing a list of file keys.</returns>
  Task<Result<IReadOnlyList<string>>> ListFilesAsync(string bucketName, string? prefix = null, CancellationToken ct = default);

  /// <summary>
  /// Checks if a file exists in the specified bucket.
  /// </summary>
  /// <param name="bucketName">The name of the bucket.</param>
  /// <param name="key">The key (path) of the file.</param>
  /// <param name="ct">Cancellation token.</param>
  /// <returns>A result indicating whether the file exists.</returns>
  Task<Result<bool>> FileExistsAsync(string bucketName, string key, CancellationToken ct = default);
}
