using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using Configgy;
using HarmonyLib;
using UnityEngine;
using static ULTRAPRACTICE.MyPluginInfo;
namespace ULTRAPRACTICE;

[BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
[BepInDependency("Hydraxous.ULTRAKILL.Configgy", BepInDependency.DependencyFlags.HardDependency)]
public sealed class Plugin : BaseUnityPlugin
{
    internal new static ManualLogSource Logger { get; private set; }

    public static Plugin Instance { get; private set; }

    private ConfigBuilder config;

    public NewMovement player;

    public GameObject coin;

    public bool jumped;

    public CheckPoint atCheckpoint;

    private void Awake()
    {
        Logger = base.Logger;
        Logger.LogInfo($"Mod {PLUGIN_NAME} version {PLUGIN_VERSION} is loading...");

        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), PLUGIN_GUID);
        gameObject.hideFlags = HideFlags.DontSaveInEditor;
        config = new ConfigBuilder(PLUGIN_GUID, PLUGIN_NAME);
        config.BuildAll();

        Instance = this;

        var obj = new GameObject(PLUGIN_GUID) { hideFlags = HideFlags.HideAndDontSave };
        DontDestroyOnLoad(obj);
        obj.AddComponent<UpdateBehaviour>();

        Logger.LogInfo($"Mod {PLUGIN_NAME} version {PLUGIN_VERSION} is loaded!");
    }

}