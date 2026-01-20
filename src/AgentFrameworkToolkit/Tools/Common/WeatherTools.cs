using Microsoft.Extensions.AI;

namespace AgentFrameworkToolkit.Tools.Common;

/// <summary>
/// Tools to Get Weather
/// <param name="options">Options for the Weather Tools</param>
/// </summary>
public class WeatherTools(WeatherOptions options)
{
    /// <summary>
    /// Get Weather for a specific City
    /// </summary>
    /// <param name="toolName">Name of tool</param>
    /// <param name="toolDescription">Description of Tool</param>
    /// <returns>AI Tool</returns>
    public AITool GetWeatherForCity(string? toolName = "get_weather_for_city", string? toolDescription = null)
    {
        if (options.Provider == WeatherOptionsProvider.OpenWeatherMap && string.IsNullOrWhiteSpace(options.ApiKey))
        {
            throw new Exception("No API Key provided for OpenWeatherMap");
        }

        return AIFunctionFactory.Create((string city) => GetWeatherAsync(options, city), toolName, toolDescription);
    }

    private static async Task<string> GetWeatherAsync(WeatherOptions options, string query)
    {
        HttpClient httpClient = options.HttpClientFactory?.Invoke() ?? new HttpClient();

        return options.Provider switch
        {
            WeatherOptionsProvider.OpenWeatherMap => await OpenWeatherMap(),
            _ => throw new ArgumentOutOfRangeException()
        };

        async Task<string> OpenWeatherMap()
        {
            string units = options.PreferredUnits switch
            {
                WeatherOptionsUnits.Metric => "&units=metric",
                WeatherOptionsUnits.Imperial => "&units=imperial",
                _ => string.Empty
            };
            HttpResponseMessage response = await httpClient.GetAsync($"https://api.openweathermap.org/data/2.5/weather?q={query}&appid={options.ApiKey}&mode=xml{units}");
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Unable to get weather: {response.StatusCode}: {response.ReasonPhrase}");
            }

            return await response.Content.ReadAsStringAsync();
        }
    }
}

/// <summary>
/// Options for the Weather Tools
/// </summary>
public class WeatherOptions
{
    /// <summary>
    /// Configure WeatherOptions for OpenWeatherMap Provider
    /// </summary>
    /// <param name="apiKey">API Key</param>
    /// <param name="preferredUnits">Metric or Imperial (if not specified result will return in Kelvin)</param>
    /// <param name="httpClientFactory">HTTP Client Factory (if not specified a new HTTPClient is generated)</param>
    /// <returns>WeatherOptions</returns>
    public static WeatherOptions OpenWeatherMap(string apiKey, WeatherOptionsUnits? preferredUnits = null, Func<HttpClient>? httpClientFactory = null)
    {
        return new WeatherOptions
        {
            Provider = WeatherOptionsProvider.OpenWeatherMap,
            ApiKey = apiKey,
            PreferredUnits = preferredUnits,
            HttpClientFactory = httpClientFactory,
        };
    }

    /// <summary>
    /// Provider to use
    /// </summary>
    public WeatherOptionsProvider Provider { get; set; } = WeatherOptionsProvider.OpenWeatherMap; //Contributors: Feel free to add more here

    /// <summary>
    /// Preferred Unit to use in query
    /// </summary>
    public WeatherOptionsUnits? PreferredUnits { get; set; }

    /// <summary>
    /// API Key for Weather API
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// HTTP Client Factory (if not specified a new HTTPClient is generated)
    /// </summary>
    public Func<HttpClient>? HttpClientFactory { get; set; }
}

/// <summary>
/// Units of the Weather (Metric or Imperial)
/// </summary>
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

/// <summary>
/// Supported Providers of Weather APIs
/// </summary>
public enum WeatherOptionsProvider //Contributors: Feel free to add more here
{
    /// <summary>
    /// https://openweathermap.org/
    /// </summary>
    OpenWeatherMap,
}
