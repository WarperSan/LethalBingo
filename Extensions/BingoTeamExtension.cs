using System;
using LethalBingo.Objects;

namespace LethalBingo.Extensions;

public static class BingoTeamExtension
{
    /// <summary>
    /// Fetches the name of the given team
    /// </summary>
    public static string GetName(this BingoTeam team) => team.ToString().ToLower();

    /// <summary>
    /// Fetches the team with the given name
    /// </summary>
    public static BingoTeam GetTeam(this string? name)
    {
        if (name == null)
            return BingoTeam.BLANK;

        return Enum.TryParse(name.ToUpper(), out BingoTeam team) ? team : BingoTeam.BLANK;
    }

    /// <summary>
    /// Fetches the HEX color of the given team
    /// </summary>
    public static string GetHexColor(this BingoTeam team)
    {
        switch (team)
        {
            case BingoTeam.PINK:
                return "#FFC0CB";
            case BingoTeam.RED:
                return "#FF0000";
            case BingoTeam.ORANGE:
                return "#FFA500";
            case BingoTeam.BROWN:
                return "#964B00";
            case BingoTeam.YELLOW:
                return "#FFFF00";
            case BingoTeam.GREEN:
                return "#00FF00";
            case BingoTeam.TEAL:
                return "#66b2b2";
            case BingoTeam.BLUE:
                return "#00f1ff";
            case BingoTeam.NAVY:
                return "#7d90ff";
            case BingoTeam.PURPLE:
                return "#800080";
            case BingoTeam.BLANK:
            default:
                return "#FFFFFF";
        }
    }
}