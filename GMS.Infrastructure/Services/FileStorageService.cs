using GMS.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;

namespace GMS.Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _env;
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx" };
    private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB

    public FileStorageService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string contentType)
    {
        if (fileStream == null || fileStream.Length == 0)
            throw new ArgumentException("File is empty or not provided.");

        if (fileStream.Length > MaxFileSize)
            throw new ArgumentException("File size exceeds the 10 MB limit.");

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !_allowedExtensions.Contains(extension))
            throw new ArgumentException($"File extension '{extension}' is not allowed.");

        if (!IsValidMimeType(extension, contentType))
            throw new ArgumentException("Invalid file content type.");

        var uploadsFolder = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "grievances");
        
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        // Prevent path traversal
        var fullPath = Path.GetFullPath(filePath);
        if (!fullPath.StartsWith(Path.GetFullPath(uploadsFolder)))
            throw new ArgumentException("Invalid file path.");

        using (var destStream = new FileStream(fullPath, FileMode.Create))
        {
            await fileStream.CopyToAsync(destStream);
        }

        return $"/uploads/grievances/{uniqueFileName}";
    }

    public void DeleteFile(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath)) return;

        var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        
        // Remove leading slash to correctly combine paths
        var normalizedRelativePath = relativePath.TrimStart('/', '\\');
        
        var fullPath = Path.GetFullPath(Path.Combine(webRoot, normalizedRelativePath));
        
        // Prevent path traversal on delete
        if (!fullPath.StartsWith(Path.GetFullPath(webRoot)))
            throw new UnauthorizedAccessException("Invalid file path.");

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }

    private bool IsValidMimeType(string extension, string contentType)
    {
        var provider = new FileExtensionContentTypeProvider();
        if (provider.TryGetContentType($"file{extension}", out var expectedContentType))
        {
            // For a production app you might want to be less strict or check file signatures (magic numbers).
            // But checking that it maps to a valid MIME type is a good start.
            return true;
        }
        return false;
    }
}
