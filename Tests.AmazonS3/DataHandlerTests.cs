using Apps.AmazonS3;
using Apps.AmazonS3.DataSourceHandlers;
using Apps.AmazonS3.Models.Request;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;
using System.Reflection;
using System.Text.Json;
using Tests.AmazonS3.Base;
using File = Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems.File;

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
    public async Task FolderDataHandler_returns_root_items_from_null(InvocationContext context)
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
        Assert.AreEqual(1, result.Count());
        Assert.AreEqual("fol/", result.First().Id);
        Assert.AreEqual("fol", result.First().DisplayName);
    }

    [TestMethod]
    [DynamicData(nameof(InvocationContexts), DynamicDataDisplayName = nameof(GetConnectionTypeName))]
    public async Task FolderDataHandler_returns_root_items_from_root_id(InvocationContext context)
    {
        // Arrange
        var amazonInvokable = new AmazonInvocable(context);

        var bucket = new BucketRequest { BucketName = TestBucketName };
        bucket.ProvideConnectionType(amazonInvokable.CurrentConnectionType, amazonInvokable.ConnectedBucket);

        var handler = new FolderDataHandler(context, bucket);
        var dataSourceContext = new FolderContentDataSourceContext { FolderId = "root" };

        // Act
        var result = await handler.GetFolderContentAsync(dataSourceContext, CancellationToken.None);

        // Assert
        PrintResult(result);
        Assert.AreEqual(1, result.Count());
        Assert.AreEqual("fol/", result.First().Id);
        Assert.AreEqual("fol", result.First().DisplayName);
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
        Assert.AreEqual(1, result.Count());
        Assert.AreEqual("fol/manually-tested-folder/", result.First().Id);
        Assert.AreEqual("manually-tested-folder", result.First().DisplayName);
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
        Assert.AreEqual("root", result.First().Id);
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
        var dataSourceContext = new FolderPathDataSourceContext
        {
            FileDataItemId = "fol/manually-tested-folder/" // tests both direct and URL-encoded inputs
        }; 

        // Act
        var result = await handler.GetFolderPathAsync(dataSourceContext, CancellationToken.None);

        // Assert
        PrintResult(result);
        Assert.AreEqual(3, result.Count());

        Assert.AreEqual("root", result.First().Id);
        Assert.AreEqual(bucket.BucketName, result.First().DisplayName);

        Assert.AreEqual("fol/", result.ElementAt(1).Id);
        Assert.AreEqual("fol", result.ElementAt(1).DisplayName);

        Assert.AreEqual("fol/manually-tested-folder/", result.ElementAt(2).Id);
        Assert.AreEqual("manually-tested-folder", result.ElementAt(2).DisplayName);
    }

    [TestMethod]
    [DynamicData(nameof(InvocationContexts), DynamicDataDisplayName = nameof(GetConnectionTypeName))]
    public async Task FileDataHandler_returns_root_items_from_null(InvocationContext context)
    {
        // Arrange
        var amazonInvokable = new AmazonInvocable(context);

        var bucket = new BucketRequest { BucketName = TestBucketName };
        bucket.ProvideConnectionType(amazonInvokable.CurrentConnectionType, amazonInvokable.ConnectedBucket);

        var handler = new FileDataHandler(context, bucket);
        var dataSourceContext = new FolderContentDataSourceContext();

        // Act
        var result = await handler.GetFolderContentAsync(dataSourceContext, CancellationToken.None);

        // Assert
        PrintResult(result);
        Assert.AreEqual(2, result.Count());

        var file = result.ElementAt(0) as File ?? new();
        Assert.AreEqual("basic-min.tbx", file.Id);
        Assert.AreEqual("basic-min.tbx", file.DisplayName);
        Assert.AreEqual(1807, file.Size);
        Assert.IsTrue(file.IsSelectable);
        Assert.AreEqual((int)FileDataItemType.File, file.Type);

        Assert.AreEqual("fol/", result.ElementAt(1).Id);
        Assert.AreEqual("fol", result.ElementAt(1).DisplayName);
        Assert.IsFalse(result.ElementAt(1).IsSelectable);
        Assert.AreEqual((int)FileDataItemType.Folder, result.ElementAt(1).Type);
    }

    [TestMethod]
    [DynamicData(nameof(InvocationContexts), DynamicDataDisplayName = nameof(GetConnectionTypeName))]
    public async Task FileDataHandler_returns_root_items_from_root_id(InvocationContext context)
    {
        // Arrange
        var amazonInvokable = new AmazonInvocable(context);

        var bucket = new BucketRequest { BucketName = TestBucketName };
        bucket.ProvideConnectionType(amazonInvokable.CurrentConnectionType, amazonInvokable.ConnectedBucket);

        var handler = new FileDataHandler(context, bucket);
        var dataSourceContext = new FolderContentDataSourceContext { FolderId = "root" };

        // Act
        var result = await handler.GetFolderContentAsync(dataSourceContext, CancellationToken.None);

        // Assert
        PrintResult(result);
        Assert.AreEqual(2, result.Count());

        Assert.AreEqual("basic-min.tbx", result.ElementAt(0).Id);
        Assert.AreEqual("basic-min.tbx", result.ElementAt(0).DisplayName);
        Assert.AreEqual((int)FileDataItemType.File, result.ElementAt(0).Type);

        Assert.AreEqual("fol/", result.ElementAt(1).Id);
        Assert.AreEqual("fol", result.ElementAt(1).DisplayName);
        Assert.AreEqual((int)FileDataItemType.Folder, result.ElementAt(1).Type);
    }

    [TestMethod]
    [DynamicData(nameof(InvocationContexts), DynamicDataDisplayName = nameof(GetConnectionTypeName))]
    public async Task FileDataHandler_returns_folder_items(InvocationContext context)
    {
        // Arrange
        var amazonInvokable = new AmazonInvocable(context);

        var bucket = new BucketRequest { BucketName = TestBucketName };
        bucket.ProvideConnectionType(amazonInvokable.CurrentConnectionType, amazonInvokable.ConnectedBucket);

        var handler = new FileDataHandler(context, bucket);
        var dataSourceContext = new FolderContentDataSourceContext { FolderId = "fol/" };

        // Act
        var result = await handler.GetFolderContentAsync(dataSourceContext, CancellationToken.None);

        // Assert
        PrintResult(result);
        Assert.AreEqual(3, result.Count());

        Assert.AreEqual("fol/manual-test-from-doc.txt", result.ElementAt(0).Id);
        Assert.AreEqual("manual-test-from-doc.txt", result.ElementAt(0).DisplayName);
        Assert.AreEqual((int)FileDataItemType.File, result.ElementAt(0).Type);

        Assert.AreEqual("fol/manually-tested-folder/", result.ElementAt(1).Id);
        Assert.AreEqual("manually-tested-folder", result.ElementAt(1).DisplayName);
        Assert.AreEqual((int)FileDataItemType.Folder, result.ElementAt(1).Type);

        Assert.AreEqual("fol/maryland.tbx", result.ElementAt(2).Id);
        Assert.AreEqual("maryland.tbx", result.ElementAt(2).DisplayName);
        Assert.AreEqual((int)FileDataItemType.File, result.ElementAt(2).Type);
    }
}