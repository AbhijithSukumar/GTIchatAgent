var builder = WebApplication.CreateBuilder(args);


// Register MCP Server services
builder.Services.AddMcpServer()
    .WithHttpTransport(options =>
    {
        // Stateless mode is recommended for production/cloud deployments
        options.Stateless = true;
    })
    .WithToolsFromAssembly(typeof(Program).Assembly);

// Enable CORS if you need browser-based clients to connect
builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy => {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
// Map the MCP endpoint
app.MapMcp("/mcp");

app.Run("http://localhost:5100");
