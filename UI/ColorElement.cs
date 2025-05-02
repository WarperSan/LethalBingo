using System.Globalization;
using BingoAPI.Models;
using BingoAPI.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable CS0649

namespace LethalBingo.UI;

public class ColorElement : MonoBehaviour
{
    private void OnEnable()
    {
        if (button != null)
            button.interactable = true;

        if (darkenElement != null)
        {
            var c = darkenElement.color;
            c.a = 125 / 255f;
            darkenElement.color = c;
        }
    }

    private void OnDisable()
    {
        if (button != null)
            button.interactable = false;

        if (darkenElement != null)
        {
            var c = darkenElement.color;
            c.a = 208 / 255f;
            darkenElement.color = c;
        }
    }

    #region Fields

    [Header("Fields")] [SerializeField] private Graphic? coloringElement;

    [SerializeField] private TextMeshProUGUI? text;
    [SerializeField] private Button? button;
    [SerializeField] private Graphic? darkenElement;

    #endregion

    #region Setters

    private Team team;

    public Team Team
    {
        set
        {
            team = value;
            SetText(team);
            SetColor(team);
        }
    }

    private void SetText(Team _team)
    {
        var teamName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(_team.ToString().ToLower());
        text?.SetText(teamName);
    }

    private void SetColor(Team _team)
    {
        if (coloringElement != null)
            coloringElement.color = _team.GetColor();
    }

    private void Start()
    {
        button?.onClick.AddListener(() => LethalBingo.CurrentClient?.ChangeTeam(team));
    }

    #endregion
}