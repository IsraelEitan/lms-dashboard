namespace Lms.Api.Application.Interfaces;

/// <summary>
/// Interface for AWS S3-like storage operations.
/// This simulates AWS S3 functionality without actual AWS SDK dependencies.
/// </summary>
public interface IS3Service
{
    /// <summary>
    /// Uploads a file to the specified S3 bucket.
    /// </summary>
    /// <param name="bucketName">The name of the S3 bucket</param>
    /// <param name="key">The key (path) where the file will be stored</param>
    /// <param name="content">The file content as a string</param>
    /// <param name="contentType">The content type (MIME type)</param>
    /// <returns>The URL of the uploaded file</returns>
    Task<string> UploadFileAsync(string bucketName, string key, string content, string contentType = "text/plain");

    /// <summary>
    /// Downloads a file from the specified S3 bucket.
    /// </summary>
    /// <param name="bucketName">The name of the S3 bucket</param>
    /// <param name="key">The key (path) of the file to download</param>
    /// <returns>The file content as a string</returns>
    Task<string?> DownloadFileAsync(string bucketName, string key);

    /// <summary>
    /// Deletes a file from the specified S3 bucket.
    /// </summary>
    /// <param name="bucketName">The name of the S3 bucket</param>
    /// <param name="key">The key (path) of the file to delete</param>
    /// <returns>True if successful</returns>
    Task<bool> DeleteFileAsync(string bucketName, string key);

    /// <summary>
    /// Lists all files in the specified S3 bucket.
    /// </summary>
    /// <param name="bucketName">The name of the S3 bucket</param>
    /// <param name="prefix">Optional prefix to filter files</param>
    /// <returns>List of file keys</returns>
    Task<IReadOnlyList<string>> ListFilesAsync(string bucketName, string? prefix = null);

    /// <summary>
    /// Checks if a file exists in the specified S3 bucket.
    /// </summary>
    /// <param name="bucketName">The name of the S3 bucket</param>
    /// <param name="key">The key (path) of the file</param>
    /// <returns>True if file exists</returns>
    Task<bool> FileExistsAsync(string bucketName, string key);
}

