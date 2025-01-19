using LethalBingo.Core.Data;
using Newtonsoft.Json.Linq;

namespace LethalBingo.Core.Events;

public class GoalEvent : Event
{
    public readonly SquareData Square;
    public readonly bool Remove;
    
    public GoalEvent(JObject json) : base(json)
    {
        var goal = json.GetValue("square");
        Square = SquareData.ParseJSON(goal);
        Remove = goal?.Value<bool>("remove") ?? false;
    }
}