using System.Collections.Generic;
using System.Text.RegularExpressions;
using BingoAPI.Managers;
using BingoAPI.Models;
using LethalBingo.UI.Elements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable CS0649

namespace LethalBingo.UI.Forms;

public class BingoGoalForm : MonoBehaviour
{
    private void Start()
    {
        var _menuManager = FindObjectOfType<MenuManager>();

        closeBtn?.onClick.AddListener(CloseForm);
        closeBtn?.onClick.AddListener(_menuManager.PlayCancelSFX);
        
        searchBar?.onValueChanged.AddListener(OnSearchChanged);

        var loadedGoals = GoalManager.GetAllGoals();
        SetGoals(loadedGoals);
        UpdateGoalCount();
    }

    #region Fields

    [Header("Fields")] [SerializeField] private Transform? goalListContainer;

    [SerializeField] private GameObject? goalItemPrefab;

    [SerializeField] private GameObject? noGoalText;

    [SerializeField] private TextMeshProUGUI? goalCountText;

    [SerializeField] private TMP_InputField? searchBar;

    [SerializeField] private Button? closeBtn;

    #endregion

    #region Animations

    [Header("Animations")] [SerializeField]
    private Animator? animator;

    public void OpenForm()
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

    #region Goals

    private void AddGoal(Goal goal)
    {
        if (goalListContainer == null || goalItemPrefab == null)
            return;

        var newItem = Instantiate(goalItemPrefab, goalListContainer, false);

        if (newItem.TryGetComponent(out GoalItemElement goalItem))
        {
            goalItem.SetGoal(goal);
            
            goalItem.OnActiveChanged?.AddListener(isActive => GoalManager.SetActiveGoal(goal.GUID, isActive));
            goalItem.OnActiveChanged?.AddListener(isActive => UpdateGoalCount());
        }
    }

    public void SetGoals(List<Goal> goals)
    {
        if (goalListContainer == null)
            return;

        foreach (Transform child in goalListContainer)
            Destroy(child.gameObject);

        noGoalText?.SetActive(goals.Count == 0);

        foreach (var goal in goals)
            AddGoal(goal);
    }
    
    #endregion

    private void OnSearchChanged(string value)
    {
        var loadedGoals = GoalManager.GetAllGoals();
        
        if (string.IsNullOrEmpty(value))
        {
            SetGoals(loadedGoals);
            return;
        }

        var regex = new Regex($".*{value}.*");
        var validGoals = new List<Goal>();

        foreach (var goal in loadedGoals)
        {
            if (!regex.IsMatch(goal.Title))
                continue;
            
            validGoals.Add(goal);
        }
        
        SetGoals(validGoals);
    }

    private void UpdateGoalCount()
    {
        if (goalCountText == null)
            return;

        var loadedGoals = GoalManager.GetAllGoals();
        
        var text = string.Format(
            "<color={0}>{1}</color> / {2}",
            GoalManager.ActiveGoalCount < BingoAPI.Bingo.Constants.BINGO_SIZE ? "red" : "green",
            GoalManager.ActiveGoalCount,
            loadedGoals.Count
        );

        goalCountText.text = text;
    }
}