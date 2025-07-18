using Apps.AmazonS3.Actions;
using Apps.AmazonS3.Models.Request;
using Apps.AmazonS3.Models.Request.Base;
using Blackbird.Applications.Sdk.Common.Files;
using Tests.AmazonS3.Base;

namespace Tests.AmazonS3;

[TestClass]
public class ObjectTests : TestBase
{
    public const string TestFileName = "simple-test.txt";

    [TestMethod]
    public async Task Search_objects_in_bucket_works()
    {
        var actions = new ObjectActions(InvocationContext, FileManager);

        var result = await actions.ListObjectsInBucket(
            new BucketRequestModel { BucketName = TestBucketName },
            new SearchFilesRequest { });

        Assert.IsNotNull(result);
        foreach (var item in result.Objects)
        {
            Console.WriteLine(item.Key);
        }
    }

    [TestMethod]
    public async Task Search_objects_with_prefix_works()
    {
        var actions = new ObjectActions(InvocationContext, FileManager);

        var result = await actions.ListObjectsInBucket(
            new BucketRequestModel { BucketName = TestBucketName },
            new SearchFilesRequest { FolderID = "fol/" });

        Assert.IsNotNull(result);
        foreach (var item in result.Objects)
        {
            Console.WriteLine(item.Key);
        }
    }

    [TestMethod]
    public async Task Upload_object_works()
    {
        var actions = new ObjectActions(InvocationContext, FileManager);
        await actions.UploadObject(new UploadFileInput
        {
            BucketName = TestBucketName,
            File = new FileReference { Name = TestFileName }
        });

        var result = await actions.ListObjectsInBucket(
            new BucketRequestModel { BucketName = TestBucketName },
            new SearchFilesRequest { });

        Assert.IsTrue(result.Objects.Any(x => x.Key == TestFileName));
    }
}