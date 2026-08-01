using McpHttpServer;

var builder = WebApplication.CreateBuilder(args);

// Optional: Optimize Kestrel for HTTP/2 Streams
builder.ConfigureKestrelForMcp();

// Add MCP Server
builder.Services.AddMcpHttpServer(options =>
{
    options.ServerName = "SampleMCPServer";
    options.Endpoint = "/mcp"; // The HTTP endpoint MCP clients will hit
    options.RequireAuthorization = false; // Set to true to require auth
    options.UseDefaultCors = true; 
});

var app = builder.Build();

app.Use(async (context, next) =>
{
    if (!context.Request.Headers.TryGetValue("X-Gateway-Secret", out var secret) || secret != "SecretGatewayKey123!")
    {
        context.Response.StatusCode = 403;
        await context.Response.WriteAsync("Forbidden: Direct access not allowed.");
        return;
    }
    await next();
});

// Map the /mcp endpoint
app.MapMcpHttpServer();

app.Run();
