using Apps.AmazonS3.Connections;
using Apps.AmazonS3.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Newtonsoft.Json;
using Tests.AmazonS3.Base;

namespace Tests.AmazonS3;

[TestClass]
public class DataHandlerTests : TestBase
{
    [TestMethod]
    public async Task Buckets_returns_items()
    {
        var handler = new BucketDataHandler(InvocationContext);
        var result = await handler.GetDataAsync(new DataSourceContext { }, CancellationToken.None);
        Assert.IsNotNull(result);
        foreach (var item in result)
        {
            Console.WriteLine($"{item.Value}: {item.DisplayName}");
        }
    }
}