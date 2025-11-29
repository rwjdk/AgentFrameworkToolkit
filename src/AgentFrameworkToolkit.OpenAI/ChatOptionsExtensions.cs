using Microsoft.Extensions.AI;
using OpenAI.Chat;
using OpenAI.Responses;

#pragma warning disable OPENAI001

namespace AgentFrameworkToolkit.OpenAI;

/// <summary>
/// OpenAI Specific Extensions for ChatOptions
/// </summary>
public static class ChatOptionsExtensions
{
    /// <summary>
    /// Apply OpenAI Responses API Reasoning Settings on ChatOptions
    /// </summary>
    /// <param name="chatOptions">The ChatOption</param>
    /// <param name="reasoningEffortLevel">The Reasoning Effort</param>
    /// <param name="reasoningSummaryVerbosity">The Reasoning Summary</param>
    /// <returns>The ChatOptions</returns>
    public static ChatOptions WithOpenAIResponsesApiReasoning(this ChatOptions chatOptions, ResponseReasoningEffortLevel? reasoningEffortLevel = null, ResponseReasoningSummaryVerbosity? reasoningSummaryVerbosity = null)
    {
        chatOptions.RawRepresentationFactory = _ =>
        {
            ResponseCreationOptions responseCreationOptions = new()
            {
                ReasoningOptions = new ResponseReasoningOptions
                {
                    ReasoningEffortLevel = reasoningEffortLevel,
                    ReasoningSummaryVerbosity = reasoningSummaryVerbosity
                }
            };
            return responseCreationOptions;
        };

        return chatOptions;
    }

    /// <summary>
    /// Apply OpenAI ChatClient Reasoning Settings on ChatOptions
    /// </summary>
    /// <param name="chatOptions">The ChatOption</param>
    /// <param name="reasoningEffortLevel">The Reasoning Effort</param>
    /// <returns>The ChatOptions</returns>
    public static ChatOptions WithOpenAIChatClientReasoning(this ChatOptions chatOptions, ChatReasoningEffortLevel? reasoningEffortLevel = null)
    {
        chatOptions.RawRepresentationFactory = _ => new ChatCompletionOptions
        {
            ReasoningEffortLevel = reasoningEffortLevel
        };

        return chatOptions;
    }
}