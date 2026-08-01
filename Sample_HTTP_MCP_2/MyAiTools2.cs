using McpHttpServer.Attributes;

namespace Sample_HTTP_MCP_2;

[McpHandler]
public class MyAiTools2
{
    [McpTool("calculate_difference", "Calculates the difference between two numbers.")]
    public int CalculateDifference([McpParameter] int a, [McpParameter] int b)
    {
        return a - b;
    }

    [McpResource("system://status2", "System Status 2", "Returns the current system status for Server 2.", "application/json")]
    public string GetStatus()
    {
        return "{ \"status\": \"online\", \"server\": \"Server 2\" }";
    }
}
