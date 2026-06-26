using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Google;

namespace GtiMcpClient.Services;

/// <summary>
/// Service responsible for orchestrating the LLM interaction for the logistics agent.
/// This service encapsulates the prompt engineering and Semantic Kernel execution logic.
/// </summary>
public class LogisticsAgentService
{
    private readonly Kernel _kernel;

    /// <summary>
    /// Injects the Semantic Kernel instance via Dependency Injection.
    /// </summary>
    /// <param name="kernel">The configured Semantic Kernel instance.</param>
    public LogisticsAgentService(Kernel kernel)
    {
        _kernel = kernel;
    }

    /// <summary>
    /// Processes a natural language request by preparing the context, setting system instructions,
    /// and invoking the LLM with automatic tool-call capabilities.
    /// </summary>
    /// <param name="prompt">The user's original query.</param>
    /// <param name="userId">The unique identifier of the user (used for tool calls).</param>
    /// <param name="username">The display name of the user (used for conversational context).</param>
    /// <returns>The final, conversationally formatted response from the LLM.</returns>
    public async Task<string> ProcessChatAsync(string prompt, string userId, string username)
    {
        // Arguments are used to pass context into the Kernel execution environment.
        var arguments = new KernelArguments { ["userId"] = userId };

        // The System Instruction provides the "Persona" and the "Rules of Engagement"
        // for the AI. This effectively acts as the boundary for user data handling.
        var systemInstruction = $@"
            You are a helpful logistics assistant. 
            The user's name is '{username}'.
            The current user has ID: '{userId}'. 
            - Always use this ID ({userId}) when calling tools that require a userId.
            - Always address the user by their name.
            - Do NOT mention the User ID in your response.
            - Use the available tools to provide data.
            - Keep the output conversational.";

        // Combine the system context with the actual user query.
        var finalPrompt = $"{systemInstruction}\n\nUser Request: {prompt}";

        // GeminiPromptExecutionSettings allows us to enable AutoInvokeKernelFunctions,
        // which tells the LLM: "If you need data, call the tools automatically."
        var geminiSettings = new GeminiPromptExecutionSettings
        {
            ToolCallBehavior = GeminiToolCallBehavior.AutoInvokeKernelFunctions
        };

        // Attach settings so the Kernel knows how to execute the specific model request.
        arguments.ExecutionSettings = new Dictionary<string, PromptExecutionSettings>
        {
            { "default", geminiSettings }
        };

        // Invoke the LLM orchestration. The Kernel will handle the loop of:
        // Prompt -> LLM decides to call Tool -> Kernel executes MCP tool -> Result back to LLM -> Final Response.
        var result = await _kernel.InvokePromptAsync(finalPrompt, arguments);

        return result.ToString();
    }
}
