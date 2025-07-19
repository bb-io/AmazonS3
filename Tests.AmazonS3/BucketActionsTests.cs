using Apps.AmazonS3.Actions;
using Apps.AmazonS3.DataSourceHandlers;
using Apps.AmazonS3.Models.Request;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Tests.AmazonS3.Base;

namespace Tests.AmazonS3;

[TestClass]
public class BucketActionsTests : TestBase
{
    public string BucketName = Guid.NewGuid().ToString();

    private readonly BucketActions Actions;

    public BucketActionsTests()
    {
        Actions = new BucketActions(InvocationContext);
    }

    [TestMethod]
    public async Task Create_and_delete_bucket_works()
    {
        var handler = new BucketDataHandler(InvocationContext);

        await Actions.CreateBucket(BucketName);        
        var result = await handler.GetDataAsync(new DataSourceContext { }, CancellationToken.None);
        Assert.IsTrue(result.Any(x => x.Value == BucketName));

        await Actions.DeleteBucket(new BucketRequest { BucketName = BucketName });
        result = await handler.GetDataAsync(new DataSourceContext { }, CancellationToken.None);
        Assert.IsTrue(result.All(x => x.Value != BucketName));
    }
}