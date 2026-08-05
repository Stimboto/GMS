using GMS.Application.DTOs.AI;
using GMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GMS.API.Controllers;

[Route("api/ai")]
public class AIController : BaseApiController
{
    private readonly IAIService _aiService;

    public AIController(IAIService aiService)
    {
        _aiService = aiService;
    }

    [HttpGet("health")]
    public async Task<IActionResult> GetHealth(CancellationToken cancellationToken)
    {
        var status = await _aiService.GetHealthStatusAsync(cancellationToken);
        return Ok(status);
    }

    [HttpPost("category")]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<IActionResult> TestCategory([FromBody] AITestRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _aiService.CategorizeGrievanceAsync(request.Title, request.Description, cancellationToken);
            return Ok(new AIResponse { Result = result });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { Error = "AI service encountered an error." });
        }
    }

    [HttpPost("priority")]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<IActionResult> TestPriority([FromBody] AITestRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _aiService.DetectPriorityAsync(request.Title, request.Description, cancellationToken);
            return Ok(new AIResponse { Result = result });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { Error = "AI service encountered an error." });
        }
    }

    [HttpPost("summary")]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<IActionResult> TestSummary([FromBody] AITestRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _aiService.GenerateSummaryAsync(request.Title, request.Description, cancellationToken);
            return Ok(new AIResponse { Result = result });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { Error = "AI service encountered an error." });
        }
    }
}
