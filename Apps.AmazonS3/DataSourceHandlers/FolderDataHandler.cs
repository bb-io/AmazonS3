using Apps.AmazonS3.Models.Request;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;
using System.Web;

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
            throw new("You should select a bucket in an input.");

        _bucketName = bucketRequest.BucketName;
    }

    public async Task<IEnumerable<FileDataItem>> GetFolderContentAsync(
        FolderContentDataSourceContext context, CancellationToken cancellationToken)
    {
        var currentFolder = context.FolderId == "root" || string.IsNullOrEmpty(context.FolderId)
            ? string.Empty
            : context.FolderId;

        var client = await CreateBucketClient(_bucketName);
        var objects = client.Paginators.ListObjectsV2(new()
        {
            BucketName = _bucketName,
            Prefix = currentFolder,
            // we can't use Delimiter as it won't return folders to us, only files
        });

        var content = new List<FileDataItem>();

        await foreach (var s3Object in objects.S3Objects)
        {
            // filter out files
            if (!s3Object.Key.EndsWith('/') || s3Object.Size != 0)
                continue;

            var folderWithoutPrefix = !string.IsNullOrEmpty(currentFolder)
                ? s3Object.Key.Replace(currentFolder, string.Empty)
                : s3Object.Key;
            var folderName = folderWithoutPrefix.TrimEnd('/');

            // filter out nested folders and a current folder
            if (folderName.Contains('/') || string.IsNullOrEmpty(folderName))
                continue;

            content.Add(new Folder()
            {
                Id = s3Object.Key,
                DisplayName = folderName,
                Date = s3Object.LastModified,
                IsSelectable = true,
            });

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