using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using LethalBingo.Core;
using LethalBingo.Core.Data;
using LethalBingo.Core.Events;
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
    // Self
    public static readonly UnityEvent<string?, PlayerData> OnSelfConnected = new();
    public static readonly UnityEvent OnSelfDisconnected = new();
    public static readonly UnityEvent<PlayerData, SquareData> OnSelfMarked = new();
    public static readonly UnityEvent<PlayerData, SquareData> OnSelfCleared = new();
    public static readonly UnityEvent<PlayerData, string, ulong> OnSelfChatted = new();
    public static readonly UnityEvent<PlayerData, BingoTeam, BingoTeam> OnSelfTeamChanged = new();
    
    // Other
    public static readonly UnityEvent<string?, PlayerData> OnOtherConnected = new();
    public static readonly UnityEvent<string?, PlayerData> OnOtherDisconnected = new();
    public static readonly UnityEvent<PlayerData, SquareData> OnOtherMarked = new();
    public static readonly UnityEvent<PlayerData, SquareData> OnOtherCleared = new();
    public static readonly UnityEvent<PlayerData, string, ulong> OnOtherChatted = new();
    public static readonly UnityEvent<PlayerData, BingoTeam, BingoTeam> OnOtherTeamChanged = new();
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

        var _event = Event.ParseEvent(json);
        
        if (_event == null)
            return;

        switch (_event)
        {
            case ConnectedEvent _connected:
                HandleConnectedEvent(_connected);
                break;
            case DisconnectedEvent _disconnected:
                HandleDisconnectedEvent(_disconnected);
                break;
            case ChatEvent _chat:
                HandleChatEvent(_chat);
                break;
            case ColorEvent _color:
                HandleColorEvent(_color);
                break;
            case GoalEvent _goal:
                if (_goal.Remove)
                    HandleClearedEvent(_goal);
                else
                    HandleMarkedEvent(_goal);
                break;
        }
    }

    private void HandleConnectedEvent(ConnectedEvent @event)
    {
        if (roomId == null)
        {
            OnSelfConnect(@event.RoomId, @event.Player);
            OnSelfConnected.Invoke(@event.RoomId, @event.Player);
        }
        else
        {
            OnOtherConnect(@event.RoomId, @event.Player);
            OnOtherConnected.Invoke(@event.RoomId, @event.Player);
        }
    }

    private void HandleDisconnectedEvent(DisconnectedEvent @event)
    {
        if (roomId != null)
        {
            OnOtherDisconnect(@event.RoomId, @event.Player);
            OnOtherDisconnected.Invoke(@event.RoomId, @event.Player);
        }
    }
    
    private void HandleChatEvent(ChatEvent @event)
    {
        if (PlayerData.UUID == @event.Player.UUID)
        {
            OnSelfMessageReceived(@event.Text, @event.Timestamp);
            OnSelfChatted.Invoke(@event.Player, @event.Text, @event.Timestamp);
        }
        else
        {
            OnOtherMessageReceived(@event.Player, @event.Text, @event.Timestamp);
            OnOtherChatted.Invoke(@event.Player, @event.Text, @event.Timestamp);
        }
    }

    private void HandleColorEvent(ColorEvent @event)
    {
        var oldTeam = PlayerData.Team;

        if (PlayerData.UUID == @event.Player.UUID)
        {
            OnSelfTeamChange(oldTeam, @event.Player.Team);
            OnSelfTeamChanged.Invoke(@event.Player, oldTeam, @event.Player.Team);
        }
        else
        {
            OnOtherTeamChange(@event.Player, oldTeam, @event.Player.Team);
            OnOtherTeamChanged.Invoke(@event.Player, oldTeam, @event.Player.Team);
        }
    }

    private void HandleMarkedEvent(GoalEvent @event)
    {
        if (PlayerData.UUID == @event.Player.UUID)
        {
            OnSelfMark(@event.Square);
            OnSelfMarked.Invoke(@event.Player, @event.Square);
        }
        else
        {
            OnOtherMark(@event.Player, @event.Square);
            OnOtherMarked.Invoke(@event.Player, @event.Square);
        }
    }

    private void HandleClearedEvent(GoalEvent @event)
    {
        if (PlayerData.UUID == @event.Player.UUID)
        {
            OnSelfClear(@event.Square);
            OnSelfCleared.Invoke(@event.Player, @event.Square);
        }
        else
        {
            OnOtherClear(@event.Player, @event.Square);
            OnOtherCleared.Invoke(@event.Player, @event.Square);
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
        roomId = null;
        PlayerData = new PlayerData
        {
            UUID = null,
            Name = null,
            Team = BingoTeam.BLANK,
            IsSpectator = true
        };
        
        LethalBingo.CurrentClient = null;
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
    protected virtual void OnOtherMark(PlayerData player, SquareData square) { /* DO NOTHING */ }

    /// <summary>
    /// Called when this client clears a square
    /// </summary>
    protected virtual void OnSelfClear(SquareData square) { /* DO NOTHING */ }

    /// <summary>
    /// Called when another client clears a square
    /// </summary>
    protected virtual void OnOtherClear(PlayerData player, SquareData square) { /* DO NOTHING */ }
    
    /// <summary>
    /// Called when this client sends a message to the room
    /// </summary>
    protected virtual void OnSelfMessageReceived(string content, ulong timestamp) { /* DO NOTHING */ }
    
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
    protected virtual void OnOtherTeamChange(PlayerData player, BingoTeam oldTeam, BingoTeam newTeam) { /* DO NOTHING */ }
    
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
        if (roomId == null)
            return true;
        
        Logger.Debug($"Disconnecting client for the room '{roomId}'...");

        try
        {
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client disconnecting", CancellationToken.None);
        }
        catch (Exception e)
        {
            Logger.Error(e.Message);
        }

        socket.Dispose();

        OnSelfDisconnect();
        OnSelfDisconnected.Invoke();
        
        Logger.Debug("Client disconnected!");
        
        return true;
    }

    #endregion
}