using Projects;
using ServiceDefaults;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

string description = "If you don't have a value then enter the a dash '-' to ignore";
IResourceBuilder<ParameterResource> azureOpenAiEndpoint = builder.AddParameter(SecretKeys.AzureOpenAIEndpoint, secret: false).WithDescription(description);
IResourceBuilder<ParameterResource> azureOpenAiApiKey = builder.AddParameter(SecretKeys.AzureOpenAIApiKey, secret: true).WithDescription(description);
IResourceBuilder<ParameterResource> mistralApiKey = builder.AddParameter(SecretKeys.MistralApiKey, secret: true).WithDescription(description);
IResourceBuilder<ParameterResource> googleApiKey = builder.AddParameter(SecretKeys.GoogleApiKey, secret: true).WithDescription(description);
IResourceBuilder<ParameterResource> anthropicApiKey = builder.AddParameter(SecretKeys.AnthropicApiKey, secret: true).WithDescription(description);
IResourceBuilder<ParameterResource> xaiApiKey = builder.AddParameter(SecretKeys.XAIApiKey, secret: true).WithDescription(description);
IResourceBuilder<ParameterResource> openAiApiKey = builder.AddParameter(SecretKeys.OpenAIApiKey, secret: true).WithDescription(description);

builder.AddProject<DevUI>("DevUI")
    .WithEnvironment(SecretKeys.AzureOpenAIEndpoint, azureOpenAiEndpoint)
    .WithEnvironment(SecretKeys.AzureOpenAIApiKey, azureOpenAiApiKey)
    .WithEnvironment(SecretKeys.MistralApiKey, mistralApiKey)
    .WithEnvironment(SecretKeys.GoogleApiKey, googleApiKey)
    .WithEnvironment(SecretKeys.AnthropicApiKey, anthropicApiKey)
    .WithEnvironment(SecretKeys.XAIApiKey, xaiApiKey)
    .WithEnvironment(SecretKeys.OpenAIApiKey, openAiApiKey);

builder.Build().Run();
