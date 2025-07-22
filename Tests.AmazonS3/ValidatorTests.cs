using Apps.AmazonS3.Connections;
using Blackbird.Applications.Sdk.Common.Authentication;
using Newtonsoft.Json;
using Tests.AmazonS3.Base;

namespace Tests.AmazonS3;

[TestClass]
public class ValidatorTests : TestBase
{
    [TestMethod]
    public async Task ValidatesCorrectConnection()
    {
        var validator = new ConnectionValidator();

        var result = await validator.ValidateConnection(Creds, CancellationToken.None);
        Console.WriteLine(result.Message);
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task DoesNotValidateIncorrectConnection()
    {
        var validator = new ConnectionValidator();

        var newCreds = Creds.Select(x => new AuthenticationCredentialsProvider(x.KeyName, x.Value + "_incorrect"));
        var result = await validator.ValidateConnection(newCreds, CancellationToken.None);
        Console.WriteLine(result.Message);
        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void ShowConnectionDefinition()
    {
        var definition = new ConnectionDefinition();

        Console.WriteLine(JsonConvert.SerializeObject(definition.ConnectionPropertyGroups, Formatting.Indented));
    }
}