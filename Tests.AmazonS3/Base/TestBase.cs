using Apps.AmazonS3.Constants;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using System.Text.Json;

namespace Tests.AmazonS3.Base;
public class TestBase
{
    public InvocationContext InvocationContext { get; set; } = new();

    public FileManager FileManager { get; init; }

    public TestContext? TestContext { get; set; }

    public string TestBucketName;

    private IEnumerable<InvocationContext> _invocationContexts { get; init; }

    protected TestBase()
    {
        var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        var credentialGroups = config
            .GetSection("ConnectionDefinition").GetChildren()
            .Select(section =>
                section.GetChildren()
               .Select(child => new AuthenticationCredentialsProvider(child.Key, child.Value!))
            ).ToList();

        _invocationContexts = credentialGroups.Select(credentialGroup => new InvocationContext
        {
            AuthenticationCredentialsProviders = credentialGroup
        }).ToList();

        FileManager = new FileManager();

        TestBucketName = config["TestBucketNameForMultibucketConnectionType"]
            ?? throw new Exception("Test bucket name for multibucket coneection type not found.");
    }

    public static IEnumerable<object[]> InvocationContexts
    {
        get
        {
            var testBase = new TestBase();
            return testBase._invocationContexts.Select(ctx => new object[] { ctx });
        }
    }

    public static IEnumerable<object[]> SingleBucketInvocationContexts
    {
        get
        {
            var testBase = new TestBase();
            return testBase._invocationContexts
                .Where(ctx => ctx.AuthenticationCredentialsProviders
                    .Any(p => p.Value == ConnectionTypes.SingleBucket && p.KeyName == CredNames.ConnectionType))
                .Select(ctx => new object[] { ctx });
        }
    }

    public static IEnumerable<object[]> AllBucketInvocationContexts
    {
        get
        {
            var testBase = new TestBase();
            return testBase._invocationContexts
                .Where(ctx => ctx.AuthenticationCredentialsProviders
                    .Any(p => 
                        (p.Value == ConnectionTypes.AllBuckets ||
                        p.Value == ConnectionTypes.S3Compatible ||
                        p.Value == ConnectionTypes.AssumeRole) && 
                        p.KeyName == CredNames.ConnectionType)
                    )
                .Select(ctx => new object[] { ctx });
        }
    }

    public static string? GetConnectionTypeFromDynamicData(MethodInfo method, object[]? data)
    {
        var context = data?[0] as InvocationContext
             ?? throw new ArgumentNullException(nameof(data));

        var connectionTypeValue = context.AuthenticationCredentialsProviders
            .FirstOrDefault(p => p.KeyName == CredNames.ConnectionType)
            ?.Value
            ?? throw new ArgumentNullException(nameof(data));

        var connectionTypeFieldName = typeof(ConnectionTypes)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .FirstOrDefault(f => f.IsLiteral && f.GetRawConstantValue()?.Equals(connectionTypeValue) == true)
            ?.Name;

        return $"{method.Name} with {connectionTypeFieldName} connection";
    }

    private static JsonSerializerOptions PrintResultOptions => new() { WriteIndented = true };

    public void PrintResult(object? obj)
    {
        TestContext?.WriteLine(JsonSerializer.Serialize(obj, PrintResultOptions));
    }
}
