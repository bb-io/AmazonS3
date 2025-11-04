using Amazon.S3.Model;
using Apps.AmazonS3.Constants;

namespace Apps.AmazonS3.Utils;

public static class ObjectUtils
{
    public static bool IsObjectInFolder(S3Object s3Object, string folderId, string? folderRelationTrigger)
    {
        if (folderId is null)
            return true;

        folderRelationTrigger ??= FolderRelationTrigger.Descendants;

        if (folderRelationTrigger == FolderRelationTrigger.Descendants
            && s3Object.Key.Contains(folderId))
            return true;

        if (folderRelationTrigger == FolderRelationTrigger.Children
            && GetParentFolder(s3Object) == folderId.Trim('/').Split('/').Last())
            return true;

        return false;
    }

    public static string GetParentFolder(S3Object s3Object)
    {
        var keySegments = s3Object.Key.Split("/");
        return keySegments.Length < 2 ? string.Empty : keySegments[^2];
    }
}