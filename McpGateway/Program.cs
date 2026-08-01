using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"] ?? "SuperSecretKeyForMcpGatewayAuthentication123!");

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // For dev purposes
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization(options =>
{
    // Require authentication by default for everything
    options.FallbackPolicy = options.DefaultPolicy;
});

// 2. Add YARP Http Forwarder
builder.Services.AddHttpForwarder();

var app = builder.Build();

// Quick endpoint to generate a token for testing
app.MapGet("/generate-token", () =>
{
    var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Issuer = jwtSettings["Issuer"],
        Audience = jwtSettings["Audience"],
        Expires = DateTime.UtcNow.AddHours(1),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };
    var token = tokenHandler.CreateToken(tokenDescriptor);
    return Results.Ok(new { token = tokenHandler.WriteToken(token) });
}).AllowAnonymous(); // Explicitly allow anonymous to generate token

app.UseRouting();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// 3. Map Dynamic Routing using IHttpForwarder
var httpClient = new HttpMessageInvoker(new SocketsHttpHandler
{
    UseProxy = false,
    AllowAutoRedirect = false,
    AutomaticDecompression = System.Net.DecompressionMethods.None,
    UseCookies = false
});

var transformer = new CustomTransformer();

app.Map("/{mcpName}/{**rest}", async (HttpContext context, string mcpName, string? rest, Yarp.ReverseProxy.Forwarder.IHttpForwarder forwarder) =>
{
    string? destUrl = null;
    var lines = new List<string>();
    using (var stream = new FileStream("routes.csv", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
    using (var reader = new StreamReader(stream))
    {
        string? currentLine;
        while ((currentLine = await reader.ReadLineAsync()) != null)
        {
            lines.Add(currentLine);
        }
    }

    foreach(var line in lines)
    {
        var parts = line.Split(',');
        if (parts.Length == 2 && parts[0] == mcpName)
        {
            destUrl = parts[1];
            break;
        }
    }

    if (destUrl == null)
    {
        return Results.NotFound(new { error = "MCP Server not found in database." });
    }

    var error = await forwarder.SendAsync(context, destUrl, httpClient, new Yarp.ReverseProxy.Forwarder.ForwarderRequestConfig(), transformer);
    
    if (error != Yarp.ReverseProxy.Forwarder.ForwarderError.None)
    {
        var errorFeature = context.GetForwarderErrorFeature();
        var exception = errorFeature?.Exception;
    }
    return Results.Empty;
});

app.Run();

class CustomTransformer : Yarp.ReverseProxy.Forwarder.HttpTransformer
{
    public override async ValueTask TransformRequestAsync(HttpContext httpContext, HttpRequestMessage proxyRequest, string destinationPrefix, CancellationToken cancellationToken)
    {
        await base.TransformRequestAsync(httpContext, proxyRequest, destinationPrefix, cancellationToken);

        var rest = httpContext.Request.RouteValues["rest"]?.ToString();
        var path = string.IsNullOrEmpty(rest) ? "/mcp" : $"/mcp/{rest}";
        proxyRequest.RequestUri = new Uri(destinationPrefix + path + httpContext.Request.QueryString);

        proxyRequest.Headers.TryAddWithoutValidation("X-Gateway-Secret", "SecretGatewayKey123!");
    }
}
