using BepInEx;
using HarmonyLib;
using LethalBingo.Core;
using LethalBingo.Helpers;
using LethalBingo.Objects;
using UnityEngine;

namespace LethalBingo;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class LethalBingo : BaseUnityPlugin
{
    public static BingoClient? CurrentClient;
    
    private void Awake()
    {
        AssemblyLoader.LoadEmbeddedDLL();
        
        Helpers.Logger.SetLogger(Logger);

        // Load bundle
        if (!Bundle.LoadBundle("LethalBingo.Resources.lb-bundle"))
        {
            Helpers.Logger.Error("Failed to load the bundle. This mod will not continue further.");
            return;
        }

        PreparePrefabs();
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

    #region Prefabs

    public static GameObject? BINGO_JOIN_FORM_PREFAB;
    public static GameObject? BINGO_CREATE_FORM_PREFAB;
    public static GameObject? BINGO_STATE_FORM_PREFAB;
    public static GameObject? BINGO_BOARD_PREFAB;
    
    private static void PreparePrefabs()
    {
        Helpers.Logger.Debug("Preparing prefabs...");

        BINGO_JOIN_FORM_PREFAB = Bundle.LoadAsset<GameObject>("BingoJoinForm");
        BINGO_CREATE_FORM_PREFAB = Bundle.LoadAsset<GameObject>("BingoCreateForm");
        BINGO_STATE_FORM_PREFAB = Bundle.LoadAsset<GameObject>("BingoStateForm");
        BINGO_BOARD_PREFAB = Bundle.LoadAsset<GameObject>("BingoBoard");
        
        Helpers.Logger.Debug("Finished preparing prefabs!");
    }

    #endregion
}