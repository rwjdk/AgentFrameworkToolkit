namespace AgentFrameworkToolkit.OpenAI.Batching;

/// <summary>
/// Status of a batch
/// </summary>
public enum BatchRunStatus
{
    /// <summary>
    /// Validating
    /// </summary>
    Validating,

    /// <summary>
    /// Failed
    /// </summary>
    Failed,

    /// <summary>
    /// In Progress
    /// </summary>
    InProgress,

    /// <summary>
    /// Finalizing
    /// </summary>
    Finalizing,

    /// <summary>
    /// Completed
    /// </summary>
    Completed,

    /// <summary>
    /// Expired
    /// </summary>
    Expired,

    /// <summary>
    /// Cancelling
    /// </summary>
    Cancelling,

    /// <summary>
    /// Cancelled
    /// </summary>
    Cancelled,

    /// <summary>
    /// Unknown Status
    /// </summary>
    Unknown
}