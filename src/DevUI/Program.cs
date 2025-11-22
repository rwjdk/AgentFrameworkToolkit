using AgentFrameworkToolkit.Anthropic;
using Microsoft.Agents.AI.DevUI;
using Microsoft.Agents.AI.Hosting;
using ServiceDefaults;
using AgentFrameworkToolkit.AzureOpenAI;
using AgentFrameworkToolkit.Google;
using AgentFrameworkToolkit.Mistral;
using AgentFrameworkToolkit.OpenAI;
using AgentFrameworkToolkit.XAI;
using Anthropic.SDK.Constants;
using GenerativeAI;
using Mistral.SDK;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

string? azureOpenAiEndpoint = builder.Configuration[SecretKeys.AzureOpenAIEndpoint];
string? azureOpenAiApiKey = builder.Configuration[SecretKeys.AzureOpenAIApiKey];
string? openAIApiKey = builder.Configuration[SecretKeys.OpenAIApiKey];
string? mistralApiKey = builder.Configuration[SecretKeys.MistralApiKey];
string? googleApiKey = builder.Configuration[SecretKeys.GoogleApiKey];
string? anthropicApiKey = builder.Configuration[SecretKeys.AnthropicApiKey];
string? xAIApiKey = builder.Configuration[SecretKeys.XAIApiKey];

if (HasValidValues(azureOpenAiEndpoint, azureOpenAiApiKey))
{
    const string agentName = "Azure OpenAI Agent";
    builder.AddAIAgent(agentName, (_, _) => new AzureOpenAIAgentFactory(azureOpenAiEndpoint, azureOpenAiApiKey).CreateAgent(deploymentModelName: "gpt-4.1-mini", name: agentName));
}

if (HasValidValues(openAIApiKey))
{
    const string agentName = "OpenAI Agent";
    builder.AddAIAgent(agentName, (_, _) => new OpenAIAgentFactory(openAIApiKey).CreateAgent(model: "gpt-4.1-mini", name: agentName));
}

if (HasValidValues(googleApiKey))
{
    const string agentName = "Google Agent";
    builder.AddAIAgent(agentName, (_, _) => new GoogleAgentFactory(googleApiKey).CreateAgent(model: GoogleAIModels.Gemini25Flash, name: agentName));
}

if (HasValidValues(mistralApiKey))
{
    const string agentName = "Mistral Agent";
    builder.AddAIAgent(agentName, (_, _) => new MistralAgentFactory(mistralApiKey).CreateAgent(model: ModelDefinitions.MistralSmall, name: agentName));
}

if (HasValidValues(anthropicApiKey))
{
    const string agentName = "Anthropic Agent";
    builder.AddAIAgent(agentName, (_, _) => new AnthropicAgentFactory(anthropicApiKey).CreateAgent(model: AnthropicModels.Claude35Haiku, maxTokenCount: 1000, name: agentName));
}

if (HasValidValues(xAIApiKey))
{
    const string agentName = "XAI Agent";
    builder.AddAIAgent(agentName, (_, _) => new XAIAgentFactory(xAIApiKey).CreateAgent(model: "grok-4-1-fast-non-reasoning", name: agentName));
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

bool HasValidValues(params string?[] values)
{
    foreach (string value in values)
    {
        if (value?.ToUpperInvariant() is null or "" or "-" or "NONE")
        {
            return false;
        }
    }

    return true;
}