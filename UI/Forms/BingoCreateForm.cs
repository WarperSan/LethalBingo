using LethalBingo.Core;
using LethalBingo.Core.Data;
using LethalBingo.Objects;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable CS0649

namespace LethalBingo.UI.Forms;

public class BingoCreateForm : MonoBehaviour
{
    #region Fields

        [Header("Fields")] [SerializeField] private TMP_InputField? roomName;

        [SerializeField] private TMP_InputField? roomPassword;

        [SerializeField] private TMP_InputField? userNickname;

        [SerializeField] private Toggle? joinAsSpectator;
        
        [SerializeField] private Toggle? randomized;
        
        [SerializeField] private Toggle? lockout;
        
        [SerializeField] private Toggle? hideCard;
        
        [SerializeField] private TMP_InputField? seed;
        
        [SerializeField] private TMP_InputField? boardJson;

        [SerializeField] private Button? createBtn;

        [SerializeField] private Button? openBtn;

        [SerializeField] private Button? closeBtn;

        #endregion

        #region Form

        private void SetActiveCreateForm(bool isEnable)
        {
            Selectable?[] fields =
            [
                roomName,
                roomPassword,
                userNickname,
                joinAsSpectator,
                randomized,
                lockout,
                hideCard,
                seed,
                boardJson,
                createBtn
            ];

            foreach (var field in fields)
            {
                if (field != null)
                    field.interactable = isEnable;
            }
        }

        private void SubmitCreate()
        {
            // Disable all inputs
            SetActiveCreateForm(false);

            var _name = roomName?.text.Trim() ?? "";
            if (_name.Length == 0)
            {
                _menuManager?.DisplayMenuNotification("Please enter a valid room name.", "Okay");
                SetActiveCreateForm(true);
                return;
            }
            
            var password = roomPassword?.text.Trim() ?? "";
            if (password.Length == 0)
            {
                _menuManager?.DisplayMenuNotification("Please enter a valid room password.", "Okay");
                SetActiveCreateForm(true);
                return;
            }
            
            var nickName = userNickname?.text.Trim() ?? "";
            if (nickName.Length == 0)
            {
                _menuManager?.DisplayMenuNotification("Please enter a valid nickname.", "Okay");
                SetActiveCreateForm(true);
                return;
            }

            var isRandomized = randomized?.isOn ?? false;
            
            var _boardJSON = boardJson?.text.Trim() ?? "";
            if (_boardJSON.Length == 0)
            {
                _menuManager?.DisplayMenuNotification("Please enter a valid JSON board.", "Okay");
                SetActiveCreateForm(true);
                return;
            }
            
            var isLockout = lockout?.isOn ?? false;

            var _seed = seed?.text.Trim() ?? "";
            
            var isSpectator = joinAsSpectator?.isOn ?? false;

            var _hideCard = hideCard?.isOn ?? false;

            TryCreate(_name, password, nickName, isRandomized, _boardJSON, isLockout, _seed, isSpectator, _hideCard);
        }

        private async void TryCreate(
            string _name, 
            string password, 
            string nickName, 
            bool isRandomized,
            string boardJSON,
            bool isLockout,
            string _seed,
            bool isSpectator,
            bool _hideCard)
        {
            bool success = await BingoAPI.CreateRoom(
                _name, 
                password, 
                nickName, 
                isRandomized,
                boardJSON, 
                isLockout,
                _seed, isSpectator, 
                _hideCard
            );
            
            SetActiveCreateForm(true);
        
            if (success)
                return;
            
            _menuManager?.DisplayMenuNotification("An error has occured while creating the room.", "Okay");
        }

        #endregion

        #region Animations

        [Header("Animations")] [SerializeField]
        private Animator? animator;

        private void OpenForm() => animator?.SetTrigger(OpenMenu);
        private void CloseForm() => animator?.SetTrigger(CloseMenu);

        private static readonly int OpenMenu = Animator.StringToHash("openMenu");
        private static readonly int CloseMenu = Animator.StringToHash("closeMenu");

        #endregion

        #region Events

        private void OnConnected(string? roomId, PlayerData player)
        {
            CloseForm();
            openBtn?.gameObject.SetActive(false);
            
            _menuManager?.DisplayMenuNotification("You successfully created the room.", "Okay");
        }

        private void OnDisconnected()
        {
            //OpenForm();
            openBtn?.gameObject.SetActive(true);
        }

        #endregion

        private MenuManager? _menuManager;

        private void Start()
        {
            _menuManager = FindObjectOfType<MenuManager>();

            openBtn?.onClick.AddListener(OpenForm);
            openBtn?.onClick.AddListener(_menuManager.PlayConfirmSFX);

            closeBtn?.onClick.AddListener(CloseForm);
            closeBtn?.onClick.AddListener(_menuManager.PlayCancelSFX);

            createBtn?.onClick.AddListener(SubmitCreate);

            if (userNickname != null && SteamClient.IsValid)
                userNickname.text = new Friend(SteamClient.SteamId.Value).Name;
        }

        private void OnEnable()
        {
            BingoClient.OnSelfConnected.AddListener(OnConnected);
            BingoClient.OnSelfDisconnected.AddListener(OnDisconnected);
        }

        private void OnDisable()
        {
            BingoClient.OnSelfConnected.RemoveListener(OnConnected);
            BingoClient.OnSelfDisconnected.RemoveListener(OnDisconnected);
        }
    }