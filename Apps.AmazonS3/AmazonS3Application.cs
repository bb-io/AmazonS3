using Blackbird.Applications.Sdk.Common;

namespace Apps.AmazonS3;

public class AmazonS3Application : IApplication
{
    public string Name
    {
        get => "Amazon S3";
        set { }
    }

    public T GetInstance<T>()
    {
        throw new NotImplementedException();
    }
}