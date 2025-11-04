using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;

namespace Tests.AmazonS3.Base;
public class FileManager() : IFileManagementClient
{
    public static string ProjectDirectory =>
        Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.Parent?.Parent?.FullName
            ?? throw new DirectoryNotFoundException("Project directory not found.");

    public Task<Stream> DownloadAsync(FileReference reference)
    {
        var path = Path.Combine(ProjectDirectory, @$"Input\{reference.Name}");
        var bytes = File.ReadAllBytes(path);

        var stream = new MemoryStream(bytes);
        return Task.FromResult((Stream)stream);
    }

    public Task<FileReference> UploadAsync(Stream stream, string contentType, string fileName)
    {
        var path = Path.Combine(ProjectDirectory, @$"Output\{fileName}");
        new FileInfo(path)?.Directory?.Create();
        using (var fileStream = File.Create(path))
        {
            stream.CopyTo(fileStream);
        }

        return Task.FromResult(new FileReference() { Name = fileName });
    }
}
