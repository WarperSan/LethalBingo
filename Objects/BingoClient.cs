using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using LethalBingo.Extensions;
using LethalBingo.Helpers;
using Newtonsoft.Json.Linq;

namespace LethalBingo.Objects;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public class BingoClient
{
    protected string? roomId { get; private set; }
    protected string? playerUUID { get; private set; }
    protected BingoTeam team { get; private set; }

    internal BingoClient(ClientWebSocket socket)
    {
        roomId = null;
        playerUUID = null;
        team = BingoTeam.BLANK;
        this.socket = socket;

        _ = socket.HandleMessages(OnSocketReceived);
    }
    
    #region Socket

    private readonly ClientWebSocket socket;

    public async Task<bool> WaitForConnection(float timeoutMS)
    {
        while (roomId == null && timeoutMS > 0)
        {
            await Task.Delay(25);
            timeoutMS -= 25;
        }

        return roomId != null;
    }

    private void OnSocketReceived(JObject? json)
    {
        if (json == null)
            return;
        
        Logger.Info(json);
        
        var type = json.Value<string>("type");

        switch (type)
        {
            case "connection":
                HandleConnection(json);
                return;
            case "chat":
                HandleIncomingMessage(json);
                return;
            default:
                Logger.Error($"Unhandled response: {json}");
                break;
        }
    }

    private void HandleConnection(JObject connection)
    {
        var eventType = connection.Value<string>("event_type");
        
        if (eventType != "connected")
            return;
        
        if (roomId == null)
            OnSelfConnect(connection);
        else
            OnOtherConnect(connection);
    }

    private void HandleIncomingMessage(JObject message)
    {
        var uuid = message.GetValue("player")?.Value<string>("uuid");
        
        if (playerUUID == uuid)
            OnSelfMessageReceived(message);
        else
            OnOtherMessageReceived(message);
    }
    
    #endregion

    #region Callbacks

    /// <summary>
    /// Called when this client gets connected to the room
    /// </summary>
    protected virtual void OnSelfConnect(JObject connection)
    {
        roomId = connection.Value<string>("room");
        playerUUID = connection.GetValue("player")?.Value<string>("uuid");
        team = connection.Value<string>("player_color").GetTeam();
    }

    /// <summary>
    /// Called when another client gets connected to the room
    /// </summary>
    protected virtual void OnOtherConnect(JObject connection) { /* DO NOTHING */ }

    /// <summary>
    /// Called when this client sends a message to the room
    /// </summary>
    protected virtual void OnSelfMessageReceived(JObject message) { /* DO NOTHING */ }
    
    /// <summary>
    /// Called when another client sends a message to the room
    /// </summary>
    protected virtual void OnOtherMessageReceived(JObject message)
    {
        if (HUDManager.Instance == null)
            return;

        var content = message.Value<string>("text");

        if (content == null)
            return;
        
        var teamColor = message.Value<string>("player_color").GetTeam().GetHexColor();

        var player = message.GetValue("player")?.Value<string>("name") ?? "???";
        
        HUDManager.Instance.AddTextToChatOnServer($"<color={teamColor}>{player}</color>: <color=#FFFF00>{content}</color>");
    }

    #endregion

    #region API

    public async Task<bool> ChangeTeam(BingoTeam team)
    {
        if (roomId == null)
        {
            Logger.Error("Tried to change team before being connected.");
            return false;
        }
        
        bool success = await BingoAPI.ChangeTeam(roomId, team);
        
        if (success)
            this.team = team;

        return success;
    }

    public async Task<bool> MarkSquare(int id)
    {
        if (roomId == null)
        {
            Logger.Error("Tried to mark a square before being connected.");
            return false;
        }
        
        return await BingoAPI.MarkSquare(roomId, team, id);
    }
    
    public async Task<bool> ClearSquare(int id)
    {
        if (roomId == null)
        {
            Logger.Error("Tried to clear a square before being connected.");
            return false;
        }
        
        return await BingoAPI.ClearSquare(roomId, team, id);
    }

    public async Task<bool> SendMessage(string message)
    {
        if (roomId == null)
        {
            Logger.Error("Tried to send a message before being connected.");
            return false;
        }

        return await BingoAPI.SendMessage(roomId, message);
    }
    
    public async Task<bool> Disconnect()
    {
        await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client disconnecting", CancellationToken.None);

        socket.Dispose();
        return true;
    }

    #endregion

    ~BingoClient()
    {
        _ = Disconnect();
    }
}