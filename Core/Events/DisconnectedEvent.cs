using Newtonsoft.Json.Linq;

namespace LethalBingo.Core.Events;

public class DisconnectedEvent : Event
{
    public readonly string RoomId;
    
    public DisconnectedEvent(JObject json) : base(json)
    {
        RoomId = json.Value<string>("room") ?? "";
    }
}