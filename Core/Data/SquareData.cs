using LethalBingo.Extensions;
using Newtonsoft.Json.Linq;

namespace LethalBingo.Core.Data;

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