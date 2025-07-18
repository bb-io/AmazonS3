using Apps.AmazonS3.DataSourceHandlers;
using Apps.AmazonS3.Polling.Models;
using Blackbird.Applications.Sdk.Common.Dynamic;
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

    [TestMethod]
    public async Task FolderDataHandler_returns_items()
    {
        var pollingFolderRequest = new PollingFolderRequest { BucketName = "myuniquebucketfortesting" };
        var handler = new FolderDataHandler(InvocationContext, pollingFolderRequest);

        var result = await handler.GetDataAsync(new DataSourceContext { }, CancellationToken.None);

        Assert.IsNotNull(result);
        foreach (var item in result)
        {
            Console.WriteLine($"{item.Value}: {item.DisplayName}");
        }
    }
}