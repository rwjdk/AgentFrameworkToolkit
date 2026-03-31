namespace AgentFrameworkToolkit.OpenAI.Batching;

/// <summary>
/// Type of Request.
/// </summary>
public enum ChatBatchClientType
{
    /// <summary>
    /// Uses the Chat Completions batch endpoint.
    /// </summary>
    ChatClient,

    /// <summary>
    /// Uses the Responses API batch endpoint.
    /// </summary>
    ResponsesApi
}