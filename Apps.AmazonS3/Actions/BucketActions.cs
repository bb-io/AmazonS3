﻿using Amazon.S3.Model;
using Apps.AmazonS3.Factories;
using Apps.AmazonS3.Models.Request;
using Apps.AmazonS3.Models.Request.Base;
using Apps.AmazonS3.Models.Response;
using Apps.AmazonS3.Utils;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using System.Text.RegularExpressions;
using ListObjectsRequest = Apps.AmazonS3.Models.Request.ListObjectsRequest;

namespace Apps.AmazonS3.Actions;

[ActionList]
public class BucketActions(IFileManagementClient fileManagementClient)
{
    #region Get

    [Action("List buckets", Description = "List all user's buckets")]
    public async Task<List<Bucket>> ListBuckets(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders)
    {
        var client = AmazonClientFactory.CreateS3Client(authenticationCredentialsProviders.ToArray());
        var bucketResponse = await AmazonClientHandler.ExecuteS3Action(() => client.ListBucketsAsync());

        return bucketResponse.Buckets.Select(x => new Bucket(x)).ToList();
    }

    [Action("List objects in a bucket", Description = "List all objects in a specific bucket")]
    public async Task<List<BucketObject>> ListObjectsInBucket(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        [ActionParameter] BucketRequestModel bucket, [ActionParameter] ListObjectsRequest input)
    {
        var request = new ListObjectsV2Request()
        {
            BucketName = bucket.BucketName
        };

        var client =
            await AmazonClientHandler.ExecuteS3Action(async () => await AmazonClientFactory.CreateS3BucketClient(authenticationCredentialsProviders.ToArray(),
                bucket.BucketName));

        var objects = AmazonClientHandler.ExecuteS3Action(() => client.Paginators.ListObjectsV2(request));
        var result = new List<BucketObject>();

        try
        {
            await foreach (var s3Object in objects.S3Objects)
            {
                if (input.IncludeFoldersInResult != null && input.IncludeFoldersInResult == true)
                { 
                    result.Add(new BucketObject(s3Object)); 
                }
                else
                {
                    if (!s3Object.Key.EndsWith("/")  ) 
                    {
                        result.Add(new BucketObject(s3Object));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }

        return result;
    }

    [Action("Get object", Description = "Get object from a bucket")]
    public async Task<BucketFileObject> GetObject(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        [ActionParameter] ObjectRequestModel objectData)
    {
        var request = new GetObjectRequest
        {
            BucketName = objectData.BucketName,
            Key = objectData.Key
        };

        var client =
            await AmazonClientFactory.CreateS3BucketClient(authenticationCredentialsProviders.ToArray(),
                objectData.BucketName);

        var response = await AmazonClientHandler.ExecuteS3Action(() => client.GetObjectAsync(request));

        var downloadFileUrl = client.GetPreSignedURL(new()
        {
            BucketName = objectData.BucketName,
            Key = objectData.Key,
            Expires = DateTime.Now.AddHours(1)
        });
        string fileName = response.Key.Contains("/") ? response.Key.Substring(response.Key.LastIndexOf('/') + 1) : response.Key;

        var file = new FileReference(new(HttpMethod.Get, downloadFileUrl), fileName, response.Headers.ContentType);
        return new(response, file);
    }

    #endregion

    #region Put

    [Action("Upload an object", Description = "Upload an object to a bucket")]
    public async Task UploadObject(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        [ActionParameter] UploadObjectModel uploadData)
    {
        var fileStream = await fileManagementClient.DownloadAsync(uploadData.File);
        var request = new PutObjectRequest
        {
            BucketName = uploadData.BucketName,
            Key = uploadData.File.Name,
            InputStream = fileStream,
            Headers = { ContentLength = uploadData.File.Size },
            ContentType = uploadData.File.ContentType
        };

        if (!string.IsNullOrEmpty(uploadData.ObjectMetadata))
        {
            request.Metadata.Add("object", uploadData.ObjectMetadata);
        }

        var client =
            await AmazonClientFactory.CreateS3BucketClient(authenticationCredentialsProviders.ToArray(),
                uploadData.BucketName);

        await AmazonClientHandler.ExecuteS3Action(() => client.PutObjectAsync(request));
    }

    [Action("Create a bucket", Description = "Create an S3 bucket.")]
    public async Task<Bucket> CreateBucket(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        [ActionParameter] [Display("Bucket name")]
        string bucketName)
    {
        var request = new PutBucketRequest
        {
            BucketName = bucketName,
            UseClientRegion = true,
        };

        var client = AmazonClientFactory.CreateS3Client(authenticationCredentialsProviders.ToArray());

        await AmazonClientHandler.ExecuteS3Action(() => client.PutBucketAsync(request));

        return new(bucketName, DateTime.Now);
    }

    #endregion

    #region Delete

    [Action("Delete a bucket", Description = "Create an S3 bucket.")]
    public async Task DeleteBucket(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        [ActionParameter] BucketRequestModel bucket)
    {
        var request = new DeleteBucketRequest
        {
            BucketName = bucket.BucketName,
        };

        var client =
            await AmazonClientFactory.CreateS3BucketClient(authenticationCredentialsProviders.ToArray(),
                bucket.BucketName);

        await AmazonClientHandler.ExecuteS3Action(() => client.DeleteBucketAsync(request));
    }

    [Action("Delete an object", Description = "Delete an object out of the S3 bucket.")]
    public async Task DeleteObject(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        [ActionParameter] ObjectRequestModel deleteData)
    {
        var request = new DeleteObjectRequest
        {
            BucketName = deleteData.BucketName,
            Key = deleteData.Key
        };

        var client =
            await AmazonClientFactory.CreateS3BucketClient(authenticationCredentialsProviders.ToArray(),
                deleteData.BucketName);

        await AmazonClientHandler.ExecuteS3Action(() => client.DeleteObjectAsync(request));
    }

    #endregion
}