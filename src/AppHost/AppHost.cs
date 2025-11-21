using Aspire.Hosting;
using Projects;
using ServiceDefaults;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<ParameterResource> azureOpenAiEndpoint = builder.AddParameter(SecretKeys.AzureOpenAIEndpoint, secret: false);
IResourceBuilder<ParameterResource> azureOpenAiApiKey = builder.AddParameter(SecretKeys.AzureOpenAIApiKey, secret: true);
IResourceBuilder<ParameterResource> mistralApiKey = builder.AddParameter(SecretKeys.MistralApiKey, secret: true);
IResourceBuilder<ParameterResource> googleApiKey = builder.AddParameter(SecretKeys.GoogleApiKey, secret: true);
IResourceBuilder<ParameterResource> anthropicApiKey = builder.AddParameter(SecretKeys.AnthropicApiKey, secret: true);
IResourceBuilder<ParameterResource> xaiApiKey = builder.AddParameter(SecretKeys.XAIApiKey, secret: true);
IResourceBuilder<ParameterResource> openAiApiKey = builder.AddParameter(SecretKeys.OpenAIApiKey, secret: true);

builder.AddProject<DevUI>("DevUI")
    .WithEnvironment(SecretKeys.AzureOpenAIEndpoint, azureOpenAiEndpoint)
    .WithEnvironment(SecretKeys.AzureOpenAIApiKey, azureOpenAiApiKey)
    .WithEnvironment(SecretKeys.MistralApiKey, mistralApiKey)
    .WithEnvironment(SecretKeys.GoogleApiKey, googleApiKey)
    .WithEnvironment(SecretKeys.AnthropicApiKey, anthropicApiKey)
    .WithEnvironment(SecretKeys.XAIApiKey, xaiApiKey)
    .WithEnvironment(SecretKeys.OpenAIApiKey, openAiApiKey);

builder.Build().Run();
