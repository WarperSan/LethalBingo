using LethalBingo.Core.Data;
using LethalBingo.Extensions;
using LethalBingo.Helpers;
using Newtonsoft.Json.Linq;

namespace LethalBingo.Core.Events;

public abstract class Event
{
    public readonly PlayerData Player;
    public readonly BingoTeam Team;
    public readonly ulong Timestamp;

    internal Event(JObject json)
    {
        Player = PlayerData.ParseJSON(json.GetValue("player"));
        Team = json.Value<string>("player_color").GetTeam();
        Timestamp = json.Value<ulong>("timestamp");
    }

    public static Event? ParseEvent(JObject json)
    {
        Logger.Debug(json);

        var type = json.Value<string>("type");
        
        switch (type)
        {
            case "connection":
                var eventType = json.Value<string>("event_type");
                
                switch (eventType)
                {
                    case "connected":
                        return new ConnectedEvent(json);
                    case "disconnected":
                        return new DisconnectedEvent(json);
                }
                break;
            case "chat":
                return new ChatEvent(json);
            case "color":
                return new ColorEvent(json);
            case "goal":
                return new GoalEvent(json);
        }
        
        Logger.Error($"Unhandled response: {json}");
        return null;
    }
}