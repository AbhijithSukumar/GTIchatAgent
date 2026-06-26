GtiChatAgent
GtiChatAgent is a modular, observable AI agent framework built with Semantic Kernel for .NET. It uses the Google Gemini model for reasoning and integrates with a custom Model Context Protocol (MCP) server to provide specialized tools.

🚀 Architecture
GtiChatClient (Orchestrator): A .NET console/service application that manages conversation history, Semantic Kernel planning, and agent orchestration.

GtiChatServer (MCP Server): A .NET-based MCP server providing domain-specific tools, exposed via the stdio or SSE transport for the client to consume.

🛠 Tech Stack
Language: C# (.NET 10)

AI Orchestration: Microsoft Semantic Kernel

Model: Google Gemini (via Vertex AI or Google AI Studio)

Observability:

OpenTelemetry: Native .NET instrumentation for distributed tracing.

LangSmith: Monitoring and evaluation for LLM interactions.

Connectivity: Model Context Protocol (MCP)

⚙️ Setup
Prerequisites
.NET 8.0 or 9.0 SDK

A Google Gemini API Key

LangSmith account for observability

Configuration
Clone the repository:

Bash
git clone <your-repo-url>
cd GtiChatAgent
Environment Variables:
Create an appsettings.json or use environment variables for your configuration:

JSON
{
  "Gemini": { "ApiKey": "your_api_key" },
  "LangSmith": { "ApiKey": "your_langsmith_key" }
}
Running the Project
Start the MCP Server:

Bash
cd GtiChatServer
dotnet run
Run the Client:
In a separate terminal:

Bash
cd GtiChatClient
dotnet run
🔍 Observability & Telemetry
This project uses Microsoft.Extensions.Telemetry to hook into OpenTelemetry. Traces are automatically collected and sent to LangSmith to visualize your agent's decision-making process, including the specific MCP tools invoked by the Gemini model.
