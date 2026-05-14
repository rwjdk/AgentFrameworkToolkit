using System.Reflection;
using System.Runtime.ExceptionServices;
using AgentFrameworkToolkit.Tools.Common;

namespace AgentFrameworkToolkit.Tests.Tools;

public class ToolConfinementTests
{
    [Fact]
    public void HttpClientToolsGuard_AllowsExactDomain()
    {
        MethodInfo guardMethod = GetNonPublicStaticMethod(typeof(HttpClientTools), "GuardThatOperationsAreWithinConfinedDomains");
        HttpClientToolsOptions options = new()
        {
            ConfinedToTheseDomains = ["api.example.com"]
        };

        InvokeStaticMethod(guardMethod, "https://api.example.com/v1/customers", null, options);
    }

    [Fact]
    public void HttpClientToolsGuard_AllowsRelativeUrlWithMatchingBaseAddress()
    {
        MethodInfo guardMethod = GetNonPublicStaticMethod(typeof(HttpClientTools), "GuardThatOperationsAreWithinConfinedDomains");
        HttpClientToolsOptions options = new()
        {
            ConfinedToTheseDomains = ["api.example.com"]
        };

        InvokeStaticMethod(guardMethod, "/v1/customers", new Uri("https://api.example.com"), options);
    }

    [Fact]
    public void HttpClientToolsGuard_RejectsSubdomainWhenOnlyParentDomainIsAllowed()
    {
        MethodInfo guardMethod = GetNonPublicStaticMethod(typeof(HttpClientTools), "GuardThatOperationsAreWithinConfinedDomains");
        HttpClientToolsOptions options = new()
        {
            ConfinedToTheseDomains = ["example.com"]
        };

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            InvokeStaticMethod(guardMethod, "https://api.example.com/v1/customers", null, options));

        Assert.Contains("allowed domain", exception.Message);
    }

    [Fact]
    public void HttpClientToolsGuard_AllowsAnyDomainWhenRestrictionIsNull()
    {
        MethodInfo guardMethod = GetNonPublicStaticMethod(typeof(HttpClientTools), "GuardThatOperationsAreWithinConfinedDomains");
        HttpClientToolsOptions options = new()
        {
            ConfinedToTheseDomains = null
        };

        InvokeStaticMethod(guardMethod, "not-a-valid-absolute-url", null, options);
    }

    [Fact]
    public void WebsiteToolsGuard_AllowsExactDomain()
    {
        MethodInfo guardMethod = GetNonPublicStaticMethod(typeof(WebsiteTools), "GuardThatOperationsAreWithinConfinedDomains");
        GetContentOfPageOptions options = new()
        {
            ConfinedToTheseDomains = ["docs.example.com"]
        };

        InvokeStaticMethod(guardMethod, new Uri("https://docs.example.com/page"), options);
    }

    [Fact]
    public void WebsiteToolsGuard_RejectsSubdomainWhenOnlyParentDomainIsAllowed()
    {
        MethodInfo guardMethod = GetNonPublicStaticMethod(typeof(WebsiteTools), "GuardThatOperationsAreWithinConfinedDomains");
        GetContentOfPageOptions options = new()
        {
            ConfinedToTheseDomains = ["example.com"]
        };

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            InvokeStaticMethod(guardMethod, new Uri("https://docs.example.com/page"), options));

        Assert.Contains("allowed domain", exception.Message);
    }

    [Fact]
    public void FileSystemToolsGuard_ThrowsInvalidOperationExceptionForOutsideFolder()
    {
        MethodInfo guardMethod = GetNonPublicStaticMethod(typeof(FileSystemTools), "GuardThatOperationsAreWithinConfinedFolderPaths");
        string rootPath = Path.Combine(Path.GetTempPath(), $"aft-{Guid.NewGuid():N}");
        string allowedPath = Path.Combine(rootPath, "allowed");
        string blockedPath = Path.Combine(rootPath, "blocked");

        Directory.CreateDirectory(allowedPath);
        Directory.CreateDirectory(blockedPath);
        FileSystemToolsOptions options = new()
        {
            ConfinedToTheseFolderPaths = [allowedPath]
        };

        try
        {
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
                InvokeStaticMethod(guardMethod, blockedPath, options));

            Assert.Contains("allowed Path", exception.Message);
        }
        finally
        {
            if (Directory.Exists(rootPath))
            {
                Directory.Delete(rootPath, recursive: true);
            }
        }
    }

    private static MethodInfo GetNonPublicStaticMethod(Type type, string methodName)
    {
        MethodInfo? methodInfo = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(methodInfo);
        return methodInfo;
    }

    private static void InvokeStaticMethod(MethodInfo methodInfo, params object?[] parameters)
    {
        try
        {
            methodInfo.Invoke(null, parameters);
        }
        catch (TargetInvocationException exception) when (exception.InnerException != null)
        {
            ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
        }
    }
}
