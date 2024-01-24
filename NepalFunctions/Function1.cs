using System.Diagnostics;
using System.Net;
using System.Text;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace NepalFunctions;

public class GreetPerson(IConfiguration config, ILoggerFactory loggerFactory)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<GreetPerson>();

    [Function("GreetPerson")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req, CancellationToken cancellationToken)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("{function} {triggerType} trigger function execution started at: {timestamp}", "GreetPerson", "HTTP", DateTime.UtcNow);
        await LogIncomingRequest(req);
        HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
        System.Collections.Specialized.NameValueCollection items = req.Query;
        StringBuilder stringBuilder = new();
        foreach (string key in items)
        {
            stringBuilder.Append($"{key} = {items[key]}");
            stringBuilder.Append(Environment.NewLine);
        }
        stringBuilder.Append(Environment.NewLine);
        stringBuilder.Append(Environment.NewLine);
        stringBuilder.Append($"The current local time is {DateTime.Now}.");
        stringBuilder.Append(Environment.NewLine);
        stringBuilder.Append($"The current UTC time is {DateTime.UtcNow}.");
        stopwatch.Stop();
        stringBuilder.Append(Environment.NewLine);
        string stopwatchTimer = stopwatch.ElapsedMilliseconds.ToString();
        stringBuilder.Append($"This request took us at least {stopwatchTimer} milliseconds, likely more but that is the minimum. This should be as close to zero as possible.");
        response.WriteString(stringBuilder.ToString());
        response.StatusCode = HttpStatusCode.OK;
        return response;
    }

    private async Task LogIncomingRequest(HttpRequestData req)
    {
        string method = req.Method;
        string url = req.Url.ToString();
        string headers = string.Join(Environment.NewLine, req.Headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}"));
        string? body = await req.ReadAsStringAsync();

        string rawRequest = $"{method} {url}{Environment.NewLine}{headers}{Environment.NewLine}{Environment.NewLine}{body}";
        _logger.LogInformation("{function} {triggerType} had this as request data {rawRequest}", nameof(GreetPerson), "HTTP", rawRequest);
    }
}
