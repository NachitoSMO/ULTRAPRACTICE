using System.Reflection;
using Configgy;
using HarmonyLib;
using ULTRAPRACTICE.Classes;
using ULTRAPRACTICE.Patches;
using UnityEngine;

namespace ULTRAPRACTICE;

public sealed class UpdateBehaviour : MonoSingleton<UpdateBehaviour>
{

    [Configgable] // ReSharper disable once FieldCanBeMadeReadOnly.Local
    private static ConfigKeybind save = new(KeyCode.F1); // configgy doesn't like mice in-game :(

    [Configgable] // ReSharper disable once FieldCanBeMadeReadOnly.Local
    private static ConfigKeybind load = new(KeyCode.F2);

    private void Update()
    {
        if (Plugin.Instance.player == null)
        {
            if (MonoSingleton<NewMovement>.Instance != null) Plugin.Instance.player = MonoSingleton<NewMovement>.Instance;
        }

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
                if (Plugin.Instance.coin == null) Plugin.Instance.coin = FindObjectOfType<Revolver>().coin;
                foreach (Coin coin in MonoSingleton<CoinList>.Instance.revolverCoinsList)
                {
                    if (coin.GetComponent<CoinTimers>() == null) coin.gameObject.AddComponent<CoinTimers>();

                    if (coin.IsInvoking(nameof(Coin.GetDeleted)))
                    {
                        coin.GetComponent<CoinTimers>().deleteTimer += Time.deltaTime;
                    }

                    if (coin.IsInvoking(nameof(Coin.StartCheckingSpeed)))
                    {
                        coin.GetComponent<CoinTimers>().checkSpeedTimer += Time.deltaTime;
                    }

                    if (coin.IsInvoking(nameof(Coin.TripleTime)))
                    {
                        coin.GetComponent<CoinTimers>().tripleTimer += Time.deltaTime;
                    }

                    if (coin.IsInvoking(nameof(Coin.TripleTimeEnd)))
                    {
                        coin.GetComponent<CoinTimers>().tripleEndTimer += Time.deltaTime;
                    }

                    if (coin.IsInvoking(nameof(Coin.DoubleTime)))
                    {
                        coin.GetComponent<CoinTimers>().doubleTimer += Time.deltaTime;
                    }
                }
            }
        }

        ManageInputs();
    }

    private void ManageInputs()
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

            foreach (SlowMo slomo in FindObjectsOfType<SlowMo>())
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
        var checkpoint = Plugin.Instance.atCheckpoint;
        for (; checkpoint.i < checkpoint.defaultRooms.Count - 1; checkpoint.i++)
        {
            checkpoint = Plugin.Instance.atCheckpoint;
            var newRoom = checkpoint.newRooms[checkpoint.i];
            var position = newRoom.transform.position;

            newRoom.SetActive(value: false);
            Destroy(newRoom);
            var defaultRoom = checkpoint.defaultRooms[checkpoint.i];
            checkpoint.newRooms[checkpoint.i] = newRoom = Instantiate(defaultRoom, position, defaultRoom.transform.rotation, defaultRoom.transform.parent);
            newRoom.SetActive(value: true);
            var bonusesAtCheckpoint = newRoom.GetComponentsInChildren<Bonus>(includeInactive: true);

            if (bonusesAtCheckpoint != null && bonusesAtCheckpoint.Length != 0)
            {
                foreach (var bonus in bonusesAtCheckpoint)
                {
                    bonus.UpdateStatsManagerReference();
                }
            }

            if (checkpoint.i >= checkpoint.defaultRooms.Count - 1) break;
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