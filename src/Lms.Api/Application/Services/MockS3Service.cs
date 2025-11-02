using System.Collections.Concurrent;
using Lms.Api.Application.Interfaces;
using Lms.Api.Common.Results;

namespace Lms.Api.Application.Services;

/// <summary>
/// Mock implementation of S3 service that simulates AWS S3 operations in-memory.
/// In production, this would be replaced with actual AWS SDK implementation.
/// Follows Open/Closed Principle - can be replaced without modifying client code.
/// </summary>
internal sealed class MockS3Service : IS3Service
{
  private readonly ILogger<MockS3Service> _logger;

  // Simulates S3 storage: bucket -> (key -> content)
  private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, StoredFile>> _storage = new();

  public MockS3Service(ILogger<MockS3Service> logger)
  {
    _logger = logger;
  }

  public Task<Result<string>> UploadFileAsync(
    string bucketName,
    string key,
    string content,
    string contentType,
    CancellationToken ct = default)
  {
    try
    {
      ValidateBucketName(bucketName);
      ValidateKey(key);

      // Get or create bucket
      var bucket = _storage.GetOrAdd(bucketName, _ => new ConcurrentDictionary<string, StoredFile>());

      // Store file
      var file = new StoredFile(content, contentType, DateTime.UtcNow);
      bucket[key] = file;

      var url = $"https://{bucketName}.s3.amazonaws.com/{key}";

      _logger.LogInformation(
        "[MOCK S3] Uploaded file to bucket '{Bucket}' with key '{Key}' ({Size} bytes)",
        bucketName, key, content.Length);

      return Task.FromResult(Result<string>.Success(url));
    }
    catch (ArgumentException ex)
    {
      _logger.LogWarning("[MOCK S3] Invalid input: {Message}", ex.Message);
      return Task.FromResult(Result<string>.Failure(Errors.Common.Validation(ex.Message)));
    }
  }

  public Task<Result<string>> DownloadFileAsync(string bucketName, string key, CancellationToken ct = default)
  {
    try
    {
      ValidateBucketName(bucketName);
      ValidateKey(key);

      if (!_storage.TryGetValue(bucketName, out var bucket))
      {
        _logger.LogWarning("[MOCK S3] Bucket '{Bucket}' not found", bucketName);
        return Task.FromResult(Result<string>.Failure(
          Errors.Common.NotFound($"Bucket '{bucketName}' not found")));
      }

      if (!bucket.TryGetValue(key, out var file))
      {
        _logger.LogWarning("[MOCK S3] File '{Key}' not found in bucket '{Bucket}'", key, bucketName);
        return Task.FromResult(Result<string>.Failure(
          Errors.Common.NotFound($"File '{key}' not found in bucket '{bucketName}'")));
      }

      _logger.LogInformation(
        "[MOCK S3] Downloaded file from bucket '{Bucket}' with key '{Key}' ({Size} bytes)",
        bucketName, key, file.Content.Length);

      return Task.FromResult(Result<string>.Success(file.Content));
    }
    catch (ArgumentException ex)
    {
      _logger.LogWarning("[MOCK S3] Invalid input: {Message}", ex.Message);
      return Task.FromResult(Result<string>.Failure(Errors.Common.Validation(ex.Message)));
    }
  }

  public Task<Result> DeleteFileAsync(string bucketName, string key, CancellationToken ct = default)
  {
    try
    {
      ValidateBucketName(bucketName);
      ValidateKey(key);

      if (!_storage.TryGetValue(bucketName, out var bucket))
      {
        return Task.FromResult(Result.Failure(
          Errors.Common.NotFound($"Bucket '{bucketName}' not found")));
      }

      var removed = bucket.TryRemove(key, out _);

      if (!removed)
      {
        return Task.FromResult(Result.Failure(
          Errors.Common.NotFound($"File '{key}' not found in bucket '{bucketName}'")));
      }

      _logger.LogInformation("[MOCK S3] Deleted file '{Key}' from bucket '{Bucket}'", key, bucketName);
      return Task.FromResult(Result.Success());
    }
    catch (ArgumentException ex)
    {
      _logger.LogWarning("[MOCK S3] Invalid input: {Message}", ex.Message);
      return Task.FromResult(Result.Failure(Errors.Common.Validation(ex.Message)));
    }
  }

  public Task<Result<IReadOnlyList<string>>> ListFilesAsync(
    string bucketName,
    string? prefix = null,
    CancellationToken ct = default)
  {
    try
    {
      ValidateBucketName(bucketName);

      if (!_storage.TryGetValue(bucketName, out var bucket))
      {
        return Task.FromResult(Result<IReadOnlyList<string>>.Success(Array.Empty<string>()));
      }

      var keys = bucket.Keys
        .Where(k => string.IsNullOrEmpty(prefix) || k.StartsWith(prefix))
        .OrderBy(k => k)
        .ToList()
        .AsReadOnly();

      _logger.LogInformation(
        "[MOCK S3] Listed {Count} files in bucket '{Bucket}' with prefix '{Prefix}'",
        keys.Count, bucketName, prefix ?? "(none)");

      return Task.FromResult(Result<IReadOnlyList<string>>.Success(keys));
    }
    catch (ArgumentException ex)
    {
      _logger.LogWarning("[MOCK S3] Invalid input: {Message}", ex.Message);
      return Task.FromResult(Result<IReadOnlyList<string>>.Failure(Errors.Common.Validation(ex.Message)));
    }
  }

  public Task<Result<bool>> FileExistsAsync(string bucketName, string key, CancellationToken ct = default)
  {
    try
    {
      ValidateBucketName(bucketName);
      ValidateKey(key);

      if (!_storage.TryGetValue(bucketName, out var bucket))
      {
        return Task.FromResult(Result<bool>.Success(false));
      }

      var exists = bucket.ContainsKey(key);
      return Task.FromResult(Result<bool>.Success(exists));
    }
    catch (ArgumentException ex)
    {
      _logger.LogWarning("[MOCK S3] Invalid input: {Message}", ex.Message);
      return Task.FromResult(Result<bool>.Failure(Errors.Common.Validation(ex.Message)));
    }
  }

  private static void ValidateBucketName(string bucketName)
  {
    if (string.IsNullOrWhiteSpace(bucketName))
    {
      throw new ArgumentException("Bucket name cannot be empty", nameof(bucketName));
    }
  }

  private static void ValidateKey(string key)
  {
    if (string.IsNullOrWhiteSpace(key))
    {
      throw new ArgumentException("Key cannot be empty", nameof(key));
    }
  }

  private sealed record StoredFile(string Content, string ContentType, DateTime UploadedAt);
}

