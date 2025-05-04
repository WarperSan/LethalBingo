using BingoAPI.Managers;
using BingoAPI.Models;
using BingoAPI.Models.Settings;
using LethalBingo.Objects;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable CS0649

namespace LethalBingo.UI.Forms;

public class BingoJoinForm : MonoBehaviour
{
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

    private void OnEnable()
    {
        ClientEventManager.OnSelfConnected.AddListener(OnConnected);
        ClientEventManager.OnSelfDisconnected.AddListener(OnDisconnected);
    }

    private void OnDisable()
    {
        ClientEventManager.OnSelfConnected.RemoveListener(OnConnected);
        ClientEventManager.OnSelfDisconnected.RemoveListener(OnDisconnected);
    }

    #region Fields

    [Header("Fields")] [SerializeField] private TMP_InputField? roomCode;

    [SerializeField] private TMP_InputField? roomPassword;

    [SerializeField] private TMP_InputField? userNickname;

    [SerializeField] private Toggle? joinAsSpectator;

    [SerializeField] private Button? joinBtn;

    [SerializeField] private Button? openBtn;

    [SerializeField] private Button? closeBtn;

    #endregion

    #region Form

    private void SetActiveJoinForm(bool isEnable)
    {
        Selectable?[] fields =
        [
            roomCode,
            roomPassword,
            userNickname,
            joinAsSpectator,
            joinBtn
        ];

        foreach (var field in fields)
            if (field != null)
                field.interactable = isEnable;
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

        var settings = new JoinRoomSettings
        {
            Code = code,
            Password = password,
            Nickname = nickname,
            IsSpectator = isSpectator
        };

        TryConnect(settings);
    }

    private async void TryConnect(JoinRoomSettings settings)
    {
        var client = await BingoAPI.Bingo.API.JoinRoom<LethalBingoClient>(settings);

        SetActiveJoinForm(true);

        if (client != null)
        {
            LethalBingo.CurrentClient = client;
            return;
        }

        _menuManager?.DisplayMenuNotification("An error has occured while joining the room.", "Okay");
    }

    #endregion

    #region Animations

    [Header("Animations")] [SerializeField]
    private Animator? animator;

    private void OpenForm()
    {
        animator?.SetTrigger(OpenMenu);
    }

    private void CloseForm()
    {
        animator?.SetTrigger(CloseMenu);
    }

    private static readonly int OpenMenu = Animator.StringToHash("openMenu");
    private static readonly int CloseMenu = Animator.StringToHash("closeMenu");

    #endregion

    #region Events

    private void OnConnected(string? roomId, PlayerData player)
    {
        CloseForm();
        openBtn?.gameObject.SetActive(false);

        _menuManager?.DisplayMenuNotification("You successfully joined the room.", "Okay");
    }

    private void OnDisconnected()
    {
        OpenForm();
        openBtn?.gameObject.SetActive(true);
    }

    #endregion
}