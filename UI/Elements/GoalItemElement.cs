using BingoAPI.Models;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Logger = LethalBingo.Helpers.Logger;

#pragma warning disable CS0649

namespace LethalBingo.UI.Elements;

public class GoalItemElement : MonoBehaviour
{
    private string? GUID;

    private void Start()
    {
        toggle?.onValueChanged.AddListener(ToggleGoal);
    }

    #region Fields

    [SerializeField] private TextMeshProUGUI? goalName;
    
    [SerializeField] private Toggle? toggle;
    
    [SerializeField] private Image? blackoutImage;
    
    #endregion

    public UnityEvent<bool>? OnActiveChanged; 

    /// <summary>
    /// Sets the information of this item from the given goal
    /// </summary>
    public void SetGoal(Goal goal)
    {
        GUID = goal.GUID;

        if (goalName != null)
            goalName.text = goal.Title;

        if (toggle != null)
            toggle.isOn = goal.IsActive;
        
        if (blackoutImage != null)
            blackoutImage.enabled = !goal.IsActive;
    }
    
    private void ToggleGoal(bool isActive)
    {
        if (GUID == null)
        {
            Logger.Info("Unset GUID for this goal.");
            return;
        }
        
        if (blackoutImage != null)
            blackoutImage.enabled = !isActive;
        
        OnActiveChanged?.Invoke(isActive);
    }
}