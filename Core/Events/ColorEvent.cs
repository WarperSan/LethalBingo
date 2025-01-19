using Newtonsoft.Json.Linq;

namespace LethalBingo.Core.Events;

public class ColorEvent : Event
{
    public ColorEvent(JObject json) : base(json)
    {
    }
}