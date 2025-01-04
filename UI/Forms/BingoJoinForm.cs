using LethalBingo.Helpers;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable CS0649

namespace LethalBingo.UI.Forms
{
    public class BingoJoinForm : MonoBehaviour
    {
        #region Fields

        [Header("Fields")] [SerializeField] private TMP_InputField? roomCode;

        [SerializeField] private TMP_InputField? roomPassword;

        [SerializeField] private TMP_InputField? userNickname;

        [SerializeField] private Toggle? joinAsSpectator;

        [SerializeField] private Button? joinBtn;

        [SerializeField] private Button? openBtn;

        [SerializeField] private Button? closeBtn;

        [SerializeField] private BingoStateForm? stateForm;

        #endregion

        #region Form

        private void SetActiveJoinForm(bool isEnable)
        {
            if (roomCode != null)
                roomCode.interactable = isEnable;

            if (roomPassword != null)
                roomPassword.interactable = isEnable;

            if (userNickname != null)
                userNickname.interactable = isEnable;

            if (joinAsSpectator != null)
                joinAsSpectator.interactable = isEnable;

            if (joinBtn != null)
                joinBtn.interactable = isEnable;
        }

        private void SubmitJoin()
        {
            // Disable all inputs
            SetActiveJoinForm(false);

            var code = roomCode?.text.Trim() ?? "";
            if (code.Length == 0)
            {
                _menuManager?.DisplayMenuNotification("Please enter a valid room link.", "Okay");
                SetActiveJoinForm(true);
                return;
            }

            var password = roomPassword?.text.Trim() ?? "";
            if (password.Length == 0)
            {
                _menuManager?.DisplayMenuNotification("Please enter a valid room password.", "Okay");
                SetActiveJoinForm(true);
                return;
            }

            var nickname = userNickname?.text.Trim() ?? "";
            if (nickname.Length == 0)
            {
                _menuManager?.DisplayMenuNotification("Please enter a valid nickname.", "Okay");
                SetActiveJoinForm(true);
                return;
            }

            var isSpectator = joinAsSpectator?.isOn ?? true;

            TryConnect(code, password, nickname, isSpectator);
        }

        private async void TryConnect(string code, string password, string nickname, bool isSpectator)
        {
            var client = await BingoAPI.JoinRoom(code, password, nickname, isSpectator);

            if (client == null)
            {
                _menuManager?.DisplayMenuNotification("An error has occured while joining the room.", "Okay");
                SetActiveJoinForm(true);
                return;
            }

            LockForm();
            stateForm?.UnlockForm();

            _menuManager?.DisplayMenuNotification("You successfully joined the room.", "Okay");
            LethalBingo.CurrentClient = client;
        }

        #endregion

        #region Animations

        [Header("Animations")] [SerializeField]
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

            joinBtn?.onClick.AddListener(SubmitJoin);

            if (userNickname != null && SteamClient.IsValid)
                userNickname.text = new Friend(SteamClient.SteamId.Value).Name;
        }
    }
}