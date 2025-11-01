using Apps.AmazonS3;
using Apps.AmazonS3.DataSourceHandlers;
using Apps.AmazonS3.Models.Request;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using System.Reflection;
using Tests.AmazonS3.Base;

namespace Tests.AmazonS3;

[TestClass]
public class DataHandlerTests : TestBase
{
    // can't use parent method directly in DynamicData decorator as studio can't see it and shows a warning
    public static string? GetConnectionTypeName(MethodInfo method, object[]? data) => GetConnectionTypeFromDynamicData(method, data);

    [TestMethod]
    [DynamicData(nameof(AllBucketInvocationContexts), DynamicDataDisplayName = nameof(GetConnectionTypeName))]
    public async Task Buckets_WithMultipBucketConnection_returns_items(InvocationContext context)
    {
        // Arrange
        var handler = new BucketDataHandler(context);
        var dataSourceContext = new DataSourceContext();

        // Act
        var result = await handler.GetDataAsync(dataSourceContext, CancellationToken.None);

        // Assert
        PrintResult(result);
        Assert.IsTrue(result.Any());
    }

    [TestMethod]
    [DynamicData(nameof(SingleBucketInvocationContexts), DynamicDataDisplayName = nameof(GetConnectionTypeName))]
    public async Task Buckets_WithSingleBucketConnection_throws(InvocationContext context)
    {
        // Arrange
        var handler = new BucketDataHandler(context);
        var dataSourceContext = new DataSourceContext();

        // Act & Assert
        await Assert.ThrowsExactlyAsync<PluginMisconfigurationException>(async () 
            => await handler.GetDataAsync(dataSourceContext, CancellationToken.None));
    }

    [TestMethod]
    [DynamicData(nameof(InvocationContexts), DynamicDataDisplayName = nameof(GetConnectionTypeName))]
    public async Task FolderDataHandler_returns_items(InvocationContext context)
    {
        // Arrange
        var amazonInvokable = new AmazonInvocable(context);

        var bucket = new BucketRequest { BucketName = TestBucketName };
        bucket.ProvideConnectionType(amazonInvokable.CurrentConnectionType, amazonInvokable.ConnectedBucket);

        var handler = new FolderDataHandler(context, bucket);
        var dataSourceContext = new DataSourceContext();

        // Act
        var result = await handler.GetDataAsync(dataSourceContext, CancellationToken.None);

        // Assert
        PrintResult(result);
        Assert.IsTrue(result.Any());
    }
}