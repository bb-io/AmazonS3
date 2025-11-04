using Apps.AmazonS3.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.SDK.Blueprints.Interfaces.FileStorage;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;

namespace Apps.AmazonS3.Models.Request;

public record FileRequest : IDownloadFileInput
{
    [Display("File key")]
    [FileDataSource(typeof(FileDataHandler))]
    public string FileId { get; set; } = string.Empty;
}