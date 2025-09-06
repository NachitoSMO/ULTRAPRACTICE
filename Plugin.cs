using BepInEx;
using Configgy;
using HarmonyLib;
using System.Collections;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using ULTRAPRACTICE.Classes;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;
using ULTRAPRACTICE.Patches;

namespace ULTRAPRACTICE
{
    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency("Hydraxous.ULTRAKILL.Configgy", BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin
    {
        private const string modGUID = "Nacho.ULTRAPRACTICE";
        private const string modName = "ULTRAPRACTICE";
        private const string modVersion = "0.1.3";

        public static Plugin Instance { get; private set; }
        private ConfigBuilder config;

        public NewMovement player;

        public GameObject coin;

        public bool jumped;

        public CheckPoint atCheckpoint;

        private static readonly Harmony Harmony = new Harmony(modGUID);
        private void Awake()
        {
            Debug.Log($"Mod {modName} version {modVersion} is loading...");
            Harmony.PatchAll();
            config = new ConfigBuilder(modGUID, modName);
            config.BuildAll();

            Instance = this;

            GameObject obj = new GameObject(modGUID);
            obj.hideFlags = HideFlags.HideAndDontSave;
            DontDestroyOnLoad(obj);
            obj.AddComponent<UpdateBehaviour>();

            Debug.Log($"Mod {modName} version {modVersion} is loaded!");
        }

    }

    public class UpdateBehaviour : MonoSingleton<UpdateBehaviour>
    {

        [Configgable]
        private static ConfigKeybind save = new ConfigKeybind(KeyCode.F1); // configgy doesn't like mice in-game :(

        [Configgable]
        private static ConfigKeybind load = new ConfigKeybind(KeyCode.F2);
        void Update()
        {
            if (Plugin.Instance.player == null)
            {
                if (MonoSingleton<NewMovement>.Instance != null) Plugin.Instance.player = MonoSingleton<NewMovement>.Instance;
            }

            if (Plugin.Instance.player != null)
            {
                if (Plugin.Instance.player.IsInvoking("NotJumping"))
                {
                    playerVariables.timeUntilNotJumping += Time.deltaTime;
                    Plugin.Instance.jumped = true;
                }

                if (Plugin.Instance.player.IsInvoking("JumpReady"))
                {
                    playerVariables.timeUntilJumpReady += Time.deltaTime;
                    Plugin.Instance.jumped = true;
                }

                if (!Plugin.Instance.player.jumping && Plugin.Instance.jumped)
                {
                    playerVariables.timeUntilNotJumpingMax = playerVariables.timeUntilNotJumping;
                    playerVariables.timeUntilJumpReadyMax = playerVariables.timeUntilJumpReady;
                    playerVariables.timeUntilNotJumping = 0;
                    playerVariables.timeUntilJumpReady = 0;
                    Plugin.Instance.jumped = false;
                }

                if (MonoSingleton<CoinList>.Instance.revolverCoinsList.Count != 0)
                {
                    if (Plugin.Instance.coin == null) Plugin.Instance.coin = FindObjectOfType<Revolver>().coin;
                    foreach (Coin coin in MonoSingleton<CoinList>.Instance.revolverCoinsList)
                    {
                        if (coin.GetComponent<CoinTimers>() == null) coin.gameObject.AddComponent<CoinTimers>();

                        if (coin.IsInvoking("GetDeleted"))
                        {
                            coin.GetComponent<CoinTimers>().deleteTimer += Time.deltaTime;
                        }

                        if (coin.IsInvoking("StartCheckingSpeed"))
                        {
                            coin.GetComponent<CoinTimers>().checkSpeedTimer += Time.deltaTime;
                        }

                        if (coin.IsInvoking("TripleTime"))
                        {
                            coin.GetComponent<CoinTimers>().tripleTimer += Time.deltaTime;
                        }

                        if (coin.IsInvoking("TripleTimeEnd"))
                        {
                            coin.GetComponent<CoinTimers>().tripleEndTimer += Time.deltaTime;
                        }

                        if (coin.IsInvoking("DoubleTime"))
                        {
                            coin.GetComponent<CoinTimers>().doubleTimer += Time.deltaTime;
                        }
                    }
                }
            }

            ManageInputs();
        }

        void ManageInputs()
        {
            if (Plugin.Instance.player == null)
            {
                return;
            }

            if (save.WasPeformed())
            {
                grenadeVariables.SaveVariables();
                coinVariables.SaveVariables();
                ProjectileVariables.SaveVariables();
                v2Variables.SaveVariables();
                playerVariables.SaveVariables(Plugin.Instance.player);
                WeaponChargeVariables.SaveVariables();
                CannonBallVariables.SaveVariables();
                ObjectActivatorVariables.SaveVariables();
                LoadedRoomsVariables.SaveVariables();

                if (MonoSingleton<StatsManager>.Instance.currentCheckPoint != null && v2Variables.states.Length == 0) Plugin.Instance.atCheckpoint = MonoSingleton<StatsManager>.Instance.currentCheckPoint;
                else Plugin.Instance.atCheckpoint = null;
            }

            if (load.WasPeformed())
            {
                // hacky workaround that makes it so if we saved at a point before v2 existed it just resets the room the same way CheckPoints do it so that
                // we can practice 1-4 properly
                if (Plugin.Instance.atCheckpoint != null && FindObjectOfType<V2>(true) != null)
                {
                    PseudoResetRoom();
                }

                // this is technically wrong, it should instead save whatever the slomo attachments hasBeenDone value was when we save but due to the way slomos are used
                // in game i havent seen a difference

                foreach (SlowMo slomo in GameObject.FindObjectsOfType<SlowMo>())
                {
                    SlowMoAttachment attach = slomo.GetComponent<SlowMoAttachment>();
                    if (attach != null)
                        attach.hasBeenDone = false;
                }

                LoadedRoomsVariables.SetVariables();

                grenadeVariables.SetVariables();
                ProjectileVariables.SetVariables();
                coinVariables.SetVariables();
                v2Variables.SetVariables();
                playerVariables.SetVariables(Plugin.Instance.player);
                WeaponChargeVariables.SetVariables();
                CannonBallVariables.SetVariables();
                ObjectActivatorVariables.SetVariables();
            }
        }

        // copy paste of CheckPoint.ResetRoom() but only the bit I care about
        public static void PseudoResetRoom()
        {
            Vector3 position = Plugin.Instance.atCheckpoint.newRooms[Plugin.Instance.atCheckpoint.i].transform.position;
            Plugin.Instance.atCheckpoint.newRooms[Plugin.Instance.atCheckpoint.i].SetActive(value: false);
            UnityEngine.Object.Destroy(Plugin.Instance.atCheckpoint.newRooms[Plugin.Instance.atCheckpoint.i]);
            Plugin.Instance.atCheckpoint.newRooms[Plugin.Instance.atCheckpoint.i] = UnityEngine.Object.Instantiate(Plugin.Instance.atCheckpoint.defaultRooms[Plugin.Instance.atCheckpoint.i], position, Plugin.Instance.atCheckpoint.defaultRooms[Plugin.Instance.atCheckpoint.i].transform.rotation, Plugin.Instance.atCheckpoint.defaultRooms[Plugin.Instance.atCheckpoint.i].transform.parent);
            Plugin.Instance.atCheckpoint.newRooms[Plugin.Instance.atCheckpoint.i].SetActive(value: true);
            Bonus[] componentsInChildren = Plugin.Instance.atCheckpoint.newRooms[Plugin.Instance.atCheckpoint.i].GetComponentsInChildren<Bonus>(includeInactive: true);
            if (componentsInChildren != null && componentsInChildren.Length != 0)
            {
                Bonus[] array = componentsInChildren;
                for (int i = 0; i < array.Length; i++)
                {
                    array[i].UpdateStatsManagerReference();
                }
            }
            if (Plugin.Instance.atCheckpoint.i + 1 < Plugin.Instance.atCheckpoint.defaultRooms.Count)
            {
                Plugin.Instance.atCheckpoint.i++;
                PseudoResetRoom();
                return;
            }
        }

        //uses 2 gameObjects and puts all of their components through CopyValues()
        public static void CopyScripts(GameObject source, GameObject target)
        {
            MonoBehaviour[] components = source.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour monoBehaviour in components)
            {
                UnityEngine.Component component = target.GetComponent(monoBehaviour.GetType());
                if (component != null)
                {
                    CopyValues(component, monoBehaviour);
                }
            }
        }

        //the hell function 
        public static void CopyValues(object target, object source)
        {
            if (target == null || source == null)
                return;

            FieldInfo[] fields = source.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (FieldInfo field in fields)
            {
                // we skip rigidbody's as assigning those during runtime doesn't work as we might expect
                if (typeof(Rigidbody).IsAssignableFrom(field.FieldType))
                    continue;

                FieldInfo targetField = target.GetType().GetField(field.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (targetField != null && targetField.FieldType == field.FieldType)
                {
                    object value = field.GetValue(source);
                    targetField.SetValue(target, value);
                }
            }
        }
    }
}
