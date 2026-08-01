using McpHttpServer;

var builder = WebApplication.CreateBuilder(args);

// Configure the new port: 5179
builder.WebHost.UseUrls("http://localhost:5179");

builder.ConfigureKestrelForMcp();

builder.Services.AddMcpHttpServer(options =>
{
    options.ServerName = "Sample_HTTP_MCP_3";
    options.Endpoint = "/mcp";
    options.RequireAuthorization = false;
    options.UseDefaultCors = true;
});

var app = builder.Build();

// Securing the internal server using the Gateway Secret
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
