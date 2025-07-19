using Apps.AmazonS3.Actions;
using Apps.AmazonS3.Models.Request;
using Blackbird.Applications.Sdk.Common.Files;
using Tests.AmazonS3.Base;

namespace Tests.AmazonS3;

[TestClass]
public class ObjectActionsTest : TestBase
{
    public const string TestFileName = "simple-test.txt";

    private readonly ObjectActions Actions;

    public ObjectActionsTest()
    {
        Actions = new ObjectActions(InvocationContext, FileManager);
    }

    [TestMethod]
    public async Task Search_objects_in_bucket_works()
    {
        var result = await Actions.ListObjectsInBucket(
            new BucketRequest { BucketName = TestBucketName },
            new SearchFilesRequest { });

        Assert.IsNotNull(result);
        foreach (var item in result.Files)
        {
            Console.WriteLine(item.FileId);
        }
    }

    [TestMethod]
    public async Task Search_objects_with_prefix_works()
    {
        var result = await Actions.ListObjectsInBucket(
            new BucketRequest { BucketName = TestBucketName },
            new SearchFilesRequest { FolderID = "fol/" });

        Assert.IsNotNull(result);
        foreach (var item in result.Files)
        {
            Console.WriteLine(item.FileId);
        }
    }

    [TestMethod]
    public async Task Upload_object_works()
    {
        await Actions.UploadObject(
            new BucketRequest { BucketName = TestBucketName },
            new UploadFileRequest { File = new FileReference { Name = TestFileName } });

        var result = await Actions.ListObjectsInBucket(
            new BucketRequest { BucketName = TestBucketName },
            new SearchFilesRequest { });

        Assert.IsTrue(result.Files.Any(x => x.FileId == TestFileName));
    }
}