using UnityEngine;
using UnityEngine.UI;

#pragma warning disable CS0649

namespace LethalBingo.UI.Forms
{
    public class BingoStateForm : MonoBehaviour
    {
        #region Fields

        [Header("Fields")]
        [SerializeField]
        private Button? leaveBtn;

        [SerializeField] 
        private Button? openBtn;

        [SerializeField] 
        private Button? closeBtn;

        [SerializeField] 
        private BingoJoinForm? joinForm;
        
        #endregion

        #region Form
        
        private void SetActiveJoinForm(bool isEnable)
        {
            if (leaveBtn != null)
                leaveBtn.interactable = isEnable;
        }

        private void SubmitLeave()
        {
            // Disable all inputs
            SetActiveJoinForm(false);

            TryDisconnect();
        }

        private async void TryDisconnect()
        {
            if (LethalBingo.CurrentClient != null)
            {
                if (!await LethalBingo.CurrentClient.Disconnect())
                {
                    _menuManager?.DisplayMenuNotification("An error has occured while leaving the room.", "Okay");
                    SetActiveJoinForm(true);
                    return;
                }
            }

            _menuManager?.DisplayMenuNotification("You successfully left the room.", "Thank you");
            LethalBingo.CurrentClient = null;
            
            LockForm();
            joinForm?.UnlockForm();
        }

        #endregion

        #region Animations
        
        [Header("Animations")]
        [SerializeField]
        private Animator? animator;

        private void OpenForm() => animator?.SetTrigger(OpenMenu);

        public void UnlockForm()
        {
            OpenForm();
            openBtn?.gameObject.SetActive(true);
        }
        
        
        private void CloseForm() => animator?.SetTrigger(CloseMenu);
        public void LockForm()
        {
            CloseForm();
            openBtn?.gameObject.SetActive(false);
        }

        private static readonly int OpenMenu = Animator.StringToHash("openMenu");
        private static readonly int CloseMenu = Animator.StringToHash("closeMenu");

        #endregion
        
        private MenuManager? _menuManager;
        
        private void Start()
        {
            _menuManager = FindObjectOfType<MenuManager>();
            
            openBtn?.onClick.AddListener(OpenForm);
            openBtn?.onClick.AddListener(_menuManager.PlayConfirmSFX);
            
            closeBtn?.onClick.AddListener(CloseForm);
            closeBtn?.onClick.AddListener(_menuManager.PlayCancelSFX);
            
            leaveBtn?.onClick.AddListener(SubmitLeave);
        }
    }
}