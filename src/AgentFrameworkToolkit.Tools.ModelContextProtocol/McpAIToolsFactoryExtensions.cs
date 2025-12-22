using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;

namespace AgentFrameworkToolkit.Tools.ModelContextProtocol;

/// <summary>
/// Extension Methods for AIToolsFactory to get Tools from MCP
/// </summary>
public static class McpAIToolsFactoryExtensions
{
    /// <summary>
    /// Get AI Tools from a Remote MCP server
    /// </summary>
    /// <param name="factory">The AIToolsFactory</param>
    /// <param name="remoteMcpUrl">the URL for the Remote MCP Server</param>
    /// <param name="additionalHeaders">Optional Headers to send for the Remote MCP server</param>
    /// <returns></returns>
    public static async Task<McpClientTools> GetToolsFromRemoteMcpAsync(this AIToolsFactory factory, string remoteMcpUrl, Dictionary<string, string>? additionalHeaders = null)
    {
        HttpClientTransportOptions options = new()
        {
            TransportMode = HttpTransportMode.AutoDetect,
            Endpoint = new Uri(remoteMcpUrl),
            AdditionalHeaders = additionalHeaders,
        };
        return await factory.GetToolsFromRemoteMcpAsync(options);
    }

    /// <summary>
    /// Get AI Tools from a Remote MCP server
    /// </summary>
    /// <param name="factory">The AIToolsFactory</param>
    /// <param name="options">Options for the Remote Server</param>
    /// <returns></returns>
    public static async Task<McpClientTools> GetToolsFromRemoteMcpAsync(this AIToolsFactory factory, HttpClientTransportOptions options)
    {
        McpClient client = await McpClient.CreateAsync(new HttpClientTransport(options));
        IList<McpClientTool> mcpTools = await client.ListToolsAsync();
        return new McpClientTools
        {
            McpClient = client,
            Tools = mcpTools.Cast<AITool>().ToList()
        };
    }

    /// <summary>
    /// Get AI Tools from a Local MCP server
    /// </summary>
    /// <param name="factory">The AIToolsFactory</param>
    /// <param name="command">The command to run (Example 'npx')</param>
    /// <param name="arguments">The arguments for the command</param>
    /// <returns></returns>
    public static async Task<McpClientTools> GetToolsFromLocalMcpAsync(this AIToolsFactory factory, string command, IList<string>? arguments)
    {
        StdioClientTransportOptions options = new()
        {
            Command = command,
            Arguments = arguments,
        };
        return await GetToolsFromLocalMcpAsync(factory, options);
    }

    /// <summary>
    /// Get AI Tools from a Local MCP server
    /// </summary>
    /// <param name="factory">The AIToolsFactory</param>
    /// <param name="options">The options for the local MCP server</param>
    /// <returns></returns>
    public static async Task<McpClientTools> GetToolsFromLocalMcpAsync(this AIToolsFactory factory, StdioClientTransportOptions options)
    {
        McpClient client = await McpClient.CreateAsync(new StdioClientTransport(options));
        IList<McpClientTool> mcpTools = await client.ListToolsAsync();
        return new McpClientTools
        {
            McpClient = client,
            Tools = mcpTools.Cast<AITool>().ToList()
        };
    }
}

/// <summary>
/// Represent an MCP Client and its tools
/// </summary>
public class McpClientTools : IAsyncDisposable
{
    /// <summary>
    /// The client hosting the tools (need to be disposed after tools usage)
    /// </summary>
    public required McpClient McpClient { get; set; }

    /// <summary>
    /// The Tools from the MCP Server
    /// </summary>
    public required IList<AITool> Tools { get; set; }

    /// <summary>
    /// Dispose the underlying Client
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await McpClient.DisposeAsync();
    }
}
