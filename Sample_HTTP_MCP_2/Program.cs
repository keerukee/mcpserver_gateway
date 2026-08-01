using McpHttpServer;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:5178");

builder.ConfigureKestrelForMcp();

builder.Services.AddMcpHttpServer(options =>
{
    options.ServerName = "Sample_HTTP_MCP_2";
    options.Endpoint = "/mcp";
    options.RequireAuthorization = false;
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

app.MapMcpHttpServer();

app.Run();
