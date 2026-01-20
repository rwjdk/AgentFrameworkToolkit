using Microsoft.Extensions.Configuration;

namespace Secrets;

public class SecretsManager
{
    /* This SecretsManager relies on .NET User Secrets in the following format
    ************************************************************************************************************************************************
    {
      "OpenAiApiKey": "todo",
      "AzureOpenAiEndpoint": "todo",
      "AzureOpenAiKey": "todo",
      "ChatDeploymentName": "todo",
      "EmbeddingModelName": "todo",
      "AzureAiFoundryAgentEndpoint" : "todo",
      "AzureAiFoundryAgentId" : "todo",
      "BingApiKey" : "todo",
      "HuggingFaceApiKey": "todo",
      "OpenRouterApiKet" : "todo",
      "OpenRouterApiKey" : "todo",
      "CohereApiKey" : "todo",
      "ApplicationInsightsConnectionString" : "todo",
      "GoogleGeminiApiKey" : "todo",
      "XAiGrokApiKey" : "todo",
      "TrelloApiKey" : "todo",
      "TrelloToken" : "todo",
      "AnthropicApiKey" : "todo",
      "MistralApiKey" : "todo",
    }
    ************************************************************************************************************************************************
    - See the how-to guides on how to create your Azure Resources in the ReadMe
    - See https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets on how to work with user-secrets
    ************************************************************************************************************************************************
    */

    public static Secrets GetSecrets()
    {
        IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddUserSecrets<SecretsManager>().Build();
        string openAiApiKey = configurationRoot["OpenAiApiKey"] ?? string.Empty;
        string azureOpenAiEndpoint = configurationRoot["AzureOpenAiEndpoint"] ?? string.Empty;
        string azureOpenAiKey = configurationRoot["AzureOpenAiKey"] ?? string.Empty;
        string chatDeploymentName = configurationRoot["ChatDeploymentName"] ?? string.Empty;
        string embeddingModelName = configurationRoot["EmbeddingModelName"] ?? string.Empty;
        string azureAiFoundryAgentEndpoint = configurationRoot["AzureAiFoundryAgentEndpoint"] ?? string.Empty;
        string azureAiFoundryAgentId = configurationRoot["AzureAiFoundryAgentId"] ?? string.Empty;
        string bingApiKey = configurationRoot["BingApiKey"] ?? string.Empty;
        string githubPatToken = configurationRoot["GitHubPatToken"] ?? string.Empty;
        string huggingFaceApiKey = configurationRoot["HuggingFaceApiKey"] ?? string.Empty;
        string openRouterApiKey = configurationRoot["OpenRouterApiKey"] ?? string.Empty;
        string cohereApiKey = configurationRoot["CohereApiKey"] ?? string.Empty;
        string applicationInsightsConnectionString = configurationRoot["ApplicationInsightsConnectionString"] ?? string.Empty;
        string googleGeminiApiKey = configurationRoot["GoogleGeminiApiKey"] ?? string.Empty;
        string xAiGrokApiKey = configurationRoot["XAiGrokApiKey"] ?? string.Empty;
        string trelloApiKey = configurationRoot["TrelloApiKey"] ?? string.Empty;
        string trelloToken = configurationRoot["TrelloToken"] ?? string.Empty;
        string anthropicApiKey = configurationRoot["AnthropicApiKey"] ?? string.Empty;
        string mistralApiKey = configurationRoot["MistralApiKey"] ?? string.Empty;
        string openWeatherApiKey = configurationRoot["OpenWeatherApiKey"] ?? string.Empty;

        return new Secrets(
            openAiApiKey,
            azureOpenAiEndpoint,
            azureOpenAiKey,
            chatDeploymentName,
            embeddingModelName,
            azureAiFoundryAgentEndpoint,
            azureAiFoundryAgentId,
            bingApiKey,
            githubPatToken,
            huggingFaceApiKey,
            openRouterApiKey,
            cohereApiKey,
            applicationInsightsConnectionString,
            googleGeminiApiKey,
            xAiGrokApiKey,
            trelloApiKey,
            trelloToken,
            anthropicApiKey,
            mistralApiKey,
            openWeatherApiKey);
    }
}
