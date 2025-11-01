using Apps.AmazonS3.Connections;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;
using System.Reflection;
using Tests.AmazonS3.Base;

namespace Tests.AmazonS3;

[TestClass]
public class ValidatorTests : TestBase
{
    // can't use parent method directly in DynamicData decorator as studio can't see it and shows a warning
    public static string? GetConnectionTypeName(MethodInfo method, object[]? data) => GetConnectionTypeFromDynamicData(method, data);

    [TestMethod]
    [DynamicData(nameof(InvocationContexts), DynamicDataDisplayName = nameof(GetConnectionTypeName))]
    public async Task ValidatesCorrectConnection(InvocationContext context)
    {
        // Arrange
        var validator = new ConnectionValidator();

        // Act
        var result = await validator.ValidateConnection(
            context.AuthenticationCredentialsProviders,
            CancellationToken.None);

        // Assert
        PrintResult(result);
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    [DynamicData(nameof(InvocationContexts), DynamicDataDisplayName = nameof(GetConnectionTypeName))]
    public async Task DoesNotValidateIncorrectConnection(InvocationContext context)
    {
        // Arrange
        var validator = new ConnectionValidator();

        var wrongCredentials = context.AuthenticationCredentialsProviders
            .Select(x => new AuthenticationCredentialsProvider(x.KeyName, x.Value + "_incorrect"));

        // Act
        var result = await validator.ValidateConnection(wrongCredentials, CancellationToken.None);

        // Assert
        PrintResult(result);
        Assert.IsFalse(result.IsValid);
    }
}