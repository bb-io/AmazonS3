using Apps.AmazonS3.Constants;
using Apps.AmazonS3.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Exceptions;

namespace Apps.AmazonS3.Models.Request;

public record BucketRequest
{
    private string? _bucketNameFromInput;
    private string? _connectedBucket;
    private string? _connectionType;

    [Display("Bucket name")]
    [DataSource(typeof(BucketDataHandler))]
    public string? BucketName
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_connectionType))
                throw new InvalidOperationException("Connection type has not been provided.");

            if (_connectionType == ConnectionTypes.SingleBucket)
            {
                if (string.IsNullOrWhiteSpace(_connectedBucket))
                    throw new PluginMisconfigurationException("Connected bucket is missing for a single-bucket connection.");

                return _connectedBucket;
            }

            if (string.IsNullOrWhiteSpace(_bucketNameFromInput))
                throw new PluginMisconfigurationException("Bucket input is required when not using a single-bucket connection.");

            return _bucketNameFromInput;
        }

        set => _bucketNameFromInput = value;
    }

    public void ProvideConnectionType(string connectionType, string connectedBucket)
    {
        _connectionType = connectionType;
        _connectedBucket = connectedBucket;
    }
}