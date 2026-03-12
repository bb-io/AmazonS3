using Apps.AmazonS3.Models.Request;
using Apps.AmazonS3.Polling;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Polling;
using System.Reflection;
using Tests.AmazonS3.Base;

namespace Tests.AmazonS3;

[TestClass]
public class PollingTests : TestBase
{
    // can't use parent method directly in DynamicData decorator as studio can't see it and shows a warning
    public static string? GetConnectionTypeName(MethodInfo method, object[]? data) => GetConnectionTypeFromDynamicData(method, data);

    [TestMethod]
    [DynamicData(nameof(AllBucketInvocationContexts), DynamicDataDisplayName = nameof(GetConnectionTypeName))]
    public async Task OnFilesUpdated_ReturnsUpdatedFiles(InvocationContext invocationContext)
    {
        // Arrange
        var polling = new PollingList(invocationContext);
        var bucket = new BucketRequest { BucketName = "blackbird-batch-helloworld-364512-us-central1" };
        var folder = new FolderRequest { FolderId = "test" };
        var dateMemory = new DateMemory { LastInteractionDate = DateTime.UtcNow - TimeSpan.FromDays(30) };
        var request = new PollingEventRequest<DateMemory> { Memory = dateMemory };
        string? folderRelationTrigger = null;

        // Act
        var result = await polling.OnFilesUpdated(request, bucket, folder, folderRelationTrigger);

        // Assert
        PrintResult(result);
        Assert.IsNotNull(result);
    }
}
