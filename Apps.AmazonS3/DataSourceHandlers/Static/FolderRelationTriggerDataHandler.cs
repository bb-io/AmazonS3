using Apps.AmazonS3.Constants;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.AmazonS3.DataSourceHandlers.Static;

public class FolderRelationTriggerDataHandler : IStaticDataSourceItemHandler
{
    public IEnumerable<DataSourceItem> GetData()
    {
        return new List<DataSourceItem>()
        {
            new DataSourceItem( FolderRelationTrigger.Children, "Children" ),
            new DataSourceItem( FolderRelationTrigger.Descendants, "Descendants" ),
        };
    }
}