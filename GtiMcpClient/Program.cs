using GtiMcpClient.Services;
using Microsoft.SemanticKernel;
using ModelContextProtocol.Client;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// --- Configuration Setup ---
// Retrieves keys from User Secrets (Development) or Environment Variables (Production).
var geminiKey = builder.Configuration["Gemini:ApiKey"]
    ?? throw new Exception("Gemini API Key missing!");
var langSmithKey = builder.Configuration["LangSmith:ApiKey"]
    ?? throw new Exception("langSmithKey API Key missing!");

// Register standard ASP.NET Core services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Logging for visibility into the application's runtime behavior
builder.Services.AddLogging(c =>
{
    c.AddConsole();
    c.SetMinimumLevel(LogLevel.Debug);
});

// --- Semantic Kernel Configuration ---
// Configure the Kernel with Gemini and the necessary logging factory to enable internal telemetry.
builder.Services.AddSingleton(sp =>
{
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var kernelBuilder = Kernel.CreateBuilder();
    kernelBuilder.Services.AddSingleton(loggerFactory);

    return kernelBuilder
        .AddGoogleAIGeminiChatCompletion("gemini-3.1-flash-lite", geminiKey)
        .Build();
});

// Register the agent service so it can be injected into controllers
builder.Services.AddScoped<LogisticsAgentService>();

// --- OpenTelemetry Configuration ---
// Sets up distributed tracing to export logs and traces to LangSmith for AI monitoring.
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(serviceName: "gti-mcp-client"))
    .WithTracing(tracing => tracing
        .AddSource("Microsoft.SemanticKernel*", "GtiMcpClient") // Monitor kernel internals and our custom activity
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddConsoleExporter()
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri("https://api.smith.langchain.com/otel/v1/traces");
            options.Protocol = OtlpExportProtocol.HttpProtobuf;
            options.Headers = $"x-api-key={langSmithKey}";
        }));

builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

var app = builder.Build();

// --- Middleware Configuration ---
// Swagger documentation for API discovery and testing in Development mode.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => { options.RoutePrefix = "swagger"; });
}

// --- Startup MCP Tool Discovery ---
// Connect to the external MCP server and dynamically import available tools into the Kernel.
// This allows the AI agent to call airline logistics functions automatically.
var transport = new HttpClientTransport(new HttpClientTransportOptions { Endpoint = new Uri("http://localhost:5100/mcp") });
var mcpClient = await McpClient.CreateAsync(transport);
var kernel = app.Services.GetRequiredService<Kernel>();
var mcpTools = await mcpClient.ListToolsAsync();

kernel.ImportPluginFromFunctions("Logistics", mcpTools.Select(t => t.AsKernelFunction()).ToList());

// Map API controllers and launch the application.
app.MapControllers();
app.Run();
