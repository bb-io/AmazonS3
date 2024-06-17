using Amazon.S3.Model;
using Apps.AmazonS3.Factories;
using Apps.AmazonS3.Models.Request;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.AmazonS3.DataSourceHandlers;

public class FolderDataHandler : BaseInvocable, IAsyncDataSourceHandler
{
    private readonly FolderRequest _folderRequest;

    private IEnumerable<AuthenticationCredentialsProvider> Creds =>
        InvocationContext.AuthenticationCredentialsProviders;

    public FolderDataHandler(InvocationContext invocationContext, [ActionParameter] FolderRequest folderRequest) : base(
        invocationContext)
    {
        _folderRequest = folderRequest;
    }

    public async Task<Dictionary<string, string>> GetDataAsync(
        DataSourceContext context,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_folderRequest.BucketName))
            throw new("You should input Bucket name first");

        var client = await AmazonClientFactory.CreateS3BucketClient(Creds.ToArray(), _folderRequest.BucketName);

        var objects = client.Paginators.ListObjectsV2(new()
        {
            BucketName = _folderRequest.BucketName
        });
        var result = new List<S3Object>();

        await foreach (var s3Object in objects.S3Objects)
            result.Add(s3Object);

        return result
            .Where(x => x.Key.EndsWith("/") && x.Size == default)
            .Where(x => context.SearchString == null ||
                        x.Key.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .Take(30)
            .Select(x => new KeyValuePair<string, string>(x.Key, x.Key))
            .Prepend(new("/", "All files"))
            .ToDictionary(x => x.Key, x => x.Value);
    }
}