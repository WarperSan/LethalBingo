using System.Net.WebSockets;
using BingoAPI;
using BingoAPI.Models;
using BingoAPI.Extensions;

namespace LethalBingo.Objects;

// ReSharper disable once ClassNeverInstantiated.Global
public class LethalBingoClient : BingoClient
{
    public LethalBingoClient(ClientWebSocket socket, bool isCreator) : base(socket, isCreator)
    {
    }

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