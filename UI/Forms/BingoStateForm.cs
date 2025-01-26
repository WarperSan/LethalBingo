using System.Collections.Generic;
using BingoAPI;
using BingoAPI.Data;
using BingoAPI.Extensions;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable CS0649

namespace LethalBingo.UI.Forms;

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

    #endregion

    #region Form
        
    private void SetActiveJoinForm(bool isEnable)
    {
        Selectable?[] fields =
        [
            leaveBtn
        ];

        foreach (var field in fields)
        {
            if (field != null)
                field.interactable = isEnable;
        }
    }

    private void SubmitLeave()
    {
        // Disable all inputs
        SetActiveJoinForm(false);

        TryDisconnect();
    }

    private async void TryDisconnect()
    {
        bool success = LethalBingo.CurrentClient == null || await LethalBingo.CurrentClient.Disconnect();

        SetActiveJoinForm(true);
        
        if (success) 
            return;
            
        _menuManager?.DisplayMenuNotification("An error has occured while leaving the room.", "Okay");
    }

    #endregion

    #region Animations
        
    [Header("Animations")]
    [SerializeField]
    private Animator? animator;

    private void OpenForm()
    {
        animator?.SetTrigger(OpenMenu);

        if (LethalBingo.CurrentClient != null)
            SetSelectedColor(LethalBingo.CurrentClient.PlayerData.Team);
    }

    private void CloseForm() => animator?.SetTrigger(CloseMenu);

    private static readonly int OpenMenu = Animator.StringToHash("openMenu");
    private static readonly int CloseMenu = Animator.StringToHash("closeMenu");

    #endregion

    #region Colors

    [Header("Colors")] 
    [SerializeField] 
    private Transform? colorParent;

    [SerializeField]
    private GameObject? colorPrefab;

    private Dictionary<BingoTeam, ColorElement>? buttonForColor;
        
    private void SetColors(BingoTeam[] teams)
    {
        if (colorParent == null || colorPrefab == null)
            return;
            
        // Destroy all children
        foreach (Transform child in colorParent)
            Destroy(child.gameObject);

        buttonForColor = [];

        foreach (var team in teams)
        {
            if (team == BingoTeam.BLANK)
                continue;
                
            // Append team colors
            var newColor = Instantiate(colorPrefab, colorParent);
                
            if (newColor == null)
                continue;

            newColor.name = team.ToString();
                
            var el = newColor.GetComponent<ColorElement>();
                
            if (el == null)
                continue;

            el.Team = team;
            buttonForColor[team] = el;
        }
    }

    private void SetSelectedColor(BingoTeam team)
    {
        if (buttonForColor == null)
            return;
            
        foreach (var (clr, button) in buttonForColor)
            button.enabled = team != clr;
    }

    #endregion

    #region Events

    private void OnConnected(string? roomId, PlayerData player)
    {
        OpenForm();
        openBtn?.gameObject.SetActive(true);

        if (LethalBingo.CurrentClient != null)
            SetSelectedColor(LethalBingo.CurrentClient.PlayerData.Team);
    }
        
    private void OnDisconnected()
    {
        CloseForm();
        openBtn?.gameObject.SetActive(false);
            
        _menuManager?.DisplayMenuNotification("You successfully left the room.", "Thank you");
    }

    private void OnTeamChanged(PlayerData player, BingoTeam oldTeam, BingoTeam newTeam)
    {
        if (buttonForColor == null)
            return;

        if (buttonForColor.TryGetValue(newTeam, out var newTeamBtn))
            newTeamBtn.enabled = false;
            
        if (buttonForColor.TryGetValue(oldTeam, out var oldTeamBtn))
            oldTeamBtn.enabled = true;
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
            
        leaveBtn?.onClick.AddListener(SubmitLeave);

        SetColors(BingoTeamExtension.GetAllTeams());
    }

    private void OnEnable()
    {
        BingoClient.OnSelfConnected.AddListener(OnConnected);
        BingoClient.OnSelfDisconnected.AddListener(OnDisconnected);
        BingoClient.OnSelfTeamChanged.AddListener(OnTeamChanged);
    }

    private void OnDisable()
    {
        BingoClient.OnSelfConnected.RemoveListener(OnConnected);
        BingoClient.OnSelfDisconnected.RemoveListener(OnDisconnected);
        BingoClient.OnSelfTeamChanged.RemoveListener(OnTeamChanged);
    }
}