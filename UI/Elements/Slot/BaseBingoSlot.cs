using System.Collections.Generic;
using BingoAPI.Models;
using UnityEngine;

namespace LethalBingo.UI.Elements.Slot;

public abstract class BaseBingoSlot : MonoBehaviour
{
    #region Markings

    /// <summary>
    ///     Caches the marking for all the given teams
    /// </summary>
    public virtual void CacheMarkings(Dictionary<Team, TeamIconInfo>? teams)
    {
    }

    /// <summary>
    ///     Sets this slot to have the given teams selected
    /// </summary>
    public abstract void SetTeams(Team[] teams);

    #endregion
}