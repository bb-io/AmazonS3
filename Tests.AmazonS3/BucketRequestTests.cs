using Apps.AmazonS3.Constants;
using Apps.AmazonS3.Models.Request;
using Blackbird.Applications.Sdk.Common.Exceptions;

namespace Tests.AmazonS3;

[TestClass]
public class BucketRequestTests
{
    private const string BucketFromConnection = "connected-bucket";
    private const string BucketFromInput = "input-bucket";

    [TestMethod]
    public void BucketName_Throws_WhenConnectionNotProvided()
    {
        // Arrange
        var request = new BucketRequest();

        // Act & Assert
        Assert.ThrowsExactly<InvalidOperationException>(() =>
        {
            var _ = request.BucketName;
        });
    }

    [TestMethod]
    public void BucketName_IgnoresOverrideFromInit_WhenSingleConnection()
    {
        // Arrange
        var request = new BucketRequest { BucketName = BucketFromInput };  // Attempt to override is ignored
        request.ProvideConnectionType(ConnectionTypes.SingleBucket, BucketFromConnection);

        // Act
        var result = request.BucketName;

        // Assert
        Assert.AreEqual(BucketFromConnection, result);
    }

    [TestMethod]
    public void BucketName_IgnoresOverrideAfterProvidingConnectionType_WhenSingleConnection()
    {
        // Arrange
        var request = new BucketRequest();
        request.ProvideConnectionType(ConnectionTypes.SingleBucket, BucketFromConnection);
        request.BucketName = BucketFromInput; // Attempt to override is ignored

        // Act
        var result = request.BucketName;

        // Assert
        Assert.AreEqual(BucketFromConnection, result);
    }

    [TestMethod]
    public void BucketName_Throws_WhenSingleConnectionWithoutConnectedBucket()
    {
        // Arrange
        var request = new BucketRequest();
        request.ProvideConnectionType(ConnectionTypes.SingleBucket, string.Empty);

        // Act & Assert
        Assert.ThrowsExactly<PluginMisconfigurationException>(() =>
        {
            var _ = request.BucketName;
        });
    }

    [TestMethod]
    public void BucketName_ReturnsInputBucket_WhenMultipleBucketConnection()
    {
        // Arrange
        var request = new BucketRequest { BucketName = BucketFromInput };
        request.ProvideConnectionType(ConnectionTypes.AllBuckets, BucketFromConnection);

        // Act
        var result = request.BucketName;

        // Assert
        Assert.AreEqual(BucketFromInput, result);
    }

    [TestMethod]
    public void BucketName_Throws_WhenMultipleBucketConnectionWithoutInput()
    {
        // Arrange
        var request = new BucketRequest();
        request.ProvideConnectionType(ConnectionTypes.AllBuckets, BucketFromConnection);

        // Act & Assert
        Assert.ThrowsExactly<PluginMisconfigurationException>(() =>
        {
            var _ = request.BucketName;
        });
    }
}
