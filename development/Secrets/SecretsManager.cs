using Microsoft.Extensions.Configuration;

namespace Secrets;

public class SecretsManager
{
    /* This SecretsManager relies on .NET User Secrets in the following format
    ************************************************************************************************************************************************
    {
      "OpenAiApiKey": "<value>",
      "AzureOpenAiEndpoint": "<value>",
      "AzureOpenAiKey": "<value>",
      "ChatDeploymentName": "<value>",
      "EmbeddingModelName": "<value>",
      "MicrosoftFoundryEndpoint" : "<value>",
      "AzureAiFoundryAgentId" : "<value>",
      "BingApiKey" : "<value>",
      "HuggingFaceApiKey": "<value>",
      "OpenRouterApiKet" : "<value>",
      "OpenRouterApiKey" : "<value>",
      "CohereApiKey" : "<value>",
      "CerebrasApiKey" : "<value>",
      "GroqApiKey" : "<value>",
      "ApplicationInsightsConnectionString" : "<value>",
      "GoogleGeminiApiKey" : "<value>",
      "XAiGrokApiKey" : "<value>",
      "TrelloApiKey" : "<value>",
      "TrelloToken" : "<value>",
      "AnthropicApiKey" : "<value>",
      "MistralApiKey" : "<value>",
      "AmazonBedrockApiKey" : "<value>",
      "EmailUsername" : "<value>",
      "EmailPassword" : "<value>"
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
        string azureAiFoundryAgentEndpoint = configurationRoot["MicrosoftFoundryEndpoint"] ?? string.Empty;
        string azureAiFoundryAgentId = configurationRoot["AzureAiFoundryAgentId"] ?? string.Empty;
        string bingApiKey = configurationRoot["BingApiKey"] ?? string.Empty;
        string huggingFaceApiKey = configurationRoot["HuggingFaceApiKey"] ?? string.Empty;
        string openRouterApiKey = configurationRoot["OpenRouterApiKey"] ?? string.Empty;
        string cohereApiKey = configurationRoot["CohereApiKey"] ?? string.Empty;
        string cerebrasApiKey = configurationRoot["CerebrasApiKey"] ?? string.Empty;
        string groqApiKey = configurationRoot["GroqApiKey"] ?? string.Empty;
        string applicationInsightsConnectionString = configurationRoot["ApplicationInsightsConnectionString"] ?? string.Empty;
        string googleGeminiApiKey = configurationRoot["GoogleGeminiApiKey"] ?? string.Empty;
        string xAiGrokApiKey = configurationRoot["XAiGrokApiKey"] ?? string.Empty;
        string trelloApiKey = configurationRoot["TrelloApiKey"] ?? string.Empty;
        string trelloToken = configurationRoot["TrelloToken"] ?? string.Empty;
        string anthropicApiKey = configurationRoot["AnthropicApiKey"] ?? string.Empty;
        string mistralApiKey = configurationRoot["MistralApiKey"] ?? string.Empty;
        string openWeatherApiKey = configurationRoot["OpenWeatherApiKey"] ?? string.Empty;
        string amazonBedrockApiKey = configurationRoot["AmazonBedrockApiKey"] ?? string.Empty;
        string emailUsername = configurationRoot["EmailUsername"] ?? string.Empty;
        string emailPassword = configurationRoot["EmailPassword"] ?? string.Empty;

        return new Secrets(
            openAiApiKey,
            azureOpenAiEndpoint,
            azureOpenAiKey,
            chatDeploymentName,
            embeddingModelName,
            azureAiFoundryAgentEndpoint,
            azureAiFoundryAgentId,
            bingApiKey,
            huggingFaceApiKey,
            openRouterApiKey,
            cohereApiKey,
            cerebrasApiKey,
            groqApiKey,
            applicationInsightsConnectionString,
            googleGeminiApiKey,
            xAiGrokApiKey,
            trelloApiKey,
            trelloToken,
            anthropicApiKey,
            mistralApiKey,
            openWeatherApiKey,
            amazonBedrockApiKey,
            emailUsername,
            emailPassword);
    }
}
