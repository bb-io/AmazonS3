using Apps.AmazonS3.Actions;
using Apps.AmazonS3.Models.Request;
using Blackbird.Applications.Sdk.Common.Invocation;
using System.Reflection;
using Tests.AmazonS3.Base;

namespace Tests.AmazonS3;

[TestClass]
public class FolderActionsTests : TestBase
{
    private readonly string TestFolderName = Guid.NewGuid().ToString() + '/';
    private readonly string TestParentFolder = Guid.NewGuid().ToString() + '/';

    // can't use parent method directly in DynamicData decorator as studio can't see it and shows a warning
    public static string? GetConnectionTypeName(MethodInfo method, object[]? data) => GetConnectionTypeFromDynamicData(method, data);

    [TestMethod]
    [DynamicData(nameof(InvocationContexts), DynamicDataDisplayName = nameof(GetConnectionTypeName))]
    public async Task Create_Delete_folder_works(InvocationContext context)
    {
        // Test data
        var actions = new FolderActions(context);
        var bucketRequest = new BucketRequest { BucketName = TestBucketName };
        var newFolderInputCombinations = new List<Tuple<string, string?, string>>
        {
            new(TestFolderName, TestParentFolder, TestParentFolder + TestFolderName),
            new(TestFolderName, string.Empty, TestFolderName),
            new(TestFolderName, null, TestFolderName),
        };
        
        foreach(var inputCombination in newFolderInputCombinations)
        {
            // Arrange
            var newFolderName = inputCombination.Item1;
            var newParentFolder = inputCombination.Item2;
            var expectedFolderKey = inputCombination.Item3;

            // Act
            var createFolderResponse = await actions.CreateFolder(bucketRequest, newFolderName, newParentFolder);

            var deleteFolderRequest = new FolderRequest { FolderId = createFolderResponse.FolderId };
            await actions.DeleteFolder(bucketRequest, deleteFolderRequest);

            // Assert
            PrintResult(createFolderResponse);
            Assert.AreEqual(expectedFolderKey, createFolderResponse.FolderId);
        }
    }
}
