using Apps.AmazonS3;
using Apps.AmazonS3.Actions;
using Apps.AmazonS3.DataSourceHandlers;
using Apps.AmazonS3.Models.Request;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using System.Reflection;
using Tests.AmazonS3.Base;

namespace Tests.AmazonS3;

[TestClass]
public class BucketActionsTests : TestBase
{
    public string BucketName = Guid.NewGuid().ToString();

    // can't use parent method directly in DynamicData decorator as studio can't see it and shows a warning
    public static string? GetConnectionTypeName(MethodInfo method, object[]? data) => GetConnectionTypeFromDynamicData(method, data);

    [TestMethod]
    [DynamicData(nameof(AllBucketInvocationContexts), DynamicDataDisplayName = nameof(GetConnectionTypeName))]
    public async Task Create_and_delete_bucket_works(InvocationContext context)
    {
        // Arrange
        var actions = new BucketActions(context);

        var check = new BucketDataHandler(context);
        var checkContext = new DataSourceContext();

        var amazonInvokable = new AmazonInvocable(context);
        var bucket = new BucketRequest { BucketName = BucketName };
        bucket.ProvideConnectionType(amazonInvokable.CurrentConnectionType, amazonInvokable.ConnectedBucket);

        // Act to create
        await actions.CreateBucket(BucketName);        
        var createCheck = await check.GetDataAsync(checkContext, CancellationToken.None);
        Assert.IsTrue(createCheck.Any(x => x.Value == BucketName));

        // Act to delete
        await actions.DeleteBucket(bucket);
        var deleteCheck = await check.GetDataAsync(checkContext, CancellationToken.None);
        Assert.IsTrue(deleteCheck.All(x => x.Value != BucketName));
    }

    [TestMethod]
    [DynamicData(nameof(SingleBucketInvocationContexts), DynamicDataDisplayName = nameof(GetConnectionTypeName))]
    public async Task Create_bucket_throws(InvocationContext context)
    {
        var actions = new BucketActions(context);

        await Assert.ThrowsExactlyAsync<PluginMisconfigurationException>(() 
            => actions.CreateBucket(BucketName));
    }

    [TestMethod]
    [DynamicData(nameof(SingleBucketInvocationContexts), DynamicDataDisplayName = nameof(GetConnectionTypeName))]
    public async Task Delete_bucket_throws(InvocationContext context)
    {
        var actions = new BucketActions(context);
        var bucket = new BucketRequest { BucketName = BucketName };

        await Assert.ThrowsExactlyAsync<PluginMisconfigurationException>(()
            => actions.DeleteBucket(bucket));
    }
}