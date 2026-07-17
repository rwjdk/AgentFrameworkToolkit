using JetBrains.Annotations;
using Microsoft.Agents.AI;

namespace AgentFrameworkToolkit.MicrosoftFoundry;

/// <summary>
/// An Agent targeting Microsoft Foundry
/// </summary>
/// <param name="innerAgent">The inner generic Agent</param>
[PublicAPI]
public class MicrosoftFoundryAgent(AIAgent innerAgent) : Agent(innerAgent);
