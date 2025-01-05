using HarmonyLib;

namespace LethalBingo.Patches;

[HarmonyPatch(typeof(HUDManager))]
internal class HUDManager_Patches
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(HUDManager.AddTextToChatOnServer))]
    private static void SendMessageToWeb(HUDManager __instance, string chatMessage, int playerId = -1)
    {
        // If message from system, skip
        if (playerId == -1)
            return;
        
        // If no client, skip
        if (LethalBingo.CurrentClient == null)
            return;

        _ = LethalBingo.CurrentClient.SendMessage(chatMessage);
    }
}