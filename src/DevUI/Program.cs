using Microsoft.Agents.AI.DevUI;
using Microsoft.Agents.AI.Hosting;
using ServiceDefaults;
using AgentFrameworkToolkit.AzureOpenAI;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

string? azureOpenAiEndpoint = builder.Configuration[SecretKeys.AzureOpenAIEndpoint];
string? azureOpenAiApiKey = builder.Configuration[SecretKeys.AzureOpenAIApiKey];

if (!string.IsNullOrWhiteSpace(azureOpenAiEndpoint) && !string.IsNullOrWhiteSpace(azureOpenAiApiKey))
{
    const string agentName = "Azure OpenAI Agent";
    builder.AddAIAgent(agentName, (_, _) => new AzureOpenAIAgentFactory(azureOpenAiEndpoint, azureOpenAiApiKey).CreateAgent(deploymentModelName: "gpt-4.1-mini", name: agentName));
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