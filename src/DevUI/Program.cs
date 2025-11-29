using AgentFrameworkToolkit.Anthropic;
using AgentFrameworkToolkit.AzureOpenAI;
using AgentFrameworkToolkit.GitHub;
using AgentFrameworkToolkit.Google;
using AgentFrameworkToolkit.Mistral;
using AgentFrameworkToolkit.OpenAI;
using AgentFrameworkToolkit.OpenRouter;
using AgentFrameworkToolkit.XAI;
using Microsoft.Agents.AI.DevUI;
using Microsoft.Agents.AI.Hosting;
using ServiceDefaults;

#pragma warning disable OPENAI001

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

string? azureOpenAiEndpoint = builder.Configuration[SecretKeys.AzureOpenAIEndpoint];
string? azureOpenAiApiKey = builder.Configuration[SecretKeys.AzureOpenAIApiKey];
string? openAIApiKey = builder.Configuration[SecretKeys.OpenAIApiKey];
string? mistralApiKey = builder.Configuration[SecretKeys.MistralApiKey];
string? googleApiKey = builder.Configuration[SecretKeys.GoogleApiKey];
string? anthropicApiKey = builder.Configuration[SecretKeys.AnthropicApiKey];
string? xAIApiKey = builder.Configuration[SecretKeys.XAIApiKey];
string? openRouterApiKey = builder.Configuration[SecretKeys.OpenRouterApiKey];
string? gitHubPatToken = builder.Configuration[SecretKeys.GitHubPatToken];

if (HasValidValues(azureOpenAiEndpoint, azureOpenAiApiKey))
{
    const string agentName = "Azure OpenAI Agent";
    builder.AddAIAgent(agentName, (_, _) => new AzureOpenAIAgentFactory(azureOpenAiEndpoint!, azureOpenAiApiKey!).CreateAgent(new OpenAIAgentOptionsForResponseApiWithoutReasoning()
    {
        Name = agentName,
        Model = OpenAIChatModels.Gpt41Mini
    }));
}

if (HasValidValues(openAIApiKey))
{
    const string agentName = "OpenAI Agent";
    builder.AddAIAgent(agentName, (_, _) => new OpenAIAgentFactory(openAIApiKey!).CreateAgent(new OpenAIAgentOptionsForChatClientWithoutReasoning
    {
        Name = agentName,
        Model = OpenAIChatModels.Gpt41Mini,
    }));
}

if (HasValidValues(googleApiKey))
{
    const string agentName = "Google Agent";
    builder.AddAIAgent(agentName, (_, _) => new GoogleAgentFactory(googleApiKey!).CreateAgent(new GoogleAgentOptions
    {
        Name = agentName,
        Model = GoogleChatModels.Gemini25Flash
    }));
}

if (HasValidValues(mistralApiKey))
{
    const string agentName = "Mistral Agent";
    builder.AddAIAgent(agentName, (_, _) => new MistralAgentFactory(mistralApiKey!).CreateAgent(new MistralAgentOptions
    {
        Name = agentName,
        Model = MistalChatModels.MistralSmall
    }));
}

if (HasValidValues(anthropicApiKey))
{
    const string agentName = "Anthropic Agent";
    builder.AddAIAgent(agentName, (_, _) => new AnthropicAgentFactory(anthropicApiKey!).CreateAgent(new AnthropicAgentOptions
    {
        Model = AnthropicChatModels.ClaudeHaiku45,
        MaxOutputTokens = 1000,
        Name = agentName
    }));
}

if (HasValidValues(xAIApiKey))
{
    const string agentName = "XAI Agent";
    builder.AddAIAgent(agentName, (_, _) => new XAIAgentFactory(xAIApiKey!).CreateAgent(new OpenAIAgentOptionsForChatClientWithoutReasoning
    {
        Name = agentName,
        Model = XAIChatModels.Grok41FastNonReasoning
    }));
}

if (HasValidValues(openRouterApiKey))
{
    const string agentName = "OpenRouter Agent";
    builder.AddAIAgent(agentName, (_, _) => new OpenRouterAgentFactory(openRouterApiKey!).CreateAgent(new OpenAIAgentOptionsForChatClientWithoutReasoning
    {
        Name = agentName,
        Model = OpenRouterChatModels.OpenAI.Gpt41Mini
    }));
}

if (HasValidValues(gitHubPatToken))
{
    const string agentName = "GitHub Models Agent";
    builder.AddAIAgent(agentName, (_, _) => new GitHubAgentFactory(gitHubPatToken!).CreateAgent(new GitHubAgentOptions
    {
        Name = agentName,
        Model = "microsoft/Phi-4-mini-instruct"
    }));
}

// Register Services needed to run DevUI
builder.Services.AddOpenAIResponses();
builder.Services.AddOpenAIConversations();

WebApplication app = builder.Build();

if (builder.Environment.IsDevelopment())
{
    //Needed for DevUI to function 
    app.MapOpenAIResponses();
    app.MapOpenAIConversations();
    app.MapDevUI();
}

app.UseHttpsRedirection();

app.Run();
return;

bool HasValidValues(params string?[] values)
{
    foreach (string? value in values)
    {
        if (value?.ToUpperInvariant() is null or "" or "-" or "NONE")
        {
            return false;
        }
    }

    return true;
}