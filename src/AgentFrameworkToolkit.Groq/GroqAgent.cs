using JetBrains.Annotations;
using Microsoft.Agents.AI;

namespace AgentFrameworkToolkit.Groq;

/// <summary>
/// An agent targeting Groq.
/// </summary>
/// <param name="innerAgent">The inner generic agent.</param>
[PublicAPI]
public class GroqAgent(AIAgent innerAgent) : Agent(innerAgent);
