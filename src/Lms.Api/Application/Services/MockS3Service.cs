using System.Collections.Concurrent;
using Lms.Api.Application.Interfaces;

namespace Lms.Api.Application.Services;

/// <summary>
/// Mock implementation of S3 service that simulates AWS S3 operations in-memory.
/// In production, this would be replaced with actual AWS SDK implementation.
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

    public Task<string> UploadFileAsync(string bucketName, string key, string content, string contentType = "text/plain")
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

        return Task.FromResult(url);
    }

    public Task<string?> DownloadFileAsync(string bucketName, string key)
    {
        ValidateBucketName(bucketName);
        ValidateKey(key);

        if (!_storage.TryGetValue(bucketName, out var bucket))
        {
            _logger.LogWarning("[MOCK S3] Bucket '{Bucket}' not found", bucketName);
            return Task.FromResult<string?>(null);
        }

        if (!bucket.TryGetValue(key, out var file))
        {
            _logger.LogWarning("[MOCK S3] File '{Key}' not found in bucket '{Bucket}'", key, bucketName);
            return Task.FromResult<string?>(null);
        }

        _logger.LogInformation(
            "[MOCK S3] Downloaded file from bucket '{Bucket}' with key '{Key}' ({Size} bytes)",
            bucketName, key, file.Content.Length);

        return Task.FromResult<string?>(file.Content);
    }

    public Task<bool> DeleteFileAsync(string bucketName, string key)
    {
        ValidateBucketName(bucketName);
        ValidateKey(key);

        if (!_storage.TryGetValue(bucketName, out var bucket))
        {
            return Task.FromResult(false);
        }

        var removed = bucket.TryRemove(key, out _);
        
        if (removed)
        {
            _logger.LogInformation("[MOCK S3] Deleted file '{Key}' from bucket '{Bucket}'", key, bucketName);
        }

        return Task.FromResult(removed);
    }

    public Task<IReadOnlyList<string>> ListFilesAsync(string bucketName, string? prefix = null)
    {
        ValidateBucketName(bucketName);

        if (!_storage.TryGetValue(bucketName, out var bucket))
        {
            return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
        }

        var keys = bucket.Keys
            .Where(k => string.IsNullOrEmpty(prefix) || k.StartsWith(prefix))
            .OrderBy(k => k)
            .ToList();

        _logger.LogInformation(
            "[MOCK S3] Listed {Count} files in bucket '{Bucket}' with prefix '{Prefix}'",
            keys.Count, bucketName, prefix ?? "(none)");

        return Task.FromResult<IReadOnlyList<string>>(keys);
    }

    public Task<bool> FileExistsAsync(string bucketName, string key)
    {
        ValidateBucketName(bucketName);
        ValidateKey(key);

        if (!_storage.TryGetValue(bucketName, out var bucket))
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(bucket.ContainsKey(key));
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

