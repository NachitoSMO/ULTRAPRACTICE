using Configgy;
using HarmonyLib;
using System.Reflection;
using ULTRAPRACTICE.Classes;
using ULTRAPRACTICE.Patches;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ULTRAPRACTICE;

public sealed class UpdateBehaviour : MonoSingleton<UpdateBehaviour>
{

    [Configgable] // ReSharper disable once FieldCanBeMadeReadOnly.Local
    private static ConfigKeybind save = new(KeyCode.F1); // configgy doesn't like mice in-game :(

    [Configgable] // ReSharper disable once FieldCanBeMadeReadOnly.Local
    private static ConfigKeybind load = new(KeyCode.F2);

    private static bool hasSaved = false;
    void OnLevelWasLoaded(int level)
    {
        hasSaved = false;
    }

    private void Update()
    {
        if (Plugin.Instance.player == null && MonoSingleton<NewMovement>.Instance != null)
            Plugin.Instance.player = MonoSingleton<NewMovement>.Instance;

        if (Plugin.Instance.player != null)
        {
            if (Plugin.Instance.player.IsInvoking(nameof(NewMovement.NotJumping)))
            {
                playerVariables.timeUntilNotJumping += Time.deltaTime;
                Plugin.Instance.jumped = true;
            }

            if (Plugin.Instance.player.IsInvoking(nameof(NewMovement.JumpReady)))
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
                if (Plugin.Instance.coin == null)
                    Plugin.Instance.coin = FindObjectOfType<Revolver>().coin;
                foreach (var coin in MonoSingleton<CoinList>.Instance.revolverCoinsList)
                {
                    if (coin.GetComponent<CoinTimers>() == null) 
                        coin.gameObject.AddComponent<CoinTimers>();

                    if (coin.IsInvoking(nameof(Coin.GetDeleted)))
                        coin.GetComponent<CoinTimers>().deleteTimer += Time.deltaTime;

                    if (coin.IsInvoking(nameof(Coin.StartCheckingSpeed))) 
                        coin.GetComponent<CoinTimers>().checkSpeedTimer += Time.deltaTime;

                    if (coin.IsInvoking(nameof(Coin.TripleTime))) 
                        coin.GetComponent<CoinTimers>().tripleTimer += Time.deltaTime;

                    if (coin.IsInvoking(nameof(Coin.TripleTimeEnd))) 
                        coin.GetComponent<CoinTimers>().tripleEndTimer += Time.deltaTime;

                    if (coin.IsInvoking(nameof(Coin.DoubleTime)))
                        coin.GetComponent<CoinTimers>().doubleTimer += Time.deltaTime;
                }
            }
        }

        ManageInputs();
    }

    private void ManageInputs()
    {
        if (Plugin.Instance.player == null) return;

        if (save.WasPeformed())
        {
            // TODO: perhaps refactor each method to be non-static and implement IVariableSaver or something
            grenadeVariables.SaveVariables();
            CoinVariables.SaveVariables();
            ProjectileVariables.SaveVariables();
            v2Variables.SaveVariables();
            playerVariables.SaveVariables(Plugin.Instance.player);
            WeaponChargeVariables.SaveVariables();
            CannonBallVariables.SaveVariables();
            ObjectActivatorVariables.SaveVariables();
            LoadedRoomsVariables.SaveVariables();

            if (MonoSingleton<StatsManager>.Instance.currentCheckPoint != null && v2Variables.states.Length == 0) Plugin.Instance.atCheckpoint = MonoSingleton<StatsManager>.Instance.currentCheckPoint;
            else Plugin.Instance.atCheckpoint = null;

            hasSaved = true;
        }

        if (hasSaved)
        {
            if (load.WasPeformed())
            {
                // hacky workaround that makes it so if we saved at a point before v2 existed it just resets the room the same way CheckPoints do it so that
                // we can practice 1-4 properly
                if (Plugin.Instance.atCheckpoint != null && FindObjectOfType<V2>() != null)
                {
                    PseudoResetRoom();
                }

                // this is technically wrong, it should instead save whatever the slomo attachments hasBeenDone value was when we save but due to the way slomos are used
                // in game i havent seen a difference

                foreach (SlowMo slomo in FindObjectsOfType<SlowMo>())
                {
                    SlowMoAttachment attach = slomo.GetComponent<SlowMoAttachment>();
                    if (attach != null)
                        attach.hasBeenDone = false;
                }

                grenadeVariables.SetVariables();
                ProjectileVariables.SetVariables();
                CoinVariables.SetVariables();
                v2Variables.SetVariables();
                playerVariables.SetVariables(Plugin.Instance.player);
                WeaponChargeVariables.SetVariables();
                CannonBallVariables.SetVariables();
                ObjectActivatorVariables.SetVariables();
                LoadedRoomsVariables.SetVariables();
            }
        }
    }

    // copy paste of CheckPoint.ResetRoom() but only the bit I care about (running the actual function makes it so your weapon "refreshes")
    public static void PseudoResetRoom()
    {
        var checkp = Plugin.Instance.atCheckpoint;
        var defaultRoom = checkp.defaultRooms[checkp.i];

        Vector3 position = checkp.newRooms[checkp.i].transform.position;
        checkp.newRooms[checkp.i].SetActive(value: false);
        Object.Destroy(checkp.newRooms[checkp.i]);
        checkp.newRooms[checkp.i] = Object.Instantiate(defaultRoom, position, defaultRoom.transform.rotation, defaultRoom.transform.parent);
        checkp.newRooms[checkp.i].SetActive(value: true);
        Bonus[] componentsInChildren = checkp.newRooms[checkp.i].GetComponentsInChildren<Bonus>(includeInactive: true);
        if (componentsInChildren != null && componentsInChildren.Length != 0)
        {
            Bonus[] array = componentsInChildren;
            for (int i = 0; i < array.Length; i++)
            {
                array[i].UpdateStatsManagerReference();
            }
        }
        if (checkp.i + 1 < checkp.defaultRooms.Count)
        {
            checkp.i++;
            PseudoResetRoom();
            return;
        }
    }

    //uses 2 gameObjects and puts all of their components through CopyValues()
    public static void CopyScripts(GameObject source, GameObject target)
    {
        var sourceBehaviours = source.GetComponents<MonoBehaviour>();
        foreach (var behaviour in sourceBehaviours)
        {
            var targetBehaviour = target.GetComponent<MonoBehaviour>();
            if (targetBehaviour)
                CopyValues(targetBehaviour, behaviour);
        }
    }

    //the hell function
    public static void CopyValues(object target, object source)
    {
        if (target == null || source == null)
            return;

        var fields = source.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (var field in fields)
        {
            // we skip rigidbody's as assigning those during runtime doesn't work as we might expect
            if (typeof(Rigidbody).IsAssignableFrom(field.FieldType))
                continue;

            var targetField = target.GetType().GetField(field.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (targetField != null && targetField.FieldType == field.FieldType)
            {
                object value = field.GetValue(source);
                targetField.SetValue(target, value);
            }
        }
    }
}