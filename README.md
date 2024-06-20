# Blackbird.io Amazon S3

Blackbird is the new automation backbone for the language technology industry. Blackbird provides enterprise-scale automation and orchestration with a simple no-code/low-code platform. Blackbird enables ambitious organizations to identify, vet and automate as many processes as possible. Not just localization workflows, but any business and IT process. This repository represents an application that is deployable on Blackbird and usable inside the workflow editor.

## Introduction

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
4. Enter the `Access key`, `Access secret` and `Region` for your Amazon S3 account.
5. Click _Connect_.
6. Verify that connection was added successfully.

![connection](image/README/connection.png)

## Actions

- **List buckets**: Retrieve a list of buckets from your Amazon S3 account. 
- **List objects in bucket**: Retrieve a list of objects from a specified bucket. 
- **Get object**: Retrieve a specific object from a bucket.
- **Upload an object**: Upload a new object to a specified bucket.
- **Create a bucket**: Create a new bucket in your Amazon S3 account.
- **Delete a bucket**: Remove a bucket from your Amazon S3 account.
- **Delete an object**: Remove an object from a bucket.

## Events

- **On object created**: This event triggers when any object is created in your buckets.
- **On object delete marker created**: This event triggers when a delete marker is created for specific objects in your buckets.
- **On object deleted**: This event triggers when any object is permanently deleted from your buckets.
- **On object restore completed**: This event triggers when the restore of a specific object is completed.
- **On object restore expired**: This event triggers when the restore of a specific object has expired.
- **On object restore initiated**: This event triggers when the restore of a specific object is initiated.
- **On object tag added**: This event triggers when a tag is added to a specific object.
- **On object tag removed**: This event triggers when a tag is removed from a specific object.

## Common issues

- **Access Denied**: This error occurs when the user does not have the necessary permissions to access the specified resource. Make sure that the user has the required permissions to perform the action. The typical operations that we use in the app are: `Get bucket location`, `List buckets`, `List objects in bucket`, `Get object`, `Upload an object`, `Create a bucket`, `Delete a bucket`, `Delete an object`. It depends on the action that you are trying to perform.

## Example

Here is an example of how you can use the Amazon S3 app in a workflow:

![example](image/README/example.png)

In this example, the workflow starts with the **On object created** event, which triggers when any object is created in your buckets. Then, the workflow uses the **Get object** action to retrieve the object that was created. In the next step we translate the object via `DeepL` and then upload the translated object back to the Amazon S3 bucket.

## Feedback

Do you want to use this app or do you have feedback on our implementation? Reach out to us using the [established channels](https://www.blackbird.io/) or create an issue.

<!-- end docs -->
