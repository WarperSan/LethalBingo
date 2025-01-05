using LethalBingo.Extensions;
using LethalBingo.Objects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable CS0649

namespace LethalBingo.UI.Forms
{
    public class ColorElement : MonoBehaviour
    {
        #region Fields

        [Header("Fields")] 
        [SerializeField] private Graphic? coloringElement;
        [SerializeField] private TextMeshProUGUI? text;
        [SerializeField] private Button? button;
        [SerializeField] private Graphic? darkenElement;

        #endregion

        #region Setters

        private BingoTeam team;

        public BingoTeam Team
        {
            set
            {
                team = value;
                SetText(team);
                SetColor(team);
            }
        }

        private void SetText(BingoTeam _team)
        {
            string teamName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(_team.ToString().ToLower());
            text?.SetText(teamName);
        }
        
        private void SetColor(BingoTeam _team)
        {
            if (coloringElement != null)
                coloringElement.color = _team.GetColor();
        }

        private void Start()
        {
            button?.onClick.AddListener(() => LethalBingo.CurrentClient?.ChangeTeam(team));
        }

        #endregion

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
    }
}