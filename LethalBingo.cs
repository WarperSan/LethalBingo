using BepInEx;
using HarmonyLib;
using LethalBingo.Helpers;
using LethalBingo.Objects;

namespace LethalBingo;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class LethalBingo : BaseUnityPlugin
{
    public static BingoClient? CurrentClient;
    
    private void Awake()
    {
        Helpers.Logger.SetLogger(Logger);

        // Load bundle
        if (!Bundle.LoadBundle("LethalBingo.Resources.lb-bundle"))
        {
            Helpers.Logger.Error("Failed to load the bundle. This mod will not continue further.");
            return;
        }
        
        ApplyPatches();

        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
    }

    #region Patches

    private Harmony? _harmony;

    private void ApplyPatches()
    {
        Helpers.Logger.Debug("Applying patches...");

        _harmony ??= new Harmony(MyPluginInfo.PLUGIN_GUID);

        _harmony.PatchAll();

        Helpers.Logger.Debug("Finished applying patches!");
    }

    #endregion
}