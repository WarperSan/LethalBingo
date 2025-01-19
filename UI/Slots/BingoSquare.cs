using LethalBingo.Core;
using UnityEngine;

namespace LethalBingo.UI.Slots;

public abstract class BingoSquare : MonoBehaviour
{
    #region Text

    public abstract void DisplayText(string text);

    #endregion

    #region Markings

    /// <summary>
    /// Caches the marking for all the given teams
    /// </summary>
    public virtual void CacheMarkings(BingoTeam[] teams)
    {
    }

    /// <summary>
    /// Sets this slot to have the given teams selected
    /// </summary>
    public abstract void SetTeams(BingoTeam[] teams);

    #endregion
}