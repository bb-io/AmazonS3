using System.Net;
using Apps.AmazonS3.Constants;
using Apps.AmazonS3.DataSourceHandlers;
using Apps.AmazonS3.Models.Request;
using Apps.AmazonS3.Polling;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Polling;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;
using Tests.AmazonS3.Base;

namespace Tests.AmazonS3;

[TestClass]
public class S3ErrorResponseTests
{
    private const string AccessDeniedMessage = "Access Denied";

    private S3ErrorStubServer _server = null!;

    [TestInitialize]
    public void StartServer()
    {
        _server = new S3ErrorStubServer(HttpStatusCode.Forbidden, "AccessDenied", AccessDeniedMessage);
    }

    [TestCleanup]
    public void StopServer()
    {
        _server.Dispose();
    }

    [TestMethod]
    public async Task OnFilesUpdated_ThrowsPluginApplicationExceptionWithS3Message_WhenListingIsDenied()
    {
        var polling = new PollingList(_server.CreateS3CompatibleInvocationContext());
        var pollingRequest = new PollingEventRequest<DateMemory> { Memory = new() { LastInteractionDate = DateTime.UtcNow } };

        var exception = await Assert.ThrowsExactlyAsync<PluginApplicationException>(() =>
            polling.OnFilesUpdated(pollingRequest, CreateBucketRequest(), new FolderRequest(), null));

        StringAssert.Contains(exception.Message, AccessDeniedMessage);
    }

    [TestMethod]
    public async Task FolderDataHandler_ThrowsPluginApplicationExceptionWithS3Message_WhenListingIsDenied()
    {
        var handler = new FolderDataHandler(_server.CreateS3CompatibleInvocationContext(), CreateBucketRequest());

        var exception = await Assert.ThrowsExactlyAsync<PluginApplicationException>(() =>
            handler.GetFolderContentAsync(new FolderContentDataSourceContext { FolderId = "root" }, CancellationToken.None));

        StringAssert.Contains(exception.Message, AccessDeniedMessage);
    }

    [TestMethod]
    public async Task FileDataHandler_ThrowsPluginApplicationExceptionWithS3Message_WhenListingIsDenied()
    {
        var handler = new FileDataHandler(_server.CreateS3CompatibleInvocationContext(), CreateBucketRequest());

        var exception = await Assert.ThrowsExactlyAsync<PluginApplicationException>(() =>
            handler.GetFolderContentAsync(new FolderContentDataSourceContext { FolderId = "root" }, CancellationToken.None));

        StringAssert.Contains(exception.Message, AccessDeniedMessage);
    }

    private static BucketRequest CreateBucketRequest()
    {
        var bucketRequest = new BucketRequest { BucketName = "bucket-1" };
        bucketRequest.ProvideConnectionType(ConnectionTypes.S3Compatible, string.Empty);
        return bucketRequest;
    }
}
