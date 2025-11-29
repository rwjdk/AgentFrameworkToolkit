using Azure.AI.Inference;

namespace AgentFrameworkToolkit.GitHub;

/// <summary>
/// Represents a connection for GitHub Models
/// </summary>
public class GitHubConnection
{
    /// <summary>
    /// The GitHub Personal Access Token (fine-grained with Models Access) 
    /// </summary>
    public required string PersonalAccessToken { get; set; }

    /// <summary>
    /// The timeout value of the LLM Call (if not defined the underlying infrastructure's default will be used)
    /// </summary>
    public TimeSpan? NetworkTimeout { get; set; }

    /// <summary>
    /// Action to set additional options on the underlying AzureAIInferenceClientOptions if needed
    /// </summary>
    public Action<AzureAIInferenceClientOptions>? AdditionalAzureAIInferenceClientOptions { get; set; }
}