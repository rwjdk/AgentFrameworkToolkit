using Microsoft.Extensions.AI;
using System.Net;
using System.Text.RegularExpressions;

namespace AgentFrameworkToolkit.Tools.Common;

/// <summary>
/// Tools Related to Website Content
/// </summary>
public static class WebsiteTools
{
    /// <summary>
    /// Get All Website Tools
    /// </summary>
    /// <returns>The Tools</returns>
    public static IList<AITool> All(GetContentOfPageOptions? getContentOfPageOptions = null)
    {
        return
        [
            GetContentOfPage(getContentOfPageOptions)
        ];
    }

    /// <summary>
    /// Get the raw content of a website
    /// <param name="options">Optional options for tool</param>
    /// <param name="toolName">Name of tool</param>
    /// <param name="toolDescription">Description of Tool</param>
    /// </summary>
    /// <returns></returns>
    public static AITool GetContentOfPage(GetContentOfPageOptions? options = null, string? toolName = "get_content_of_url", string? toolDescription = "Get the content of a webpage from a URL")
    {
        GetContentOfPageOptions optionToUse = options ?? new GetContentOfPageOptions();
        return AIFunctionFactory.Create(async (string url) => await GetContentAsync(url, optionToUse), toolName, toolDescription);
    }

    private static async Task<string> GetContentAsync(string url, GetContentOfPageOptions options)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("URL cannot be null or whitespace.", nameof(url));
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri) || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException("URL must be an absolute HTTP/HTTPS URL.", nameof(url));
        }

        using HttpClient httpClient = new();
        using HttpResponseMessage response = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        if (options.StripMarkup)
        {
            content = StripMarkup(content);
        }

        return content;
    }

    private static string StripMarkup(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        string withoutScripts = Regex.Replace(html, "<(script|style)[^>]*?>.*?</\\1>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        string withoutTags = Regex.Replace(withoutScripts, "<[^>]+>", " ");
        string decoded = WebUtility.HtmlDecode(withoutTags);
        string normalizedWhitespace = Regex.Replace(decoded, "\\s+", " ").Trim();

        return normalizedWhitespace;
    }
}

/// <summary>
/// Options of GetContentOfPageTool Tool
/// </summary>
public class GetContentOfPageOptions
{
    /// <summary>
    /// If tool should Strip away markup (HTML, JS and CSS) leaving only raw text (Default = true)
    /// </summary>
    public bool StripMarkup { get; set; } = true;
}
