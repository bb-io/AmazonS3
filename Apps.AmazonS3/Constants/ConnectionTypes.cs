namespace Apps.AmazonS3.Constants;

public class ConnectionTypes
{
    public const string SingleBucket = "Single bucket";
    public const string AllBuckets = "Developer API key";

    public static readonly IEnumerable<string> SupportedConnectionTypes = [SingleBucket, AllBuckets];
}
