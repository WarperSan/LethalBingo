using System.Collections.Generic;
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

public class BingoCreateForm : MonoBehaviour
{
    private MenuManager? _menuManager;

    private void Start()
    {
        _menuManager = FindObjectOfType<MenuManager>();

        openBtn?.onClick.AddListener(OpenForm);
        openBtn?.onClick.AddListener(_menuManager.PlayConfirmSFX);

        closeBtn?.onClick.AddListener(CloseForm);
        closeBtn?.onClick.AddListener(_menuManager.PlayCancelSFX);
        
        if (goalForm != null)
            openGoalListBtn?.onClick.AddListener(goalForm.OpenForm);
        openGoalListBtn?.onClick.AddListener(_menuManager.PlayCancelSFX);

        createBtn?.onClick.AddListener(SubmitCreate);

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

    [Header("Fields")] [SerializeField] private TMP_InputField? roomName;

    [SerializeField] private TMP_InputField? roomPassword;

    [SerializeField] private TMP_InputField? userNickname;

    [SerializeField] private Toggle? joinAsSpectator;

    [SerializeField] private Toggle? randomized;

    [SerializeField] private Toggle? lockout;

    [SerializeField] private Toggle? hideCard;

    [SerializeField] private TMP_InputField? seed;

    [SerializeField] private TMP_InputField? boardJson;

    [SerializeField] private Button? openGoalListBtn;

    [SerializeField] private BingoGoalForm? goalForm;
    
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
            openGoalListBtn,
            createBtn
        ];

        foreach (var field in fields)
            if (field != null)
                field.interactable = isEnable;
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

        var goals = GoalManager.GetAllGoals();
        var activeGoals = new List<Goal>();

        foreach (var goal in goals)
        {
            if (goal.IsActive)
                activeGoals.Add(goal);
        }

        var isLockout = lockout?.isOn ?? false;

        var _seed = seed?.text.Trim() ?? "";

        var isSpectator = joinAsSpectator?.isOn ?? false;

        var _hideCard = hideCard?.isOn ?? false;
        
        var settings = new CreateRoomSettings
        {
            Name = _name,
            Password = password,
            Nickname = nickName,
            IsRandomized = isRandomized,
            Goals = activeGoals.ToArray(),
            IsLockout = isLockout,
            Seed = _seed,
            IsSpectator = isSpectator,
            HideCard = _hideCard
        };

        TryCreate(settings);
    }

    private async void TryCreate(CreateRoomSettings settings)
    {
        var client = await BingoAPI.Bingo.API.CreateRoom<LethalBingoClient>(settings);

        SetActiveCreateForm(true);

        if (client != null)
        {
            LethalBingo.CurrentClient = client;
            return;
        }

        _menuManager?.DisplayMenuNotification("An error has occured while creating the room.", "Okay");
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

        _menuManager?.DisplayMenuNotification("You successfully created the room.", "Okay");
    }

    private void OnDisconnected()
    {
        //OpenForm();
        openBtn?.gameObject.SetActive(true);
    }

    #endregion
}