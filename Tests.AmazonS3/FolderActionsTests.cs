using Amazon.S3.Model;
using Apps.AmazonS3;
using Apps.AmazonS3.Actions;
using Apps.AmazonS3.Models.Request;
using Tests.AmazonS3.Base;

namespace Tests.AmazonS3;

[TestClass]
public class FolderActionsTests : TestBase
{
    private readonly FolderActions Actions;

    private readonly string TestFolderName = Guid.NewGuid().ToString() + '/';
    private readonly string TestParentFolder = Guid.NewGuid().ToString() + '/';

    public FolderActionsTests()
    {
        Actions = new FolderActions(InvocationContext);
    }

    [TestInitialize]
    public async Task TestInitialize()
    {
        var s3BucketClient = await new AmazonInvocable(InvocationContext).CreateBucketClient(TestBucketName);

        await s3BucketClient.PutObjectAsync(new PutObjectRequest
        {
            BucketName = TestBucketName,
            Key = TestParentFolder,
            ContentBody = string.Empty
        });
    }

    [TestCleanup]
    public async Task TestCleanup()
    {
        var s3BucketClient = await new AmazonInvocable(InvocationContext).CreateBucketClient(TestBucketName);

        var deleteRequest = new DeleteObjectsRequest
        {
            BucketName = TestBucketName,
            Objects = []
        };

        var listRequest = new ListObjectsV2Request
        {
            BucketName = TestBucketName,
            Prefix = TestParentFolder
        };

        await foreach (var s3Object in s3BucketClient.Paginators.ListObjectsV2(listRequest).S3Objects)
        {
            deleteRequest.Objects.Add(new KeyVersion { Key = s3Object.Key });
        }

        await s3BucketClient.DeleteObjectsAsync(deleteRequest);
    }

    [TestMethod]
    public async Task Create_folder_works()
    {
        var request = new FolderRequest
        {
            BucketName = TestBucketName,
            FolderId = TestFolderName,
            ParentFolderId = TestParentFolder,
        };

        var response = await Actions.CreateFolder(request);

        Assert.IsNotNull(response);
    }

    [TestMethod]
    public async Task Delete_folder_works()
    {
        var request = new FolderRequest
        {
            BucketName = TestBucketName,
            FolderId = TestFolderName,
            ParentFolderId = TestParentFolder
        };

        await Actions.DeleteFolder(request);
    }
}

[TestClass]
public class FolderRequestTests
{
    [TestMethod]
    public void FolderRequest_GetKey_WithOnlyFolderId_ReturnsFolderIdWithTrailingSlash()
    {
        var request = new FolderRequest
        {
            FolderId = "/my-folder",
            ParentFolderId = string.Empty
        };

        var key = request.GetKey();

        Assert.AreEqual("my-folder/", key);
    }

    [TestMethod]
    public void FolderRequest_GetKey_WithParentFolderIdAndFolderId_ReturnsCombinedKeyWithTrailingSlash()
    {
        var request = new FolderRequest
        {
            FolderId = "/child-folder/",
            ParentFolderId = "/parent-folder/"
        };

        var key = request.GetKey();

        Assert.AreEqual("parent-folder/child-folder/", key);
    }
}