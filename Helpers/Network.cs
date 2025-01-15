using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;

namespace LethalBingo.Helpers;

public static class Network
{
    #region WebRequest

    /// <summary>
    /// Sends a request to the given URI using the given method
    /// </summary>
    private static Task<UnityWebRequest> SendRequest(string requestUri, HttpMethod method, Action<UnityWebRequest>? onPrepare)
    {
        UnityWebRequest request = new UnityWebRequest(requestUri, method.Method);
        
        onPrepare?.Invoke(request);

        request.downloadHandler = new DownloadHandlerBuffer();

        return Task.Run(async () =>
        {
            var req = request.SendWebRequest();

            while (!req.isDone)
                await Task.Delay(25);

            return req.webRequest;
        });
    }

    /// <summary>
    /// Sends a request to the given URI using the given method
    /// </summary>
    public static Task<UnityWebRequest> Request(string requestUri, HttpMethod method) 
        => SendRequest(requestUri, method, null);

    /// <summary>
    /// Sends a request to the given URI using the given method and with the given JSON payload
    /// </summary>
    public static Task<UnityWebRequest> RequestAsJson(string requestUri, HttpMethod method, object value)
        => SendRequest(requestUri, method, r => PrepareJson(r, value));
    
    /// <summary>
    /// Sends a request to the given URI using the given method and with the given form payload
    /// </summary>
    public static Task<UnityWebRequest> RequestAsForm(string requestUri, HttpMethod method, object value)
        => SendRequest(requestUri, method, r => PrepareForm(r, value));

    private static void PrepareJson(UnityWebRequest request, object payload)
    {
        var json = JsonConvert.SerializeObject(payload);
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        request.SetRequestHeader("Content-Type", "application/json");
    }

    private static void PrepareForm(UnityWebRequest request, object payload)
    {
        var properties = payload.GetType().GetProperties();
        var keyValuePairs = new List<string>();

        foreach (var prop in properties)
        {
            var value = prop.GetValue(payload)?.ToString();
            if (value != null)
            {
                var encodedKey = HttpUtility.UrlEncode(prop.Name);
                var encodedValue = HttpUtility.UrlEncode(value);
                keyValuePairs.Add($"{encodedKey}={encodedValue}");
            }
        }
        
        string data = string.Join("&", keyValuePairs);
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(data));

        const string csrfToken = "JdNt8KeU7a6gs9ygalsjmMGiILrfqacT";
        const string sessionId = "nnyvfe24m7fucng03zgc5oqbfxr4v1qf";
        
        request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
        request.SetRequestHeader("Cookie", $"csrftoken={csrfToken}; sessionid={sessionId}");
        request.SetRequestHeader("X-CSRFToken", csrfToken);
    }

    #endregion

    #region Socket

    /// <summary>
    /// Sends a request using the given payload at the given socket
    /// </summary>
    public static async Task SendAsJson(this ClientWebSocket socket, object value)
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
            await socket.SendAsJson(new { socket_key = socketKey });
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