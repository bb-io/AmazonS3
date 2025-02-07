using Amazon;
using Apps.AmazonS3.Constants;
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
            ConnectionProperties = new List<ConnectionProperty>
            {
                new(CredNames.AccessKey) { DisplayName = "Access key" },
                new(CredNames.AccessSecret) { DisplayName = "Access secret", Sensitive = true },
                new(CredNames.Region) { DisplayName = "Region", DataItems = RegionEndpoint.EnumerableAllRegions.Select(x => new ConnectionPropertyValue(x.SystemName, x.SystemName))  }
            }
        }
    };

    public IEnumerable<AuthenticationCredentialsProvider> CreateAuthorizationCredentialsProviders(Dictionary<string, string> values)
    {
        return values.Select(x => new AuthenticationCredentialsProvider(x.Key, x.Value)).ToList();
    }
}