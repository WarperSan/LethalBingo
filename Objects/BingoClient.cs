using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using LethalBingo.Extensions;
using LethalBingo.Helpers;
using Newtonsoft.Json.Linq;
using UnityEngine.Events;

namespace LethalBingo.Objects;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public class BingoClient
{
    public string? roomId { get; private set; }
    public PlayerData PlayerData { get; protected set; }
    public readonly bool IsCreator;

    internal BingoClient(ClientWebSocket socket, bool isCreator)
    {
        IsCreator = isCreator;
        roomId = null;
        PlayerData = new PlayerData
        {
            UUID = null,
            Name = null,
            Team = BingoTeam.BLANK,
            IsSpectator = true
        };
        this.socket = socket;

        _ = socket.HandleMessages(OnSocketReceived);
    }
    
    ~BingoClient() => _ = Disconnect();

    #region Socket

    private readonly ClientWebSocket socket;

    // ReSharper disable ArrangeObjectCreationWhenTypeNotEvident
    public static readonly UnityEvent<string?, PlayerData> OnSelfConnected = new();
    public static readonly UnityEvent<string?, PlayerData> OnOtherConnected = new();
    public static readonly UnityEvent OnSelfDisconnected = new();
    public static readonly UnityEvent<string?, PlayerData> OnOtherDisconnected = new();
    public static readonly UnityEvent<SquareData> OnSelfMarked = new();
    public static readonly UnityEvent<SquareData> OnOtherMarked = new();
    public static readonly UnityEvent<SquareData> OnSelfCleared = new();
    public static readonly UnityEvent<SquareData> OnOtherCleared = new();
    public static readonly UnityEvent<PlayerData, string, ulong> OnSelfChatted = new();
    public static readonly UnityEvent<PlayerData, string, ulong> OnOtherChatted = new();
    public static readonly UnityEvent<BingoTeam, BingoTeam> OnSelfTeamChanged = new();
    public static readonly UnityEvent<BingoTeam, BingoTeam> OnOtherTeamChanged = new();
    // ReSharper restore ArrangeObjectCreationWhenTypeNotEvident

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
        var playerData = PlayerData.ParseJSON(json.GetValue("player"));

