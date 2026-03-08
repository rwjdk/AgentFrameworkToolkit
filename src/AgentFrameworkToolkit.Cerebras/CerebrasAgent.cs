using JetBrains.Annotations;
using Microsoft.Agents.AI;

namespace AgentFrameworkToolkit.Cerebras;

/// <summary>
/// An Agent targeting Cerebras
/// </summary>
/// <param name="innerAgent">The inner generic Agent</param>
[PublicAPI]
public class CerebrasAgent(AIAgent innerAgent) : Agent(innerAgent);
