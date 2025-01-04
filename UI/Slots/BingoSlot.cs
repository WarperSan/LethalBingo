using UnityEngine;

namespace LethalBingo.UI.Slots
{
    public abstract class BingoSlot : MonoBehaviour
    {
        #region ID

        private int id;

        public int Id
        {
            get => id;
            set
            {
                gameObject.name = name + $"({id})";
                id = value;
            }
        }

        #endregion

        #region Text

        private string text = "";

        public string Text
        {
            get => text;
            set
            {
                DisplayText(text);
                text = value;
            }
        }

        protected abstract void DisplayText(string text);

        #endregion 
    }
}