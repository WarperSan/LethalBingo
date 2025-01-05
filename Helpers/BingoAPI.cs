using System.Net.Http;
using System.Threading.Tasks;
using LethalBingo.Extensions;
using LethalBingo.Objects;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;

namespace LethalBingo.Helpers;

public static class BingoAPI
{
    // Size
    public const int BINGO_HEIGHT = 5;
    public const int BINGO_WIDTH = 5;
    public const int MAX_BINGO_SIZE = BINGO_HEIGHT * BINGO_WIDTH;
    
    // URLs
    public const string SOCKETS_URL = "wss://sockets.bingosync.com/broadcast";
    private const string BINGO_URL = "https://bingosync.com";
    private const string JOIN_ROOM_URL = BINGO_URL + "/api/join-room";
    private const string GET_BOARD_URL = BINGO_URL + "/room/{0}/board";
    private const string CHANGE_TEAM_URL = BINGO_URL + "/api/color";
    private const string SELECT_SQUARE_URL = BINGO_URL + "/api/select";
    private const string SEND_MESSAGE_URL = BINGO_URL + "/api/chat";
    
    /// <summary>
    /// Joins the given room and creates a client out of it
    /// </summary>
    /// <param name="roomId">ID of the room</param>
    /// <param name="roomPassword">Password of the room</param>
    /// <param name="nickName">Nickname of the user</param>
    /// <param name="isSpectator">Is the user a spectator or not</param>
    public static async Task<bool> JoinRoom(string roomId, string roomPassword, string nickName, bool isSpectator = true)
    {
        Logger.Debug($"Joining the room '{roomId}'...");
        
        var body = new
        {
            room = roomId,
            password = roomPassword,
            nickname = nickName,
            is_spectator = isSpectator
        };
        
        var response = await Network.RequestAsJsonAsync(JOIN_ROOM_URL, body, HttpMethod.Post);

        // If failed, fetch error
        if (response.HasFailed())
        {
            response.PrintError($"Failed to join room '{roomId}'");
            return false;
        }

        var responseJson = response.GetJSON();
        var socket = await Network.CreateSocket(responseJson?.Value<string>("socket_key"));

        if (socket == null)
        {
            Logger.Error("Failed to create the socket.");
            return false;
        }
        
        Logger.Debug("Room joined!");
        
        var client = new BingoClient(socket);

        Logger.Debug("Waiting for connection...");
        var connected = await client.WaitForConnection(60_000);

        if (!connected)
        {
            Logger.Error("Could not connect before the timeout.");
            return false;
        }
        
        Logger.Debug("Client successfully connected!");

        LethalBingo.CurrentClient = client;

        return true;
    }

    /// <summary>
    /// Fetches the current board of the given room
    /// </summary>
    /// <param name="roomId">ID of the room</param>
    public static async Task<SquareData[]?> GetBoard(string roomId)
    {
        Logger.Debug($"Obtaining the board of the room '{roomId}'...");
        
        var url = string.Format(GET_BOARD_URL, roomId);

        var response = await Network.RequestAsync(url, HttpMethod.Get);
        
        if (response.HasFailed())
        {
            response.PrintError($"Failed to obtain the board of the room '{roomId}'");
            return null;
        }

        Logger.Debug($"Board successfully obtained from the room '{roomId}'!");

        var json = response.GetJSON<JArray>();

        if (json == null)
            return [];
        
        var squares = new SquareData[json.Count];

        var index = 0;
        foreach (var square in json.Children())
        {
            squares[index] = SquareData.ParseJSON(square);
            index++;
        }

        return squares;
    }

