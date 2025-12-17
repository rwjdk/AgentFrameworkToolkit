using AgentFrameworkToolkit.Tools;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;

namespace AgentFrameworkToolkit.ModelContextProtocol;

public static class McpAIToolsFactoryExtensions
{
    public static async Task<IList<AITool>> GetToolsFromRemoteMcpAsync(this AIToolsFactory factory, string remoteMcpUrl, Dictionary<string, string>? additionalHeaders = null)
    {
        HttpClientTransportOptions options = new()
        {
            TransportMode = HttpTransportMode.AutoDetect,
            Endpoint = new Uri(remoteMcpUrl),
            AdditionalHeaders = additionalHeaders,
        };
        return await factory.GetToolsFromRemoteMcpAsync(options);
    }

    public static async Task<IList<AITool>> GetToolsFromRemoteMcpAsync(this AIToolsFactory factory, HttpClientTransportOptions options)
    {
        await using McpClient client = await McpClient.CreateAsync(new HttpClientTransport(options));
        IList<McpClientTool> mcpTools = await client.ListToolsAsync();
        return mcpTools.Cast<AITool>().ToList();
    }

    public static async Task<IList<AITool>> GetToolsFromLocalMcpAsync(this AIToolsFactory factory, string command, IList<string>? arguments)
    {
        StdioClientTransportOptions options = new()
        {
            Command = command,
            Arguments = arguments,
        };
        return await GetToolsFromLocalMcpAsync(factory, options);
    }

    public static async Task<IList<AITool>> GetToolsFromLocalMcpAsync(this AIToolsFactory factory, StdioClientTransportOptions options)
    {
        await using McpClient client = await McpClient.CreateAsync(new StdioClientTransport(options));
        IList<McpClientTool> mcpTools = await client.ListToolsAsync();
        return mcpTools.Cast<AITool>().ToList();
    }
}