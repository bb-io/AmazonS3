using Apps.AmazonS3.Actions;
using Apps.AmazonS3.DataSourceHandlers;
using Apps.AmazonS3.Models.Request.Base;
using Blackbird.Applications.Sdk.Common.Dynamic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests.AmazonS3.Base;

namespace Tests.AmazonS3;

[TestClass]
public class BucketTests : TestBase
{
    public const string BucketName = "blackbird-test-bucket-name";

    [TestMethod]
    public async Task Create_and_delete_bucket_works()
    {
        var actions = new BucketActions(InvocationContext);
        var handler = new BucketDataHandler(InvocationContext);

        await actions.CreateBucket(BucketName);        
        var result = await handler.GetDataAsync(new DataSourceContext { }, CancellationToken.None);
        Assert.IsTrue(result.Any(x => x.Value == BucketName));

        await actions.DeleteBucket(new BucketRequestModel { BucketName = BucketName });
        result = await handler.GetDataAsync(new DataSourceContext { }, CancellationToken.None);
        Assert.IsTrue(result.All(x => x.Value != BucketName));
    }
}