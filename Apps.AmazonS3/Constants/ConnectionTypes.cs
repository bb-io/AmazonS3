namespace Apps.AmazonS3.Constants;

public class ConnectionTypes
{
    public const string SingleBucket = "Single bucket";
    public const string AllBuckets = "Developer API key";
    public const string AssumeRole = "Assume role";
    public const string S3Compatible = "S3 Compatible storage";

    public static readonly IEnumerable<string> SupportedConnectionTypes = [SingleBucket, AllBuckets, AssumeRole, S3Compatible];
}
