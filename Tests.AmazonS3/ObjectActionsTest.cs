using Apps.AmazonS3.Actions;
using Apps.AmazonS3.Models.Request;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.Sdk.Common.Invocation;
using System.Reflection;
using Tests.AmazonS3.Base;

namespace Tests.AmazonS3;

[TestClass]
public class ObjectActionsTest : TestBase
{
    public const string TestFileName = "simple-test.txt";

    // can't use parent method directly in DynamicData decorator as studio can't see it and shows a warning
    public static string? GetConnectionTypeName(MethodInfo method, object[]? data) => GetConnectionTypeFromDynamicData(method, data);

    [TestMethod]
    [DynamicData(nameof(InvocationContexts), DynamicDataDisplayName = nameof(GetConnectionTypeName))]
    public async Task Search_objects_in_bucket_works(InvocationContext context)
    {
        // Arrange
        var actions = new ObjectActions(context, FileManager);
        var bucket = new BucketRequest { BucketName = TestBucketName };
        var search = new SearchFilesRequest { };

        // Act
        var result = await actions.ListObjectsInBucket(bucket, search);

        // Assert
        PrintResult(result);
        Assert.IsTrue(result.Files.Any());
    }

    [TestMethod]
    [DynamicData(nameof(InvocationContexts), DynamicDataDisplayName = nameof(GetConnectionTypeName))]
    public async Task Search_objects_with_prefix_works(InvocationContext context)
    {
        // Arrange
        var actions = new ObjectActions(context, FileManager);
        var bucket = new BucketRequest { BucketName = TestBucketName };
        var search = new SearchFilesRequest { FolderId = "fol/" };

        // Act
        var result = await actions.ListObjectsInBucket(bucket, search);

        // Assert
        PrintResult(result);
        Assert.IsTrue(result.Files.Any());
    }

    [TestMethod]
    [DynamicData(nameof(InvocationContexts), DynamicDataDisplayName = nameof(GetConnectionTypeName))]
    public async Task Upload_object_works(InvocationContext context)
    {
        // Arrange
        var actions = new ObjectActions(context, FileManager);
        var bucket = new BucketRequest { BucketName = TestBucketName };
        var upload = new UploadFileRequest { File = new FileReference { Name = TestFileName } };

        // Act
        await actions.UploadObject(bucket, upload);

        // Assert
        var check = await actions.ListObjectsInBucket(bucket, new());
        Assert.IsTrue(check.Files.Any(x => x.FileId == TestFileName));

        // Clean up
        await actions.DeleteObject(bucket, new() { FileId = TestFileName });
    }

    [TestMethod]
    [DynamicData(nameof(InvocationContexts), DynamicDataDisplayName = nameof(GetConnectionTypeName))]
    public async Task Get_object_works(InvocationContext context)
    {
        // Arrange
        var actions = new ObjectActions(context, FileManager);
        var bucket = new BucketRequest { BucketName = TestBucketName };
        var fileRequest = new FileRequest { FileId = "fol/maryland.tbx" };

        // Act
        var result = await actions.GetObject(bucket, fileRequest);

        // Assert
        PrintResult(result);
        Assert.AreEqual("maryland.tbx", result.File.Name);
    }

    [TestMethod]
    [DynamicData(nameof(InvocationContexts), DynamicDataDisplayName = nameof(GetConnectionTypeName))]
    public async Task Delete_object_works_upload_first(InvocationContext context)
    {
        // Arrange
        var fileName = "simple-test.txt";
        var actions = new ObjectActions(context, FileManager);
        var bucket = new BucketRequest { BucketName = TestBucketName };

        var upload = new UploadFileRequest { File = new FileReference { Name = fileName } };
        await actions.UploadObject(bucket, upload);
        var before = await actions.ListObjectsInBucket(bucket, new());
        Assert.IsTrue(before.Files.Any(x => x.FileId == fileName));

        // Act
        await actions.DeleteObject(bucket, new() { FileId = fileName });

        // Assert
        var after = await actions.ListObjectsInBucket(bucket, new());
        PrintResult(after);
        Assert.IsFalse(after.Files.Any(x => x.FileId == fileName));
    }
}