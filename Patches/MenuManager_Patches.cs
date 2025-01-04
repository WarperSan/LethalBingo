using HarmonyLib;
using LethalBingo.Helpers;
using UnityEngine;
using Logger = LethalBingo.Helpers.Logger;

namespace LethalBingo.Patches;

[HarmonyPatch(typeof(MenuManager))]
internal class MenuManager_Patches
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(MenuManager.Start))]
    private static void CreateUI(MenuManager __instance)
    {
        BuildUI();
    }

    private static void BuildUI()
    {
        // Fetch container
        var container = GameObject.Find(Constants.MENU_CONTAINER_PATH);

        if (container == null)
        {
            Logger.Error($"Could not find the container at '{Constants.MENU_CONTAINER_PATH}'.");
            return;
        }
        
        // Fetch prefab
        var prefab = Bundle.LoadAsset<GameObject>(Constants.BINGO_UI_PREFAB);

        if (prefab == null)
        {
            Logger.Error("Could not load the UI for the bingo.");
            return;
        }

        // Create menu
        var menu = Object.Instantiate(prefab, container.transform, false);

        if (menu == null) return;

        var index = container.transform.Find("LobbyHostSettings")?.GetSiblingIndex() ?? -1;
        
        if (index != -1)
            menu.transform.SetSiblingIndex(index);
    }
}