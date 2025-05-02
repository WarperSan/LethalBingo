using System.Collections.Generic;
using BingoAPI.Data;
using BingoAPI.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable CS0649

namespace LethalBingo.UI.Slots;

public class StandardBingoSquare : BingoSquare
{
    #region Fields

    [Header("Fields")] [SerializeField] private TextMeshProUGUI? _text;

    [SerializeField] private Transform? _markings;

    [SerializeField] private GameObject? _markingPrefab;

    #endregion

    #region BingoSquare

    private Dictionary<BingoTeam, GameObject?>? cachedMarkings;

    /// <inheritdoc />
    public override void DisplayText(string text)
    {
        _text?.SetText(text);
    }

    /// <inheritdoc />
    public override void CacheMarkings(BingoTeam[] teams)
    {
        cachedMarkings = [];

        foreach (var team in teams)
        {
            var newMark = Instantiate(_markingPrefab, _markings);

            cachedMarkings[team] = newMark;

            if (newMark == null)
                continue;

            newMark.name = team.ToString();
            newMark.SetActive(false);
            newMark.GetComponent<Image>().color = team.GetColor();
        }
    }

    /// <inheritdoc />
    public override void SetTeams(BingoTeam[] teams)
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