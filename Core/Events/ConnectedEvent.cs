using Newtonsoft.Json.Linq;

namespace LethalBingo.Core.Events;

public class ConnectedEvent : Event
{
    public readonly string RoomId;
    
    public ConnectedEvent(JObject json) : base(json)
    {
        RoomId = json.Value<string>("room") ?? "";
    }
}