using Amazon.S3.Model;
using Apps.AmazonS3;
using Apps.AmazonS3.Actions;
using Apps.AmazonS3.Models.Request;
using Blackbird.Applications.Sdk.Common.Invocation;
using System.Reflection;
using Tests.AmazonS3.Base;

namespace Tests.AmazonS3;

[TestClass]
public class FolderActionsTests : TestBase
{
    private readonly string TestFolderName = Guid.NewGuid().ToString() + '/';
    private readonly string TestParentFolder = Guid.NewGuid().ToString() + '/';
    private string TestFolderId => TestParentFolder + TestFolderName;

    // can't use parent method directly in DynamicData decorator as studio can't see it and shows a warning
    public static string? GetConnectionTypeName(MethodInfo method, object[]? data) => GetConnectionTypeFromDynamicData(method, data);

    [TestMethod]
    [DynamicData(nameof(InvocationContexts), DynamicDataDisplayName = nameof(GetConnectionTypeName))]
    public async Task Create_folder_works(InvocationContext context)
    {
        // Arrange
        await SetupTestFolder(context);
        var actions = new FolderActions(context);

        try
        {
            var bucketRequest = new BucketRequest { BucketName = TestBucketName };
            var folderRequest = new CreateFolderRequest
            {
                FolderName = TestFolderName,
                ParentFolderId = TestParentFolder,
            };

            // Act
            var response = await actions.CreateFolder(bucketRequest, folderRequest);

            // Assert
            PrintResult(response);
            Assert.AreEqual(TestParentFolder + TestFolderName, response.FolderId);
        }
        finally
        {
            // Cleanup
            await CleanupTestFolder(context);
        }
    }

    [TestMethod]
    [DynamicData(nameof(InvocationContexts), DynamicDataDisplayName = nameof(GetConnectionTypeName))]
    public async Task Delete_folder_works(InvocationContext context)
    {
        // Arrange
        await SetupTestFolder(context);
        var actions = new FolderActions(context);

        try
        {
            var bucketRequest = new BucketRequest { BucketName = TestBucketName };
            var folderRequest = new FolderRequest
            {
                FolderId = TestFolderId
            };

            // Act
            await actions.DeleteFolder(bucketRequest, folderRequest);
        }
        finally
        {
            // Cleanup
            await CleanupTestFolder(context);
        }
    }

    #region Helper Methods for Setup and Cleanup

    private async Task SetupTestFolder(InvocationContext context)
    {
        var s3BucketClient = await new AmazonInvocable(context)
            .CreateBucketClient(TestBucketName);

        await s3BucketClient.PutObjectAsync(new PutObjectRequest
        {
            BucketName = TestBucketName,
            Key = TestParentFolder,
            ContentBody = string.Empty
        });
    }

    private async Task CleanupTestFolder(InvocationContext context)
    {
        var s3BucketClient = await new AmazonInvocable(context)
            .CreateBucketClient(TestBucketName);

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

        await foreach (var s3Object in s3BucketClient.Paginators
            .ListObjectsV2(listRequest).S3Objects)
        {
            deleteRequest.Objects.Add(new KeyVersion { Key = s3Object.Key });
        }

        await s3BucketClient.DeleteObjectsAsync(deleteRequest);
    }

    #endregion
}

[TestClass]
public class FolderRequestTests : TestBase
{
    [TestMethod]
    public void FolderRequest_Getter_ReturnsUrlDecoded()
    {
        // Arrange
        var request = new FolderRequest
        {
            FolderId = "parent-folder%2fchild-folder%2f",
        };

        // Act
        var key = request.FolderId;

        // Assert
        Assert.AreEqual("parent-folder/child-folder/", key);
    }

    [TestMethod]
    public void FolderRequest_Getter_SupportsAsIs()
    {
        // Arrange
        var request = new FolderRequest
        {
            FolderId = "parent-folder/child-folder/",
        };

        // Act
        var key = request.FolderId;

        // Assert
        Assert.AreEqual("parent-folder/child-folder/", key);
    }
}

[TestClass]
public class CreateFolderRequestTests : TestBase
{
    [TestMethod]
    public void CreateFolderRequest_GetKey_ReturnsUrlDecoded()
    {
        // Arrange
        var request = new CreateFolderRequest
        {
            FolderName = "child-folder",
            ParentFolderId = "%2fparent-folder%2f"
        };

        // Act
        var key = request.GetKey();

        // Assert
        Assert.AreEqual("parent-folder/child-folder/", key);
    }

    [TestMethod]
    public void CreateFolderRequest_GetKey_WithOnlyFolderId_ReturnsFolderIdWithTrailingSlash()
    {
        // Arrange
        var request = new CreateFolderRequest
        {
            FolderName = "/my-folder",
            ParentFolderId = string.Empty
        };

        //Act
        var key = request.GetKey();

        // Assert
        Assert.AreEqual("my-folder/", key);
    }

    [TestMethod]
    public void CreateFolderRequest_GetKey_WithParentNull_Returns()
    {
        // Arrange
        var request = new CreateFolderRequest
        {
            FolderName = "/my-folder",
            ParentFolderId = null
        };

        //Act
        var key = request.GetKey();

        // Assert
        Assert.AreEqual("my-folder/", key);
    }

    [TestMethod]
    public void CreateFolderRequest_GetKey_WithParentFolderIdAndFolderId_ReturnsCombinedKeyWithTrailingSlash()
    {
        // Arrange
        var request = new CreateFolderRequest
        {
            FolderName = "/child-folder/",
            ParentFolderId = "/parent-folder/"
        };

        // Act
        var key = request.GetKey();

        // Assert
        Assert.AreEqual("parent-folder/child-folder/", key);
    }
}