using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Metadata;

namespace Apps.AmazonS3;

public class AmazonS3Application : IApplication, ICategoryProvider
{
    public IEnumerable<ApplicationCategory> Categories
    {
        get => [ApplicationCategory.AmazonApps, ApplicationCategory.FileManagementAndStorage, ApplicationCategory.SoftwareDevelopment];
        set { }
    }

    public T GetInstance<T>()
    {
        throw new NotImplementedException();
    }
}