# Blackbird.io Amazon S3

Blackbird is the new automation backbone for the language technology industry. Blackbird provides enterprise-scale automation and orchestration with a simple no-code/low-code platform. Blackbird enables ambitious organizations to identify, vet and automate as many processes as possible. Not just localization workflows, but any business and IT process. This repository represents an application that is deployable on Blackbird and usable inside the workflow editor.

## Introduction

<!-- begin docs -->

Amazon S3 or Amazon Simple Storage Service is a service offered by Amazon Web Services that provides object storage through a web service interface. Amazon S3 uses the same scalable storage infrastructure that Amazon.com uses to run its e-commerce network. Amazon S3 can store any type of object, which allows uses like storage for Internet applications, backups, disaster recovery, data archives, data lakes for analytics, and hybrid cloud storage.

## Before setting up

Before you can connect you need to make sure that:

- You have a **Amazon S3** account and you have the credentials to access it.
- You have the `Access key` and `Access secret` for your Amazon S3 account.
- You know `Region` for your Amazon S3 account.

You can find how to get the `Access key` and `Access secret` [here](https://support.promax.com/knowledge/amazon-s3).

## Connecting

1. Navigate to Apps, and identify the **Amazon S3** app. You can use search to find it.
2. Click _Add Connection_.
3. Name your connection for future reference e.g. 'My X connection'.
4. Enter the `Access key`, `Access secret` and selection your `Region` for your Amazon S3 account.
5. Click _Connect_.
6. Verify that connection was added successfully.

![connection](image/README/connection.png)

## Actions

### Bucket actions

- **Create bucket**: Creates a new bucket in your Amazon S3 account.
- **Delete bucket**: Removes a bucket from your Amazon S3 account.

### File actions

Single files actions are using File key to locale specific object in S3 bucket. Use a full path there, for example: `example/folder/your_file__name__with.extension`

- **Search files**: Retrieve a list of files from a specified bucket. Optionally search by folder prefix, e.g. specify a full folder path `example/folder/` to find all files.
- **Download file**: Retrieve a specific file from a bucket.
- **Upload file**: Uploads a new file to a specified bucket.
- **Delete file**: Remove a file from a bucket.

### Folder actions

Single folder actions are using Folder prefix to locale specific object in S3 bucket. Use a full path there, for example: `example/folder/`. Notice, that folders in S3 should always end with a slash (`/`) character.

- **Create folder**: Creates a new folder in a specified bucket.
- **Delete folder**: Removes a folder from a specified bucket.

## Events

- **On files updated**: This event triggers when a file (object) is changed in any way. Could be filtered by a folder.
- **On file or folder created**: This event triggers when any object is created in your buckets.
- **On file or folder delete marker created**: This event triggers when a delete marker is created for specific objects in your buckets.
- **On file or folder deleted**: This event triggers when any object is permanently deleted from your buckets.
- **On file or folder restore completed**: This event triggers when the restore of a specific object is completed.
- **On file or folder restore expired**: This event triggers when the restore of a specific object has expired.
- **On file or folder restore initiated**: This event triggers when the restore of a specific object is initiated.
- **On file or folder tag added**: This event triggers when a tag is added to a specific object.
- **On file or folder tag removed**: This event triggers when a tag is removed from a specific object.

## Common issues

- **Access Denied**: This error occurs when the user does not have the necessary permissions to access the specified resource. Make sure that the user has the required permissions to perform the action. The typical operations that we use in the app are: `Get bucket location`, `List objects in bucket`, `Get object`, `Upload object`, `Create a bucket`, `Delete a bucket`, `Delete object`. It depends on the action that you are trying to perform.

## Example

Here is an example of how you can use the Amazon S3 app in a workflow:

![example](image/README/example.png)

In this example, the workflow starts with the **On files updated** event, which triggers when a file is created or changed in your buckets. Then, the workflow uses the **Download file** action to retrieve the file. In the next step we translate the object via `DeepL` and then upload the translated file back to the Amazon S3 bucket.

## Feedback

Do you want to use this app or do you have feedback on our implementation? Reach out to us using the [established channels](https://www.blackbird.io/) or create an issue.

<!-- end docs -->