        switch (type)
        {
            case "connection":
                var eventType = json.Value<string>("event_type");
                
                switch (eventType)
                {
                    case "connected":
                        HandleConnectedEvent(json, playerData);
                        break;
                    case "disconnected":
                        HandleDisconnectedEvent(json, playerData);
                        break;
                }
                return;
            case "chat":
                HandleChatEvent(json, playerData);
                return;
            case "color":
                HandleColorEvent(playerData);
                return;
            case "goal":
                var goal = json.GetValue("square");
                var remove = goal?.Value<bool>("remove") ?? false;
                var squareData = SquareData.ParseJSON(goal);
                
                if (remove)
                    HandleClearedEvent(squareData, playerData);
                else
                    HandleMarkedEvent(squareData, playerData);
                return;
            default:
                Logger.Error($"Unhandled response: {json}");
                break;
        }
    }

    private void HandleConnectedEvent(JObject connected, PlayerData data)
    {
        var _roomId = connected.Value<string>("room");
        
        if (roomId == null)
        {
            OnSelfConnect(_roomId, data);
            OnSelfConnected.Invoke(_roomId, data);
        }
        else
        {
            OnOtherConnect(_roomId, data);
            OnOtherConnected.Invoke(_roomId, data);
        }
    }

    private void HandleDisconnectedEvent(JObject disconnected, PlayerData data)
    {
        var _roomId = disconnected.Value<string>("room");
        
        if (roomId != null)
        {
            OnOtherDisconnect(_roomId, data);
            OnOtherDisconnected.Invoke(_roomId, data);
        }
    }
    
    private void HandleChatEvent(JObject message, PlayerData data)
    {
        var content = message.Value<string>("text") ?? "";
        var timestamp = message.Value<ulong>("timestamp");
        
        if (PlayerData.UUID == data.UUID)
        {
            OnSelfMessageReceived(data, content, timestamp);
            OnSelfChatted.Invoke(data, content, timestamp);
        }
        else
        {
            OnOtherMessageReceived(data, content, timestamp);
            OnOtherChatted.Invoke(data, content, timestamp);
        }
    }

    private void HandleColorEvent(PlayerData data)
    {
        var newColor = data.Team;
        var oldTeam = PlayerData.Team;

        if (PlayerData.UUID == data.UUID)
        {
            OnSelfTeamChange(oldTeam, newColor);
            OnSelfTeamChanged.Invoke(oldTeam, newColor);
        }
        else
        {
            OnSelfTeamChange(oldTeam, newColor);
            OnOtherTeamChanged.Invoke(oldTeam, newColor);
        }
    }

    private void HandleMarkedEvent(SquareData square, PlayerData player)
    {
        if (PlayerData.UUID == player.UUID)
        {
            OnSelfMark(square);
            OnSelfMarked.Invoke(square);
        }
        else
        {
            OnOtherMark(square);
            OnOtherMarked.Invoke(square);
        }
    }

    private void HandleClearedEvent(SquareData square, PlayerData player)
    {
        if (PlayerData.UUID == player.UUID)
        {
            OnSelfClear(square);
            OnSelfCleared.Invoke(square);
        }
        else
        {
            OnOtherClear(square);
            OnOtherCleared.Invoke(square);
        }
    }
    
    #endregion

    #region Callbacks

    /// <summary>
    /// Called when this client gets connected to the room
    /// </summary>
    protected virtual void OnSelfConnect(string? _roomId, PlayerData player)
    {
        roomId = _roomId;
        PlayerData = player;
    }

    /// <summary>
    /// Called when another client gets connected to the room
    /// </summary>
    protected virtual void OnOtherConnect(string? _roomId, PlayerData player) { /* DO NOTHING */ }

    /// <summary>
    /// Called when this client gets disconnected to the room
    /// </summary>
    protected virtual void OnSelfDisconnect()
    {
        LethalBingo.CurrentClient = null;
        roomId = null;
        PlayerData = new PlayerData
        {
            UUID = null,
            Name = null,
            Team = BingoTeam.BLANK,
            IsSpectator = true
        };
    }
    
    /// <summary>
    /// Called when another client gets disconnected to the room
    /// </summary>
    protected virtual void OnOtherDisconnect(string? _roomId, PlayerData player) { /* DO NOTHING */ }

    /// <summary>
    /// Called when this client marks a square
    /// </summary>
    protected virtual void OnSelfMark(SquareData square) { /* DO NOTHING */ }

    /// <summary>
    /// Called when another client marks a square
    /// </summary>
    protected virtual void OnOtherMark(SquareData square) { /* DO NOTHING */ }

    /// <summary>
    /// Called when this client clears a square
    /// </summary>
    protected virtual void OnSelfClear(SquareData square) { /* DO NOTHING */ }

    /// <summary>
    /// Called when another client clears a square
    /// </summary>
    protected virtual void OnOtherClear(SquareData square) { /* DO NOTHING */ }
    
    /// <summary>
    /// Called when this client sends a message to the room
    /// </summary>
    protected virtual void OnSelfMessageReceived(PlayerData player, string content, ulong timestamp) { /* DO NOTHING */ }
    
    /// <summary>
    /// Called when another client sends a message to the room
    /// </summary>
    protected virtual void OnOtherMessageReceived(PlayerData player, string content, ulong timestamp)
    {
        if (HUDManager.Instance == null)
            return;

        content = content.Trim();

        if (content.Length == 0)
            return;

        var teamColor = player.Team.GetHexColor();
        
        HUDManager.Instance.AddTextToChatOnServer($"<color={teamColor}>{player.Name}</color>: <color=#FFFF00>{content}</color>");
    }

    /// <summary>
    /// Called when this client changes team
    /// </summary>
    protected virtual void OnSelfTeamChange(BingoTeam oldTeam, BingoTeam newTeam)
    {
        var data = PlayerData;
        data.Team = newTeam;
        PlayerData = data;
    }
    
    /// <summary>
    /// Called when another client changes team
    /// </summary>
    protected virtual void OnOtherTeamChange(BingoTeam oldTeam, BingoTeam newTeam) { /* DO NOTHING */ }
    
    #endregion

    #region API

    public async Task<SquareData[]?> GetBoard()
    {
        if (roomId == null)
        {
            Logger.Error("Tried to obtain the board before being connected.");
            return null;
        }

        return await BingoAPI.GetBoard(roomId);
    }
    
    public async Task ChangeTeam(BingoTeam newTeam)
    {
        if (roomId == null)
        {
            Logger.Error("Tried to change team before being connected.");
            return;
        }

        if (!await BingoAPI.ChangeTeam(roomId, newTeam))
            return;

        var data = PlayerData;
        data.Team = newTeam;
        PlayerData = data;
    }

    public async Task<bool> MarkSquare(int id)
    {
        if (roomId == null)
        {
            Logger.Error("Tried to mark a square before being connected.");
            return false;
        }
        
        return await BingoAPI.MarkSquare(roomId, PlayerData.Team, id);
    }
    
    public async Task<bool> ClearSquare(int id)
    {
        if (roomId == null)
        {
            Logger.Error("Tried to clear a square before being connected.");
            return false;
        }
        
        return await BingoAPI.ClearSquare(roomId, PlayerData.Team, id);
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

        OnSelfDisconnect();
        OnSelfDisconnected.Invoke();
        
        socket.Dispose();
        return true;
    }

    #endregion
}