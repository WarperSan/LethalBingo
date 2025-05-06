using BingoAPI.Models;
using UnityEngine;

namespace LethalBingo.UI.Elements.Marking;

public abstract class BaseBingoMarking : MonoBehaviour
{
    /// <summary>
    ///     Sets the icon of this marking to the given sprite
    /// </summary>
    public abstract void SetIcon(Sprite? sprite);
    
    /// <summary>
    ///     Sets the color of this marking based on the given team
    /// </summary>
    public abstract void SetColor(Team team);
}