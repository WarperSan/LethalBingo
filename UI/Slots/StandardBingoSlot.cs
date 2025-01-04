using TMPro;
using UnityEngine;

namespace LethalBingo.UI.Slots
{
    public class StandardBingoSlot : BingoSlot
    {
        #region Fields

        [Header("Fields")]
        [SerializeField]
        private TextMeshProUGUI _text;

        // Markings

        #endregion

        #region BingoSlot

        /// <inheritdoc/>
        protected override void DisplayText(string text)
        {
            _text.SetText(text);
        }

        #endregion
    }
}