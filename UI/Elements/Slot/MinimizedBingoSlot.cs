using System.Collections.Generic;
using BingoAPI.Models;
using LethalBingo.UI.Elements.Marking;
using UnityEngine;

#pragma warning disable CS0649

namespace LethalBingo.UI.Elements.Slot;

public class MinimizedBingoSlot : BaseBingoSlot
{
    #region Fields

    [Header("Fields")]

    [SerializeField] private Transform? _markings;

    [SerializeField] private GameObject? _markingPrefab;
    
    #endregion

    #region BaseBingoSlot

    private Dictionary<Team, BaseBingoMarking?>? cachedMarkings;

    /// <inheritdoc />
    public override void CacheMarkings(Dictionary<Team, TeamIconInfo>? teams)
    {
        cachedMarkings = [];

        if (teams == null || teams.Count == 0)
            return;
        
        foreach (var (team, teamInfo) in teams)
        {
            var newMark = Instantiate(_markingPrefab, _markings);

            if (newMark == null)
                continue;

            newMark.name = team.ToString();

            if (newMark.TryGetComponent(out BaseBingoMarking markingElement))
            {
                markingElement.SetIcon(teamInfo.Icon);
                markingElement.SetColor(team);

                cachedMarkings[team] = markingElement;
            }
        }
    }

    /// <inheritdoc />
    public override void SetTeams(Team[] teams)
    {
        if (cachedMarkings == null)
            return;

        // Disable all markings
        foreach (var (_, o) in cachedMarkings)
            o?.SetActive(false);

        // If blank, skip
        if (teams.Length == 0)
            return;

        // Enable active markins
        foreach (var team in teams)
        {
            if (!cachedMarkings.TryGetValue(team, out var o) || o == null)
                continue;

            o.SetActive(true);
        }
    }

    #endregion
}