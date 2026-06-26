using Microsoft.AspNetCore.Mvc;
using GtiMcpClient.Services;
using System.Diagnostics;
using GtiMcpClient.DTOs;

namespace GtiMcpClient.Controllers;

/// <summary>
/// Controller responsible for handling chat interactions between the user and the Logistics Agent.
/// It acts as the API layer, exposing endpoints that the frontend or external clients can consume.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly LogisticsAgentService _agentService;

    // ActivitySource used for OpenTelemetry tracing.
    // This allows us to track the lifecycle of the request in tools like LangSmith.
    private static readonly ActivitySource _source = new("GtiMcpClient");

    /// <summary>
    /// Initializes the controller with the required LogisticsAgentService.
    /// Dependency Injection ensures the service is properly configured and ready for use.
    /// </summary>
    public ChatController(LogisticsAgentService agentService)
        => _agentService = agentService;

    /// <summary>
    /// POST endpoint to process a user's natural language request.
    /// Triggers the orchestration logic through the service layer.
    /// </summary>
    /// <param name="request">The ChatRequest object containing the user's prompt, ID, and name.</param>
    /// <returns>A JSON response containing the agent's generated reply.</returns>
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] ChatRequest request)
    {
        // Start a new activity span for tracking the LLM interaction in OTel/LangSmith.
        // This will be the "Parent" span for the interaction.
        using var activity = _source.StartActivity("GeminiChat");

        // Tagging the activity provides context in monitoring tools (e.g., input display).
        activity?.SetTag("gen_ai.prompt", request.Prompt);

        try
        {
            // Delegate the heavy lifting of orchestration and LLM communication to the service layer.
            var response = await _agentService.ProcessChatAsync(request.Prompt, request.UserId, request.Username);

            // Tagging the completion output for visibility in trace analysis.
            activity?.SetTag("gen_ai.completion", response);

            return Ok(new { response });
        }
        catch (Exception ex)
        {
            // If something goes wrong (LLM timeout, tool failure), record the error 
            // in the trace so it shows up as "failed" in LangSmith.
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            return BadRequest(new { error = ex.Message });
        }
    }
}
