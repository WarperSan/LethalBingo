using BingoAPI.Models;
using BingoAPI.Extensions;
using BingoAPI.Managers;
using LethalBingo.UI.Slots;
using UnityEngine;

namespace LethalBingo.UI;

#pragma warning disable CS0649

public class BingoBoard : MonoBehaviour
{
    #region Squares

    private BingoSquare?[]? squares;

    #endregion

    private void Start()
    {
        FetchBoard();
    }

    private void OnEnable()
    {
        ClientEventManager.OnSelfMarked.AddListener(OnSquareMarked);
        ClientEventManager.OnOtherMarked.AddListener(OnSquareMarked);
        ClientEventManager.OnSelfCleared.AddListener(OnSquareCleared);
        ClientEventManager.OnOtherCleared.AddListener(OnSquareCleared);
    }

    private void OnDisable()
    {
        ClientEventManager.OnSelfMarked.RemoveListener(OnSquareMarked);
        ClientEventManager.OnOtherMarked.RemoveListener(OnSquareMarked);
        ClientEventManager.OnSelfCleared.RemoveListener(OnSquareCleared);
        ClientEventManager.OnOtherCleared.RemoveListener(OnSquareCleared);
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

        var allTeams = TeamExtension.GetAllTeams();

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

    #region Fields

    [Header("Fields")] [SerializeField] private Transform? slotsParent;

    [SerializeField] private GameObject? slotPrefab;

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
}