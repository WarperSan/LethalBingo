using BingoAPI.Models;
using UnityEngine;

namespace LethalBingo.UI;

[CreateAssetMenu(fileName = "TeamIconInfo")]
public class TeamIconInfo : ScriptableObject
{
    public Team Team;
    public Sprite? Icon;
}