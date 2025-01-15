using LethalBingo.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;

namespace LethalBingo.Extensions;

public static class NetworkExtension
{
    /// <summary>
    /// Checks if the given response has failed
    /// </summary>
    public static bool HasFailed(this UnityWebRequest response)
        => response.result is not (UnityWebRequest.Result.Success or UnityWebRequest.Result.InProgress);

    /// <summary>
    /// Fetches the JSON object from the given response
    /// </summary>
    public static T? GetJSON<T>(this UnityWebRequest response)
    {
        string text = response.downloadHandler.text.Trim();

        if (!text.StartsWith("[") && !text.StartsWith("{"))
        {
            Logger.Error($"Expected JSON, but received: {text}");
            return default;
        }
        
        return JsonConvert.DeserializeObject<T>(text);
    }

    /// <summary>
    /// Fetches the JSON object from the given response
    /// </summary>
    public static JObject? GetJSON(this UnityWebRequest response) => response.GetJSON<JObject>();

    /// <summary>
    /// Prints the error from the given response
    /// </summary>
    public static string PrintError(this UnityWebRequest response, string errorMessage)
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

        // Report messages
        var systemMessage = $"[{code}] {errorMessage}";
        
        Logger.Error($"{systemMessage}: \"{requestMessage}\".");
        return systemMessage;
    }
}