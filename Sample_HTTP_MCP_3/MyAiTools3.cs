using McpHttpServer.Attributes;

namespace Sample_HTTP_MCP_3;

[McpHandler]
public class MyAiTools3
{
    [McpTool("calculate_product", "Calculates the product of two numbers.")]
    public int CalculateProduct([McpParameter] int a, [McpParameter] int b)
    {
        return a * b;
    }

    [McpResource("system://status3", "System Status 3", "Returns the current system status for Server 3.", "application/json")]
    public string GetStatus()
    {
        return "{ \"status\": \"online\", \"server\": \"Server 3\" }";
    }
}
