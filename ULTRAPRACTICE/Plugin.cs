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
    private ManualLogSource log => base.Logger;
    internal new static ManualLogSource Logger => Instance.log;

    public static Plugin Instance { get; private set; }

    private ConfigBuilder config;

    public NewMovement player;

    public GameObject coin;

    public bool jumped;

    public CheckPoint atCheckpoint;

    private void Awake()
    {
        gameObject.hideFlags = HideFlags.DontSaveInEditor;
        Logger.LogInfo($"Mod {PLUGIN_NAME} version {PLUGIN_VERSION} is loading...");

        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), PLUGIN_GUID);

        config = new ConfigBuilder(PLUGIN_GUID, PLUGIN_NAME);
        config.BuildAll();

        Instance = this;

        var obj = new GameObject(PLUGIN_GUID) { hideFlags = HideFlags.HideAndDontSave };
        DontDestroyOnLoad(obj);
        obj.AddComponent<UpdateBehaviour>();

        Logger.LogInfo($"Mod {PLUGIN_NAME} version {PLUGIN_VERSION} is loaded!");
    }

}