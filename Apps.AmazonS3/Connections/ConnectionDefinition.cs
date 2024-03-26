using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;

namespace Apps.AmazonS3.Connections;

public class ConnectionDefinition : IConnectionDefinition
{
    public IEnumerable<ConnectionPropertyGroup> ConnectionPropertyGroups => new List<ConnectionPropertyGroup>
    {
        new()
        {
            Name = "Developer API key",
            AuthenticationType = ConnectionAuthenticationType.Undefined,
            ConnectionUsage = ConnectionUsage.Actions,
            ConnectionProperties = new List<ConnectionProperty>
            {
                new("access_key") { DisplayName = "Access key" },
                new("access_secret") { DisplayName = "Access secret", Sensitive = true },
                new("region") { DisplayName = "Region", }
            }
        }
    };

    public IEnumerable<AuthenticationCredentialsProvider> CreateAuthorizationCredentialsProviders(
        Dictionary<string, string> values)
    {
        var accessKey = values.First(x => x.Key == "access_key");
        var accessSecret = values.First(x => x.Key == "access_secret");
        var region = values.First(x => x.Key == "region");

        yield return new(AuthenticationCredentialsRequestLocation.None, accessKey.Key, accessKey.Value);
        yield return new(AuthenticationCredentialsRequestLocation.None, accessSecret.Key, accessSecret.Value);
        yield return new(AuthenticationCredentialsRequestLocation.None, region.Key, region.Value);
    }
}