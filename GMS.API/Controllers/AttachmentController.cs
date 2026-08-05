using GMS.Application.DTOs.Grievances;
using GMS.Application.Interfaces;
using GMS.Domain.Entities;
using GMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GMS.API.Controllers;

[Route("api")]
public class AttachmentController : BaseApiController
{
    private readonly IFileStorageService _fileStorageService;
    private readonly IGrievanceRepository _grievanceRepository;
    private readonly IGenericRepository<Attachment> _attachmentRepository;

    public AttachmentController(
        IFileStorageService fileStorageService,
        IGrievanceRepository grievanceRepository,
        IGenericRepository<Attachment> attachmentRepository)
    {
        _fileStorageService = fileStorageService;
        _grievanceRepository = grievanceRepository;
        _attachmentRepository = attachmentRepository;
    }

    [HttpPost("grievances/{id}/attachments")]
    [Authorize(Policy = "CitizenPolicy")]
    public async Task<IActionResult> UploadAttachment(int id, IFormFile file)
    {
        var grievance = await _grievanceRepository.GetByIdAsync(id);
        if (grievance == null) return NotFound("Grievance not found.");

        if (grievance.SubmittedByUserId != CurrentUserId)
            return Forbid("You can only add attachments to your own grievances.");

        if (grievance.Status != GrievanceStatus.Submitted)
            return BadRequest(new { error = "Attachments can only be added when status is Submitted." });

        try
        {
            using var stream = file.OpenReadStream();
            var relativePath = await _fileStorageService.SaveFileAsync(stream, file.FileName, file.ContentType);

            var attachment = new Attachment
            {
                GrievanceId = id,
                FileName = file.FileName,
                ContentType = file.ContentType,
                FilePath = relativePath,
                UploadedAt = DateTime.UtcNow
            };

            var created = await _attachmentRepository.AddAsync(attachment);

            return CreatedAtAction(nameof(GetAttachments), new { id = id }, new { attachmentId = created.Id, path = relativePath });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("grievances/{id}/attachments")]
    [Authorize]
    public async Task<IActionResult> GetAttachments(int id)
    {
        var grievance = await _grievanceRepository.GetByIdAsync(id);
        if (grievance == null) return NotFound("Grievance not found.");

        // Authorize based on role
        if (CurrentUserRole == "Citizen" && grievance.SubmittedByUserId != CurrentUserId)
            return Forbid("You cannot view attachments for this grievance.");
        
        if (CurrentUserRole == "Officer" && grievance.AssignedOfficerId != CurrentUserId)
            return Forbid("You cannot view attachments for this grievance.");

        var allAttachments = await _attachmentRepository.GetAllAsync(); // For a real app, use a specific repository method filtering by GrievanceId
        var attachments = allAttachments.Where(a => a.GrievanceId == id).Select(a => new AttachmentResponse
        {
            Id = a.Id,
            FileName = a.FileName,
            ContentType = a.ContentType,
            UploadedAt = a.UploadedAt,
            DownloadUrl = $"/api/attachments/{a.Id}" // As per requirement to have an endpoint for the file
        });

        return Ok(attachments);
    }

    [HttpGet("attachments/{attachmentId}")]
    [Authorize]
    public async Task<IActionResult> DownloadAttachment(int attachmentId)
    {
        var attachment = await _attachmentRepository.GetByIdAsync(attachmentId);
        if (attachment == null) return NotFound("Attachment not found.");

        var grievance = await _grievanceRepository.GetByIdAsync(attachment.GrievanceId);
        if (grievance == null) return NotFound();

        // Check permissions
        if (CurrentUserRole == "Citizen" && grievance.SubmittedByUserId != CurrentUserId)
            return Forbid("You cannot view this attachment.");
        
        if (CurrentUserRole == "Officer" && grievance.AssignedOfficerId != CurrentUserId)
            return Forbid("You cannot view this attachment.");

        // Read physical file
        var currentDir = Directory.GetCurrentDirectory();
        // Adjust if webroot is different
        var webRoot = Path.Combine(currentDir, "wwwroot");
        var fullPath = Path.Combine(webRoot, attachment.FilePath.TrimStart('/', '\\'));
        
        if (!System.IO.File.Exists(fullPath))
            return NotFound("File does not exist on disk.");

        var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(fullPath, out var contentType))
        {
            contentType = "application/octet-stream";
        }

        var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
        return File(stream, contentType, attachment.FileName); // Forces download
    }

    [HttpDelete("attachments/{attachmentId}")]
    [Authorize(Policy = "CitizenPolicy")]
    public async Task<IActionResult> DeleteAttachment(int attachmentId)
    {
        var attachment = await _attachmentRepository.GetByIdAsync(attachmentId);
        if (attachment == null) return NotFound("Attachment not found.");

        var grievance = await _grievanceRepository.GetByIdAsync(attachment.GrievanceId);
        if (grievance == null) return NotFound();

        if (grievance.SubmittedByUserId != CurrentUserId)
            return Forbid("You can only delete your own attachments.");

        if (grievance.Status != GrievanceStatus.Submitted)
            return BadRequest(new { error = "Attachments can only be deleted when grievance status is Submitted." });

        _fileStorageService.DeleteFile(attachment.FilePath);
        await _attachmentRepository.DeleteAsync(attachment);

        return NoContent();
    }
}
