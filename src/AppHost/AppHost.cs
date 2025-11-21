using Aspire.Hosting;
using Projects;
using ServiceDefaults;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<ParameterResource> azureOpenAiEndpoint = builder.AddParameter(SecretKeys.AzureOpenAIEndpoint, secret: false);
IResourceBuilder<ParameterResource> azureOpenAiApiKey = builder.AddParameter(SecretKeys.AzureOpenAIApiKey, secret: true);

builder.AddProject<DevUI>("DevUI")
    .WithEnvironment(SecretKeys.AzureOpenAIEndpoint, azureOpenAiEndpoint)
    .WithEnvironment(SecretKeys.AzureOpenAIApiKey, azureOpenAiApiKey);

builder.Build().Run();
