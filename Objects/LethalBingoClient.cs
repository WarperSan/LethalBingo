using BingoAPI.Bingo;
using BingoAPI.Models;
using BingoAPI.Extensions;

namespace LethalBingo.Objects;

public class LethalBingoClient : BingoClient
{
    #region Events

    /// <inheritdoc />
    protected override void OnOtherMessageReceived(PlayerData player, string content, ulong timestamp)
    {
        if (HUDManager.Instance == null)
            return;

        content = content.Trim();

        if (content.Length == 0)
            return;

        var teamColor = player.Team.GetHexColor();

        HUDManager.Instance.AddTextToChatOnServer(
            $"<color={teamColor}>{player.Name}</color>: <color=#FFFF00>{content}</color>");
    }

    #endregion
}