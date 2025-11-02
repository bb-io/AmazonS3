using Apps.AmazonS3;
using Apps.AmazonS3.DataSourceHandlers;
using Apps.AmazonS3.Models.Request;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;
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
    public async Task FolderDataHandler_returns_root_items(InvocationContext context)
    {
        // Arrange
        var amazonInvokable = new AmazonInvocable(context);

        var bucket = new BucketRequest { BucketName = TestBucketName };
        bucket.ProvideConnectionType(amazonInvokable.CurrentConnectionType, amazonInvokable.ConnectedBucket);

        var handler = new FolderDataHandler(context, bucket);
        var dataSourceContext = new FolderContentDataSourceContext();

        // Act
        var result = await handler.GetFolderContentAsync(dataSourceContext, CancellationToken.None);

        // Assert
        PrintResult(result);
        Assert.IsTrue(result.Any());
    }

    [TestMethod]
    [DynamicData(nameof(InvocationContexts), DynamicDataDisplayName = nameof(GetConnectionTypeName))]
    public async Task FolderDataHandler_returns_folder_items(InvocationContext context)
    {
        // Arrange
        var amazonInvokable = new AmazonInvocable(context);

        var bucket = new BucketRequest { BucketName = TestBucketName };
        bucket.ProvideConnectionType(amazonInvokable.CurrentConnectionType, amazonInvokable.ConnectedBucket);

        var handler = new FolderDataHandler(context, bucket);
        var dataSourceContext = new FolderContentDataSourceContext { FolderId = "fol/" };

        // Act
        var result = await handler.GetFolderContentAsync(dataSourceContext, CancellationToken.None);

        // Assert
        PrintResult(result);
        Assert.IsTrue(result.Any());
    }

    [TestMethod]
    [DynamicData(nameof(InvocationContexts), DynamicDataDisplayName = nameof(GetConnectionTypeName))]
    public async Task FolderDataHandler_returns_root_path(InvocationContext context)
    {
        // Arrange
        var amazonInvokable = new AmazonInvocable(context);

        var bucket = new BucketRequest { BucketName = TestBucketName };
        bucket.ProvideConnectionType(amazonInvokable.CurrentConnectionType, amazonInvokable.ConnectedBucket);

        var handler = new FolderDataHandler(context, bucket);
        var dataSourceContext = new FolderPathDataSourceContext();

        // Act
        var result = await handler.GetFolderPathAsync(dataSourceContext, CancellationToken.None);

        // Assert
        PrintResult(result);
        Assert.AreEqual(1, result.Count());
        Assert.AreEqual(string.Empty, result.First().Id);
        Assert.AreEqual(bucket.BucketName, result.First().DisplayName);
    }

    [TestMethod]
    [DynamicData(nameof(InvocationContexts), DynamicDataDisplayName = nameof(GetConnectionTypeName))]
    public async Task FolderDataHandler_returns_folder_path(InvocationContext context)
    {
        // Arrange
        var amazonInvokable = new AmazonInvocable(context);

        var bucket = new BucketRequest { BucketName = TestBucketName };
        bucket.ProvideConnectionType(amazonInvokable.CurrentConnectionType, amazonInvokable.ConnectedBucket);

        var handler = new FolderDataHandler(context, bucket);
        var dataSourceContext = new FolderPathDataSourceContext { FileDataItemId = "fol/manually-tested-folder/" };

        // Act
        var result = await handler.GetFolderPathAsync(dataSourceContext, CancellationToken.None);

        // Assert
        PrintResult(result);
        Assert.AreEqual(3, result.Count());

        Assert.AreEqual(string.Empty, result.First().Id);
        Assert.AreEqual(bucket.BucketName, result.First().DisplayName);

        Assert.AreEqual("fol/", result.ElementAt(1).Id);
        Assert.AreEqual("fol", result.ElementAt(1).DisplayName);

        Assert.AreEqual("fol/manually-tested-folder/", result.ElementAt(2).Id);
        Assert.AreEqual("manually-tested-folder", result.ElementAt(2).DisplayName);
    }
}