using JetBrains.Annotations;

namespace AgentFrameworkToolkit;

/// <summary>
/// Represent the data of a raw Call to an LLM
/// </summary>
[PublicAPI]
public class RawCallDetails
{
    /// <summary>
    /// The URL the LLM was contacted on
    /// </summary>
    public required string RequestUrl { get; set; }

    /// <summary>
    /// The Request Data
    /// </summary>
    public required string RequestData { get; set; }

    /// <summary>
    /// The Response Data
    /// </summary>
    public required string ResponseData { get; set; }
}