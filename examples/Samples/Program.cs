Console.Clear();

//await Samples.Providers.Anthropic.Run();
//await Samples.Providers.OpenAI.Run();
//await Samples.Providers.GitHub.Run();
await Samples.Providers.AzureOpenAI.RunAsync();
//await Samples.Providers.Mistral.Run();
//await Samples.Providers.Google.Run();
//await Samples.Providers.XAI.RunAsync();
//await Samples.Providers.OpenRouter.RunAsync();

Console.WriteLine("Done");