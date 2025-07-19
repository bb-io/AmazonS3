using Apps.AmazonS3.Constants;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.AmazonS3.DataSourceHandlers.Static;

public class FolderRelationTriggerDataHandler : IStaticDataSourceItemHandler
{
    public IEnumerable<DataSourceItem> GetData()
    {
        return
        [
            new( FolderRelationTrigger.Children, "Only files in selected folder" ),
            new( FolderRelationTrigger.Descendants, "Files in selected folder and all subfolders" ),
        ];
    }
}