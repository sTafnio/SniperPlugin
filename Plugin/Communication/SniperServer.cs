using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using ExileCore;

namespace SniperPlugin.Communication;

public class SniperServer(SniperPlugin plugin)
{
    private readonly SniperPlugin _plugin = plugin;
    private HttpListener? _listener;
    public bool IsRunning => _listener?.IsListening ?? false;
    private CancellationTokenSource? _cts;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public const int Port = 49152;

    public void Start()
    {
        if (IsRunning) return;

        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://localhost:{Port}/");
        _listener.Start();

        _cts = new CancellationTokenSource();
        Task.Run(() => ListenAsync(_cts.Token));
        DebugWindow.LogMsg($"[Sniper] Server started on port {Port}");
    }

    public void Stop()
    {
        _cts?.Cancel();
        _listener?.Stop();
        _listener?.Abort();

        _cts = null;
        _listener = null;

        DebugWindow.LogMsg($"[Sniper] Server stopped on port {Port}");
    }

    private async Task ListenAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                var context = await _listener!.GetContextAsync();
                _ = Task.Run(() => ProcessRequestAsync(context), token);
            }
            catch (HttpListenerException ex) when (ex.ErrorCode == 995 || token.IsCancellationRequested)
            {
                break;
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (Exception ex)
            {
                if (token.IsCancellationRequested) break;
                DebugWindow.LogError($"[Sniper] Server Listener Error: {ex.Message}");
            }
        }
    }

    private async Task ProcessRequestAsync(HttpListenerContext context)
    {
        var response = context.Response;

        response.Headers.Add("Access-Control-Allow-Origin", "*");
        response.Headers.Add("Access-Control-Allow-Methods", "POST, OPTIONS");
        response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");

        try
        {
            if (context.Request.HttpMethod == "OPTIONS")
            {
                response.StatusCode = (int)HttpStatusCode.OK;
                response.Close();
                return;
            }

            if (context.Request.HttpMethod == "POST")
            {
                await HandlePostRequestAsync(context.Request, response);
            }
            else
            {
                response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
            }
        }
        catch (Exception ex)
        {
            DebugWindow.LogError($"[Sniper] Request handling error: {ex.Message}");
            await WriteJsonResponseAsync(response, HttpStatusCode.InternalServerError, new { error = ex.Message });
        }
        finally
        {
            response.Close();
        }
    }

    private async Task HandlePostRequestAsync(HttpListenerRequest request, HttpListenerResponse response)
    {
        using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
        string body = await reader.ReadToEndAsync();

        if (request.Url?.AbsolutePath == "/item")
        {
            var item = ItemData.Create(body, JsonOptions);
            if (item != null)
            {
                var decision = await _plugin.Logic.EvaluateTeleportForItem(item);
                await WriteJsonResponseAsync(response, HttpStatusCode.OK, decision);
            }
            else
            {
                await WriteJsonResponseAsync(response, HttpStatusCode.BadRequest, new { error = "Invalid Item Data" });
            }
        }
        else if (request.Url?.AbsolutePath == "/teleport-success")
        {
            _ = Task.Run(_plugin.Logic.OnTeleportSuccess);
            await WriteJsonResponseAsync(response, HttpStatusCode.OK, new { status = "ok" });
        }
        else if (request.Url?.AbsolutePath == "/teleport-failure")
        {
            var reason = body;
            try
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("error", out var errorProp))
                    reason = errorProp.GetString() ?? body;
            }
            catch { }
            _ = Task.Run(() => _plugin.Logic!.OnTeleportFailure(reason));
            await WriteJsonResponseAsync(response, HttpStatusCode.OK, new { status = "ok" });
        }
        else
        {
            await WriteJsonResponseAsync(response, HttpStatusCode.NotFound, new { error = "Unknown endpoint" });
        }
    }

    private static async Task WriteJsonResponseAsync(HttpListenerResponse response, HttpStatusCode status, object data)
    {
        response.StatusCode = (int)status;
        response.ContentType = "application/json";
        byte[] buffer = JsonSerializer.SerializeToUtf8Bytes(data, JsonOptions);
        response.ContentLength64 = buffer.Length;
        await response.OutputStream.WriteAsync(buffer);
    }
}
