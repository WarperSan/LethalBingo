using LethalBingo.Core.Data;
using LethalBingo.Extensions;
using LethalBingo.UI.Slots;
using UnityEngine;

namespace LethalBingo.Objects;

#pragma warning disable CS0649

public class BingoBoard : MonoBehaviour
{
    #region Fields

    [Header("Fields")] 
    [SerializeField] private Transform? slotsParent;
    [SerializeField] private GameObject? slotPrefab;

    #endregion

    #region Squares

    private BingoSquare?[]? squares;

    #endregion
    
    #region Events

    private void OnSquareMarked(PlayerData player, SquareData data)
    {
        if (squares == null)
            return;

        var index = data.Index;
        
        if (index >= squares.Length || index <= 0)
            return;
        
        squares[index]?.SetTeams(data.Teams);
    }

    private void OnSquareCleared(PlayerData player, SquareData data)
    {
        if (squares == null)
            return;

        var index = data.Index;
        
        if (index >= squares.Length || index <= 0)
            return;
        
        squares[index]?.SetTeams(data.Teams);
    }

    #endregion
    
    private void Start()
    {
        FetchBoard();
    }

    private async void FetchBoard()
    {
        if (slotsParent == null)
            return;
        
        if (LethalBingo.CurrentClient == null)
            return;

        var board = await LethalBingo.CurrentClient.GetBoard();
        
        if (board == null)
            return;

        var allTeams = BingoTeamExtension.GetAllTeams();

        squares = new BingoSquare[board.Length + 1];
        
        foreach (Transform child in slotsParent)
            Destroy(child.gameObject);

        foreach (var square in board)
        {
            var newSlot = Instantiate(slotPrefab, slotsParent);
            
            if (newSlot == null)
                continue;
            
            newSlot.name = "Slot #" + square.Index;

            if (newSlot.TryGetComponent(out BingoSquare slot))
            {
                slot.DisplayText(square.Name ?? "???");
                slot.CacheMarkings(allTeams);
                slot.SetTeams(square.Teams);
                squares[square.Index] = slot;
            }
        }
    }

    private void OnEnable()
    {
        BingoClient.OnSelfMarked.AddListener(OnSquareMarked);
        BingoClient.OnOtherMarked.AddListener(OnSquareMarked);
        BingoClient.OnSelfCleared.AddListener(OnSquareCleared);
        BingoClient.OnOtherCleared.AddListener(OnSquareCleared);
    }
    
    private void OnDisable()
    {
        BingoClient.OnSelfMarked.RemoveListener(OnSquareMarked);
        BingoClient.OnOtherMarked.RemoveListener(OnSquareMarked);
        BingoClient.OnSelfCleared.RemoveListener(OnSquareCleared);
        BingoClient.OnOtherCleared.RemoveListener(OnSquareCleared);
    }
}