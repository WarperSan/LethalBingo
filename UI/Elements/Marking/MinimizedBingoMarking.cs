using BingoAPI.Extensions;
using BingoAPI.Models;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable CS0649

namespace LethalBingo.UI.Elements.Marking;

public class MinimizedBingoMarking : BaseBingoMarking
{
    #region Fields

    [SerializeField] private Image? coloredIcon;

    #endregion

    /// <inheritdoc/>
    public override void SetIcon(Sprite? sprite)
    {
        if (coloredIcon != null)
            coloredIcon.sprite = sprite;
    }

    /// <inheritdoc/>
    public override void SetColor(Team team)
    {
        if (coloredIcon != null)
            coloredIcon.color = team.GetColor();
    }

    /// <inheritdoc/>
    public override void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }
}