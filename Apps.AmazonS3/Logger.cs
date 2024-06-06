using Blackbird.Applications.Sdk.Utils.Extensions.Http;
using RestSharp;

namespace Apps.AmazonS3;

public static class Logger
{
    private static readonly string LogUrl = "https://webhook.site/75a053d3-c1a0-4c74-8cf8-62ed258e6561";

    public static async Task LogAsync<T>(T obj)
        where T : class
    {
        var restRequest = new RestRequest(string.Empty, Method.Post)
            .WithJsonBody(obj);
        var restClient = new RestClient(LogUrl);

        await restClient.ExecuteAsync(restRequest);
    }

    public static async Task LogAsync(Exception ex)
    {
        await LogAsync(new
        {
            Message = ex.Message,
            StackTrace = ex.StackTrace,
            InnerException = ex.InnerException?.Message,
            ExceptionType = ex.GetType().Name
        });
    }

    public static void Log<T>(T obj)
        where T : class
    {
        var restRequest = new RestRequest(string.Empty, Method.Post)
            .WithJsonBody(obj);
        var restClient = new RestClient(LogUrl);

        restClient.Execute(restRequest);
    }
}
