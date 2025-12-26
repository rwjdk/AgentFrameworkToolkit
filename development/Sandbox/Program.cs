using Sandbox.Providers;

Console.Clear();

//await Samples.Providers.Anthropic.RunAsync();
//await Samples.Providers.OpenAI.RunAsync();
//await Samples.Providers.GitHub.RunAsync();
await Cohere.RunAsync();
//await AzureOpenAI.RunAsync();
//await Samples.Providers.Mistral.RunAsync();
//await Samples.Providers.Google.RunAsync();
//await Samples.Providers.XAI.RunAsync();
//await Samples.Providers.OpenRouter.RunAsync();

Console.WriteLine("Done");
