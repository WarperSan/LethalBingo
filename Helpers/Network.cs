using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;

namespace LethalBingo.Helpers;

public struct NetworkResponse
{
    public string URL;
    public long Code;
    public string Status;
    
    // Error
    public string Error;
    public bool IsError;
    public void PrintError(string errorMessage) => Logger.Error($"[{Code}] {errorMessage}: {Status} ({Error})");
    
    // Content
    public string Content;
    public T? Parse<T>() => JsonConvert.DeserializeObject<T>(Content);
    public JObject? Json() => Parse<JObject>();
}

public static class Network
{
    #region WebRequest

    private static async Task Send(UnityWebRequest request)
    {
        var req = request.SendWebRequest();

        while (!req.isDone)
        {
            Logger.Debug(req.webRequest.url + ": " + (req.progress * 100) + "%");
            await Task.Delay(25);
        }
    }

    private static NetworkResponse CompileResponse(UnityWebRequest req) => new()
    {
        URL = req.url,
        Code = req.responseCode,
        Status = UnityWebRequest.GetHTTPStatusString(req.responseCode),
        
        // Error
        Error = req.error,
        IsError = req.result is not (UnityWebRequest.Result.Success or UnityWebRequest.Result.InProgress),
        
        // Content
        Content = req.downloadHandler.text.Trim()
    };

    public static async Task<NetworkResponse> Get(string uri)
    {
        using var request = UnityWebRequest.Get(uri);
        request.downloadHandler = new DownloadHandlerBuffer();
        
        await Send(request);
        return CompileResponse(request);
    }

    public static async Task<string?> GetCORSToken(string uri)
    {
        using var request = UnityWebRequest.Get(uri);
        
        request.downloadHandler = new DownloadHandlerBuffer();

        await Send(request);
        var response = CompileResponse(request);

        if (response.IsError)
        {
            response.PrintError("Failed to fetch CORS token");
            return null;
        }

        HtmlDocument doc = new HtmlDocument();
        doc.LoadHtml(response.Content);
        
        var input = doc.DocumentNode.SelectSingleNode("//input[@name='csrfmiddlewaretoken']");
        
        if (input == null)
        {
            Logger.Error("Could not find the input 'csrfmiddlewaretoken'.");
            return null;
        }

        var token = input.GetAttributeValue("value", null);
        
        if (token == null)
        {
            Logger.Error("Could not find the attribute 'value'.");
            return null;
        }
        
        return token;
    }

    public static async Task<NetworkResponse> PostJson(string uri, object payload)
    {
        var json = JsonConvert.SerializeObject(payload);
        using var request = UnityWebRequest.Post(uri, json, "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();

        await Send(request);
        return CompileResponse(request);
    }

    public static async Task<NetworkResponse> PostCORSForm(string uri, string corsToken, object payload)
    {
        var formFields = new Dictionary<string, string>();
        
        foreach (var property in payload.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            formFields[property.Name] = property.GetValue(payload).ToString();
        
        using var request = UnityWebRequest.Post(uri, formFields);
        request.downloadHandler = new DownloadHandlerBuffer();
        
        request.SetRequestHeader("X-CSRFToken", corsToken);
        
        await Send(request);
        return CompileResponse(request);
    }

    public static async Task<NetworkResponse> PutJson(string uri, object payload)
    {
        var json = JsonConvert.SerializeObject(payload);
        using var request = UnityWebRequest.Put(uri, json);
        request.downloadHandler = new DownloadHandlerBuffer();

        await Send(request);
        return CompileResponse(request);
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