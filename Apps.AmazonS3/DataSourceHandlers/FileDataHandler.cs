using Apps.AmazonS3.Models.Request;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;
using File = Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems.File;

namespace Apps.AmazonS3.DataSourceHandlers;

public class FileDataHandler : AmazonInvocable, IAsyncFileDataSourceItemHandler
{
    private readonly string _bucketName;
    private readonly FolderDataHandler _folderDataHandler;

    public FileDataHandler(
        InvocationContext invocationContext,
        [ActionParameter] BucketRequest bucketRequest)
        : base(invocationContext)
    {
        bucketRequest.ProvideConnectionType(CurrentConnectionType, ConnectedBucket);

        if (string.IsNullOrWhiteSpace(bucketRequest.BucketName))
            throw new("You should select a bucket in an input.");

        _bucketName = bucketRequest.BucketName;
        _folderDataHandler = new FolderDataHandler(invocationContext, bucketRequest);
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
            var objectWithoutPrefix = !string.IsNullOrEmpty(currentFolder)
                ? s3Object.Key.Replace(currentFolder, string.Empty)
                : s3Object.Key;
            var objectName = objectWithoutPrefix.TrimEnd('/');

            // filter out nested objects and a current folder
            if (objectName.Contains('/') || string.IsNullOrEmpty(objectName))
                continue;

            FileDataItem dataItem = (!s3Object.Key.EndsWith('/') || s3Object.Size != 0)
                ? new File { IsSelectable = true, Size = s3Object.Size, Id = s3Object.Key, DisplayName = objectName, Date = s3Object.LastModified }
                : new Folder { IsSelectable = false, Id = s3Object.Key, DisplayName = objectName, Date = s3Object.LastModified };

            content.Add(dataItem);

            if (cancellationToken.IsCancellationRequested)
                break;
        }

        return content;
    }

    public Task<IEnumerable<FolderPathItem>> GetFolderPathAsync(FolderPathDataSourceContext context, CancellationToken _)
    {
        return _folderDataHandler.GetFolderPathAsync(context, CancellationToken.None);
    }
}