    /// <summary>
    /// Changes the team of the client in the room
    /// </summary>
    /// <param name="roomId">ID of the room</param>
    /// <param name="newTeam">Team to change to</param>
    public static async Task<bool> ChangeTeam(string roomId, BingoTeam newTeam)
    {
        Logger.Debug($"Changing team to '{newTeam}'...");
        
        var body = new
        {
            room = roomId,
            color = newTeam.GetName(),
        };
        
        var response = await Network.RequestAsJsonAsync(CHANGE_TEAM_URL, body, HttpMethod.Put);

        if (response.HasFailed())
        {
            response.PrintError($"Failed to change team to '{body.color}'");
            return false;
        }

        Logger.Debug($"Team successfully changed to '{newTeam}'!");
        return true;
    }
    
    private static async Task<UnityWebRequest?> SelectSquare(string roomId, BingoTeam team, int id, bool isMarking)
    {
        if (id is <= 0 or > MAX_BINGO_SIZE)
        {
            Logger.Error("Could not mark square as the id is out of range.");
            return null;
        }
        
        Logger.Debug($"{(isMarking ? "Marking" : "Clearing")} the square '{id}' for the team '{team}'.");
        
        var body = new
        {
            room = roomId,
            color = team.GetName(),
            slot = id,
            remove_color = !isMarking
        };
        
        return await Network.RequestAsJsonAsync(SELECT_SQUARE_URL, body, HttpMethod.Put);
    }

    /// <summary>
    /// Marks a square in the room for a certain team
    /// </summary>
    /// <param name="roomId">ID of the room</param>
    /// <param name="team">Name of the team</param>
    /// <param name="id">Index of the square</param>
    public static async Task<bool> MarkSquare(string roomId, BingoTeam team, int id)
    {
        var response = await SelectSquare(roomId, team, id, true);

        if (response == null)
            return false;

        if (response.HasFailed())
        {
            response.PrintError($"Failed to mark the square '{id}'");
            return false;
        }
        
        Logger.Debug($"Square '{id}' successfully marked!");
        
        return true;
    }
    
    /// <summary>
    /// Clears a square in the room for a certain team
    /// </summary>
    /// <param name="roomId">ID of the room</param>
    /// <param name="team">Name of the team</param>
    /// <param name="id">Index of the square</param>
    public static async Task<bool> ClearSquare(string roomId, BingoTeam team, int id)
    {
        var response = await SelectSquare(roomId, team, id, false);

        if (response == null)
            return false;

        if (response.HasFailed())
        {
            response.PrintError($"Failed to clear the square '{id}'");
            return false;
        }
        
        Logger.Debug($"Square '{id}' successfully cleared!");
        
        return true;
    }
    
    /// <summary>
    /// Sends a message in the room
    /// </summary>
    /// <param name="roomId">ID of the room</param>
    /// <param name="message">Message to send</param>
    public static async Task<bool> SendMessage(string roomId, string message)
    {
        var body = new
        {
            room = roomId,
            text = message
        };
        
        var response = await Network.RequestAsJsonAsync(SEND_MESSAGE_URL, body, HttpMethod.Put);

        if (response.HasFailed())
        {
            response.PrintError($"Failed to send the message '{message}'");
            return false;
        }

        return true;
    }
}

public struct PlayerData
{
    public string? UUID;
    public string? Name;
    public BingoTeam Team;
    public bool IsSpectator;

    public static PlayerData ParseJSON(JToken? obj) => new()
    {
        UUID = obj?.Value<string>("uuid"),
        Name = obj?.Value<string>("name"),
        Team = obj?.Value<string>("color").GetTeam() ?? BingoTeam.BLANK,
        IsSpectator = obj?.Value<bool>("is_spectator") ?? false
    };
}

public struct SquareData
{
    public string? Name;
    public int Index;
    public BingoTeam[] Teams;

    public static SquareData ParseJSON(JToken? obj)
    {
        var slot = obj?.Value<string>("slot")?.Replace("slot", "");
        return new SquareData
        {
            Name = obj?.Value<string>("name"),
            Index = slot != null && int.TryParse(slot, out var index) ? index : 0,
            Teams = obj?.Value<string>("colors").GetTeams() ?? []
        };
    }
}