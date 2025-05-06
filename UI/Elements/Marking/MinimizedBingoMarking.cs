using BingoAPI.Extensions;
using BingoAPI.Models;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable CS0649

namespace LethalBingo.UI.Elements.Marking;

public class MinimizedBingoMarking : BaseBingoMarking
{
    #region Fields

    [Header("Fields")]

    [SerializeField] private Image? _icon;

    #endregion

    /// <inheritdoc/>
    public override void SetIcon(Sprite? sprite)
    {
        if (_icon == null)
            return;

        _icon.sprite = sprite;
    }

    /// <inheritdoc/>
    public override void SetColor(Team team)
    {
        if (_icon == null)
            return;

        _icon.color = team.GetColor();
    }
}