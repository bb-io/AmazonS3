using System.Net;
using System.Net.Sockets;
using System.Text;
using Apps.AmazonS3.Constants;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Tests.AmazonS3.Base;

public sealed class S3ErrorStubServer : IDisposable
{
    private readonly HttpListener _listener = new();
    private readonly HttpStatusCode _statusCode;
    private readonly string _errorBody;
    private readonly Task _serveLoop;

    public string ServiceUrl { get; }

    public S3ErrorStubServer(HttpStatusCode statusCode, string errorCode, string errorMessage)
    {
        _statusCode = statusCode;
        _errorBody = $"<?xml version=\"1.0\" encoding=\"UTF-8\"?><Error><Code>{errorCode}</Code><Message>{errorMessage}</Message><RequestId>request-1</RequestId><HostId>host-1</HostId></Error>";
        ServiceUrl = $"http://localhost:{GetFreePort()}/";
        _listener.Prefixes.Add(ServiceUrl);
        _listener.Start();
        _serveLoop = Task.Run(ServeAsync);
    }

    public InvocationContext CreateS3CompatibleInvocationContext() => new()
    {
        AuthenticationCredentialsProviders =
        [
            new AuthenticationCredentialsProvider(CredNames.ConnectionType, ConnectionTypes.S3Compatible),
            new AuthenticationCredentialsProvider(CredNames.AccessKey, "access-key"),
            new AuthenticationCredentialsProvider(CredNames.AccessSecret, "access-secret"),
            new AuthenticationCredentialsProvider(CredNames.Region, "us-east-1"),
            new AuthenticationCredentialsProvider(CredNames.ServiceUrl, ServiceUrl),
        ]
    };

    public void Dispose()
    {
        _listener.Stop();
        _listener.Close();
        _serveLoop.Wait(TimeSpan.FromSeconds(5));
    }

    private async Task ServeAsync()
    {
        while (_listener.IsListening)
        {
            HttpListenerContext context;
            try
            {
                context = await _listener.GetContextAsync();
            }
            catch (Exception) when (!_listener.IsListening)
            {
                return;
            }

            var body = Encoding.UTF8.GetBytes(_errorBody);
            context.Response.StatusCode = (int)_statusCode;
            context.Response.ContentType = "application/xml";
            context.Response.ContentLength64 = body.Length;
            await context.Response.OutputStream.WriteAsync(body);
            context.Response.Close();
        }
    }

    private static int GetFreePort()
    {
        var tcpListener = new TcpListener(IPAddress.Loopback, 0);
        tcpListener.Start();
        var port = ((IPEndPoint)tcpListener.LocalEndpoint).Port;
        tcpListener.Stop();
        return port;
    }
}
