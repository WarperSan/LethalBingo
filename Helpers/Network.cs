using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using LethalBingo.Helpers;
using Newtonsoft.Json;
using UnityEngine.Networking;

namespace LethalBingo.Extensions;

public static class HttpExtension
{
    /// <summary>
    /// Prints the error from the given response
    /// </summary>
    public static async Task PrintError(this HttpResponseMessage? response, string errorMessage)
    {
        if (response is null)
            return;

        // Find error code
        var code = (int)response.StatusCode;
        
        // Find error message
        var stringResponse = await response.Content.ReadAsStringAsync();
        var jsonResponse = JsonConvert.DeserializeObject(stringResponse);

        string? requestMessage = null;

        if (jsonResponse != null)
        {
            var type = jsonResponse.GetType();
            var error = type.GetField("error").GetValue(jsonResponse);
        
            if (error != null)
                requestMessage = error.ToString();
        }
        
        requestMessage ??= "Unknown error";
            
        Logger.Error($"[{code}] {errorMessage}: \"{requestMessage}\".");
    }

    public static UnityWebRequestAsyncOperation RequestAsJsonAsync(string requestUri, object value, HttpMethod method)
    {
        var json = JsonConvert.SerializeObject(value);

        return UnityWebRequest.PostWwwForm(requestUri, json).SendWebRequest();
    }
}