using JetBrains.Annotations;
using Microsoft.Extensions.AI;

namespace AgentFrameworkToolkit.Tools.Common;

/// <summary>
/// Tools related to Weather
/// </summary>
[PublicAPI]
public static class WeatherTools
{
    /// <summary>
    /// Get All OpenWeatherMap Tools
    /// </summary>
    /// <param name="options">Options for OpenWeatherMap</param>
    /// <returns>Tools</returns>
    public static IList<AITool> All(OpenWeatherMapOptions options)
    {
        return
        [
            GetWeatherForCity(options)
        ];
    }
    
    /// <summary>
    /// Get Weather for a specific City
    /// </summary>
    /// <param name="options">Options for OpenWeatherMap</param>
    /// <param name="toolName">Name of tool</param>
    /// <param name="toolDescription">Description of Tool</param>
    /// <returns>AI Tool</returns>
    public static AITool GetWeatherForCity(OpenWeatherMapOptions options, string? toolName = null, string? toolDescription = null)
    {
        return AIFunctionFactory.Create(async (string city) =>
        {
            HttpClient httpClient = GetHttpClient(options);
            string units = options.PreferredUnits switch
            {
                WeatherOptionsUnits.Metric => "&units=metric",
                WeatherOptionsUnits.Imperial => "&units=imperial",
                _ => string.Empty
            };
            string requestUri =
                $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={options.ApiKey}&mode=xml{units}";
            HttpResponseMessage response = await httpClient.GetAsync(requestUri);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Unable to get weather: {response.StatusCode}: {response.ReasonPhrase}");
            }

            return await response.Content.ReadAsStringAsync();
        }, toolName ?? "get_weather_for_city", toolDescription ?? "Get weather for specified city. If country is needed to specify same name cities the define as <city>,<country_code>");
    }

    private static HttpClient GetHttpClient(OpenWeatherMapOptions options)
    {
        return options.HttpClientFactory?.Invoke() ?? new HttpClient();
    }
}

/// <summary>
/// Options for the Weather Tools
/// </summary>
[PublicAPI]
public class OpenWeatherMapOptions
{
    /// <summary>
    /// Preferred Unit to use in query
    /// </summary>
    public WeatherOptionsUnits? PreferredUnits { get; set; }

    /// <summary>
    /// API Key
    /// </summary>
    public required string ApiKey { get; set; }

    /// <summary>
    /// HTTP Client Factory (if not specified a new HTTPClient is generated)
    /// </summary>
    public Func<HttpClient>? HttpClientFactory { get; set; }
}

/// <summary>
/// Units of the Weather (Metric or Imperial)
/// </summary>
[PublicAPI]
public enum WeatherOptionsUnits
{
    /// <summary>
    /// Metric (Celsius)
    /// </summary>
    Metric,

    /// <summary>
    /// Imperial (Fahrenheit)
    /// </summary>
    Imperial
}
