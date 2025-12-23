using Amazon;
using Apps.AmazonS3.Constants;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;

namespace Apps.AmazonS3.Connections;

public class ConnectionDefinition : IConnectionDefinition
{
    public IEnumerable<ConnectionPropertyGroup> ConnectionPropertyGroups =>
    [
        new()
        {
            Name = ConnectionTypes.SingleBucket,
            DisplayName = "Single bucket",
            AuthenticationType = ConnectionAuthenticationType.Undefined,
            ConnectionProperties =
            [
                new(CredNames.AccessKey) { DisplayName = "Access key" },
                new(CredNames.AccessSecret) { DisplayName = "Access secret", Sensitive = true },
                new(CredNames.Region) { DisplayName = "Region", DataItems = RegionEndpoint.EnumerableAllRegions.Select(x => new ConnectionPropertyValue(x.SystemName, x.SystemName))  },
                new(CredNames.Bucket) { DisplayName = "Bucket" },
            ]
        },
        new()
        {
            Name = ConnectionTypes.AllBuckets,
            DisplayName = "All buckets",
            AuthenticationType = ConnectionAuthenticationType.Undefined,
            ConnectionProperties =
            [
                new(CredNames.AccessKey) { DisplayName = "Access key" },
                new(CredNames.AccessSecret) { DisplayName = "Access secret", Sensitive = true },
                new(CredNames.Region) { DisplayName = "Region", DataItems = RegionEndpoint.EnumerableAllRegions.Select(x => new ConnectionPropertyValue(x.SystemName, x.SystemName))  }
            ]
        },
        new()
        {
            Name = ConnectionTypes.AssumeRole,
            DisplayName = "Assume role",
            AuthenticationType = ConnectionAuthenticationType.Undefined,
            ConnectionProperties =
            [
                new(CredNames.AccessKey) { DisplayName = "Source access key" },
                new(CredNames.AccessSecret) { DisplayName = "Source access secret", Sensitive = true },
                new(CredNames.Region) { DisplayName = "Region", DataItems = RegionEndpoint.EnumerableAllRegions.Select(x => new ConnectionPropertyValue(x.SystemName, x.SystemName))  },
                new(CredNames.RoleArn) { DisplayName = "Role ARN" },
                new(CredNames.ExternalId) { DisplayName = "External ID", Sensitive = true }
            ]
        }
    ];

    public IEnumerable<AuthenticationCredentialsProvider> CreateAuthorizationCredentialsProviders(Dictionary<string, string> values)
    {
        var credentials = values.Select(x => new AuthenticationCredentialsProvider(x.Key, x.Value)).ToList();
        var connectionType = values[nameof(ConnectionPropertyGroup)] switch
        {
            var ct when ConnectionTypes.SupportedConnectionTypes.Contains(ct) => ct,
            _ => throw new Exception($"Unknown connection type: {values[nameof(ConnectionPropertyGroup)]}")
        };
        credentials.Add(new AuthenticationCredentialsProvider(CredNames.ConnectionType, connectionType));

        return credentials;
    }
}