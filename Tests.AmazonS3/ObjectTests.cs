using Apps.AmazonS3.Actions;
using Apps.AmazonS3.DataSourceHandlers;
using Apps.AmazonS3.Models.Request;
using Apps.AmazonS3.Models.Request.Base;
using Blackbird.Applications.Sdk.Common.Dynamic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests.AmazonS3.Base;

namespace Tests.AmazonS3;

[TestClass]
public class ObjectTests : TestBase
{
    public const string BucketName = "myuniquebucketfortesting";

    [TestMethod]
    public async Task Search_objects_in_bucket_works()
    {
        var actions = new ObjectActions(InvocationContext, FileManager);

        var result = await actions.ListObjectsInBucket(new BucketRequestModel { BucketName = BucketName }, new ListObjectsRequest { IncludeFoldersInResult = true});

        Assert.IsNotNull(result);
        foreach (var item in result)
        {
            Console.WriteLine(item.Key);
        }
    }
}