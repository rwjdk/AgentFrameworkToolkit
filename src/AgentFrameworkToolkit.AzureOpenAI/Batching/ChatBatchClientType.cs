namespace AgentFrameworkToolkit.AzureOpenAI.Batching;

/// <summary>
/// The batch endpoint style to use.
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