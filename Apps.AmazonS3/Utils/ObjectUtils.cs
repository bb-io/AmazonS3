using Amazon.S3.Model;
using Apps.AmazonS3.Constants;
using Apps.AmazonS3.Models.Request;

namespace Apps.AmazonS3.Utils;

public static class ObjectUtils
{
    public static bool IsObjectInFolder(S3Object s3Object, SearchFilesRequest request)
    {
        if (request.FolderId is null)
            return true;

        if ((string.IsNullOrWhiteSpace(request.FolderRelationTrigger) ||
             request.FolderRelationTrigger == FolderRelationTrigger.Descendants) && s3Object.Key.Contains(request.FolderId))
            return true;

        if (request.FolderRelationTrigger == FolderRelationTrigger.Children &&
            GetParentFolder(s3Object) == request.FolderId.Trim('/').Split('/').Last())
            return true;

        return false;
    }

    public static string GetParentFolder(S3Object s3Object)
    {
        var keySegments = s3Object.Key.Split("/");
        return keySegments.Length < 2 ? string.Empty : keySegments[^2];
    }
}