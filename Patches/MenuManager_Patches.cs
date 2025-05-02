using HarmonyLib;
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
        // Fetch container
        var container = GameObject.Find(Constants.MENU_CONTAINER_PATH);

        if (container == null)
        {
            Logger.Error($"Could not find the container at '{Constants.MENU_CONTAINER_PATH}'.");
            return;
        }

        // Create ui
        var ui = new GameObject(nameof(LethalBingo) + "_UI");
        ui.transform.SetParent(container.transform, false);

        var rect = ui.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.anchoredPosition = Vector2.zero;
        rect.offsetMin = rect.offsetMax = Vector2.zero;

        var index = container.transform.Find("LobbyHostSettings")?.GetSiblingIndex() ?? -1;

        if (index != -1)
            ui.transform.SetSiblingIndex(index);

        // Create forms
        if (LethalBingo.BINGO_JOIN_FORM_PREFAB != null)
            Object.Instantiate(LethalBingo.BINGO_JOIN_FORM_PREFAB, ui.transform, false);
        else
            Logger.Error("Could not spawn the join form.");

        if (LethalBingo.BINGO_CREATE_FORM_PREFAB != null)
            Object.Instantiate(LethalBingo.BINGO_CREATE_FORM_PREFAB, ui.transform, false);
        else
            Logger.Error("Could not spawn the create form.");

        if (LethalBingo.BINGO_STATE_FORM_PREFAB != null)
            Object.Instantiate(LethalBingo.BINGO_STATE_FORM_PREFAB, ui.transform, false);
        else
            Logger.Error("Could not spawn the state form.");
    }
}