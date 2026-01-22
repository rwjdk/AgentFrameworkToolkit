namespace AgentFrameworkToolkit.Tools;

/// <summary>
/// AI Tool Attribute you can apply to Methods to indicate they are AI Tools
/// </summary>
/// <param name="name">Name of the tool (snake_case is recommended)</param>
/// <param name="description">The description of the Tool</param>
public class AIToolAttribute(string? name = null, string? description = null) : Attribute
{
    /// <summary>
    /// Name of the tool (snake_case is recommended)
    /// </summary>
    public string? Name { get; } = name;

    /// <summary>
    /// The description of the Tool
    /// </summary>
    public string? Description { get; } = description;
}