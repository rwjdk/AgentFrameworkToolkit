# AgentFrameworkToolkit.AmazonBedrock

Amazon Bedrock provider package for Agent Framework Toolkit.

## Install

```powershell
dotnet add package AgentFrameworkToolkit.AmazonBedrock
```

## Usage (AgentFactory)

```csharp
using AgentFrameworkToolkit.AmazonBedrock;
using Amazon;
using Microsoft.Extensions.AI;

var factory = new AmazonBedrockAgentFactory(new AmazonBedrockConnection
{
    Region = RegionEndpoint.EUNorth1,
    ApiKey = "<your-bedrock-api-key>",
});

var agent = factory.CreateAgent(new AmazonBedrockAgentOptions
{
    Model = "eu.anthropic.claude-haiku-4-5-20251001-v1:0",
    Instructions = "You are a helpful assistant",
    Tools = [AIFunctionFactory.Create((string city) => \"sunny\", \"get_weather\")],
});

var response = await agent.RunAsync(\"Hello\");
Console.WriteLine(response.Text);
```
