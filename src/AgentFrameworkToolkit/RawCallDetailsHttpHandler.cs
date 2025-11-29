using JetBrains.Annotations;
using System.Text.Json;

namespace AgentFrameworkToolkit;

/// <summary>
/// HTTP Handler for allowing Raw Call Details
/// </summary>
/// <param name="rawCallDetails">Action on how to consume the Raw Call Details</param>
[PublicAPI]
public class RawCallDetailsHttpHandler(Action<RawCallDetails> rawCallDetails) : HttpClientHandler
{
    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string requestString = await request.Content?.ReadAsStringAsync(cancellationToken)!;
        HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
        string responseString = await response.Content.ReadAsStringAsync(cancellationToken);

        rawCallDetails.Invoke(new RawCallDetails
        {
            RequestUrl = request.RequestUri!.AbsoluteUri,
            RequestData = MakePretty(requestString),
            ResponseData = MakePretty(responseString)
        });
        return response;

        static string MakePretty(string input)
        {
            try
            {
                JsonElement jsonElement = JsonSerializer.Deserialize<JsonElement>(input);
                return JsonSerializer.Serialize(jsonElement, new JsonSerializerOptions { WriteIndented = true });
            }
            catch
            {
                //Input is not JSON so treat as is
                return input;
            }
        }
    }
}