using System;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;

namespace LethalBingo.Helpers;

public static class Network
{
    /// <summary>
    /// Checks if the given response has failed
    /// </summary>
    public static bool HasFailed(this UnityWebRequest response)
        => response.result is not (UnityWebRequest.Result.Success or UnityWebRequest.Result.InProgress);

    /// <summary>
    /// Fetches the JSON object from the given response
    /// </summary>
    public static JObject? GetJSON(this UnityWebRequest response)
    {
        string text = response.downloadHandler.text.Trim();

        if (!text.StartsWith("[") && !text.StartsWith("{"))
        {
            Logger.Error($"Expected JSON, but received: {text}");
            return null;
        }
        
        return JsonConvert.DeserializeObject<JObject>(text);
    }

    /// <summary>
    /// Prints the error from the given response
    /// </summary>
    public static void PrintError(this UnityWebRequest response, string errorMessage)
    {
        // Find error code
        var code = response.responseCode;
        
        // Find error message
        string? requestMessage;
        var json = response.GetJSON();
        
        var error = json?.GetValue("error");

        if (error == null)
        {
            var allErrors = json?.GetValue("__all__");

            requestMessage = allErrors?.First?.Value<string>("message");
        }
        else
            requestMessage = error.ToString();

        requestMessage ??= "Unknown Error";
        
        Logger.Error($"[{code}] {errorMessage}: \"{requestMessage}\".");
    }

    /// <summary>
    /// Sends a request to the given URI using the given method
    /// </summary>
    public static Task<UnityWebRequest> RequestAsync(string requestUri, HttpMethod method)
    {
        return Task.Run(async () =>
        {
            var req = UnityWebRequest.Get(requestUri).SendWebRequest();
            
            while (!req.isDone)
                await Task.Delay(25);

            return req.webRequest;
        });
    }

    /// <summary>
    /// Sends a request to the given URI using the given method and with the given JSON payload
    /// </summary>
    public static Task<UnityWebRequest> RequestAsJsonAsync(string requestUri, object value, HttpMethod method)
    {
        UnityWebRequest request = new UnityWebRequest(requestUri, method.Method);
        
        var json = JsonConvert.SerializeObject(value);
        
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        return Task.Run(async () =>
        {
            var req = request.SendWebRequest();

            while (!req.isDone)
                await Task.Delay(25);

            return req.webRequest;
        });
    }

    #region Socket

    /// <summary>
    /// Sends a request using the given payload at the given socket
    /// </summary>
    public static async Task SendAsJsonAsync(this ClientWebSocket socket, object value)
    {
        string jsonMessage = JsonConvert.SerializeObject(value);
        byte[] buffer = Encoding.UTF8.GetBytes(jsonMessage);
        await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
    }
    
    /// <summary>
    /// Creates a socket with the given key
    /// </summary>
    public static async Task<ClientWebSocket?> CreateSocket(string? socketKey)
    {
        var socket = new ClientWebSocket();

        try
        {
            // Connect to server
            await socket.ConnectAsync(new Uri(BingoAPI.SOCKETS_URL), CancellationToken.None);

            // Authenticate to the server
            await socket.SendAsJsonAsync(new { socket_key = socketKey });
        }
        catch (Exception e)
        {
            Logger.Error($"Error while trying to create a socket with '{socketKey ?? "null"}': {e.Message}");
            socket.Dispose();
            socket = null;
        }

        return socket;
    }
    
    public static async Task HandleMessages(this ClientWebSocket socket, Action<JObject?> onReceive)
    {
        byte[] buffer = new byte[1024];

        while (socket.State == WebSocketState.Open)
        {
            WebSocketReceiveResult result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
            var json = JsonConvert.DeserializeObject<JObject>(message);
            
            onReceive?.Invoke(json);
        }
    }

    #endregion
}