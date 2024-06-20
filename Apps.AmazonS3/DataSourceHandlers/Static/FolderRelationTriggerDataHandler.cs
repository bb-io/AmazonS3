using Apps.AmazonS3.Constants;
using Blackbird.Applications.Sdk.Common.Dictionaries;

namespace Apps.AmazonS3.DataSourceHandlers.Static;

public class FolderRelationTriggerDataHandler : IStaticDataSourceHandler
{
    public Dictionary<string, string> GetData()
    {
        return new()
        {
            [FolderRelationTrigger.Children] = "Children",
            [FolderRelationTrigger.Descendants] = "Descendants",
        };
    }
}