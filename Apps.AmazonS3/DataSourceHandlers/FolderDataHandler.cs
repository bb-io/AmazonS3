using Apps.AmazonS3.Models.Request;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;

namespace Apps.AmazonS3.DataSourceHandlers;

public class FolderDataHandler : AmazonInvocable, IAsyncFileDataSourceItemHandler
{
    private readonly string _bucketName;

    public FolderDataHandler(
        InvocationContext invocationContext,
        [ActionParameter] BucketRequest bucketRequest)
        : base(invocationContext)
    {
        bucketRequest.ProvideConnectionType(CurrentConnectionType, ConnectedBucket);

        if (string.IsNullOrWhiteSpace(bucketRequest.BucketName))
            throw new PluginMisconfigurationException("You should select a bucket in an input.");

        _bucketName = bucketRequest.BucketName;
    }

    public async Task<IEnumerable<FileDataItem>> GetFolderContentAsync(
        FolderContentDataSourceContext context, CancellationToken cancellationToken)
    {
        var currentFolder = context.FolderId == "root" || string.IsNullOrEmpty(context.FolderId)
            ? string.Empty
            : context.FolderId; 
        
        if (!string.IsNullOrEmpty(currentFolder) && !currentFolder.EndsWith('/'))
            currentFolder += "/";

        var client = await CreateBucketClient(_bucketName);
        var paginator = client.Paginators.ListObjectsV2(new()
        {
            BucketName = _bucketName,
            Prefix = currentFolder,
            Delimiter = "/"
        });

        var content = new List<FileDataItem>();

        await foreach (var response in paginator.Responses)
        {
            if (response.CommonPrefixes == null)
                continue;

            foreach (var prefix in response.CommonPrefixes)
            {
                var folderName = string.IsNullOrEmpty(currentFolder)
                    ? prefix.TrimEnd('/')
                    : prefix.Substring(currentFolder.Length).TrimEnd('/');

                if (string.IsNullOrEmpty(folderName))
                    continue;

                content.Add(new Folder()
                {
                    Id = prefix,
                    DisplayName = folderName,
                    IsSelectable = true,
                });
            }

            if (cancellationToken.IsCancellationRequested)
                break;
        }

        return content;
    }

    public Task<IEnumerable<FolderPathItem>> GetFolderPathAsync(FolderPathDataSourceContext context, CancellationToken _)
    {
        var path = new List<FolderPathItem>()
        {
            new() { DisplayName = _bucketName, Id = "root" }
        };

        if (string.IsNullOrEmpty(context?.FileDataItemId))
            return Task.FromResult<IEnumerable<FolderPathItem>>(path);

        var folders = context.FileDataItemId.TrimEnd('/').Split('/');
        if (folders.Length == 0)
        {
            path.Add(new() { DisplayName = context.FileDataItemId, Id = context.FileDataItemId });
            return Task.FromResult<IEnumerable<FolderPathItem>>(path);
        }

        var currentPath = "";

        foreach (var folder in folders)
        {
            currentPath += folder + "/";
            path.Add(new()
            {
                DisplayName = folder,
                Id = currentPath,
            });
        }

        return Task.FromResult<IEnumerable<FolderPathItem>>(path);
    }
}