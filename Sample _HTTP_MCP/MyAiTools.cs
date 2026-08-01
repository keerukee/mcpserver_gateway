using McpHttpServer.Attributes;

namespace SampleHttpMcp;

[McpHandler]
public class MyAiTools
{
    [McpTool("calculate_sum", "Calculates the sum of two numbers.")]
    public int CalculateSum([McpParameter] int a, [McpParameter] int b)
    {
        return a + b;
    }

    [McpResource("system://status", "System Status", "Returns the current system status.", "application/json")]
    public string GetStatus()
    {
        return "{ \"status\": \"online\", \"uptime\": \"99.9%\" }";
    }

    [McpPrompt("greet_user", "Generates a greeting prompt.")]
    public string Greet([McpArgument] string username)
    {
        return $"Hello {username}, how can I help you today?";
    }
}
