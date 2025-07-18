using Amazon.S3.Model;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.SDK.Blueprints.Interfaces.FileStorage;

namespace Apps.AmazonS3.Models.Response;

public record FileObject(S3Object s3Object) : BucketObject(s3Object), IDownloadFileInput
{
    [Display("File ID (key)")]
    public string FileId { get { return Key; } set { Key = value; } }
}