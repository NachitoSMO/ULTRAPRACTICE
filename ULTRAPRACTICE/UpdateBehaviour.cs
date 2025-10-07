using Configgy;
using System;
using System.Linq;
using System.Reflection;
using TMPro;
using ULTRAPRACTICE.Classes;
using ULTRAPRACTICE.Patches;
using UnityEngine;
namespace ULTRAPRACTICE;

public class UpdateBehaviour : MonoSingleton<UpdateBehaviour>
{

    [Configgable]
    private static ConfigKeybind save = new(KeyCode.F1); // configgy doesn't like mice in-game :(

    [Configgable]
    private static ConfigKeybind load = new(KeyCode.F2);

    private static bool hasSaved = false;

    private static float currentTime;
    void OnLevelWasLoaded(int level)
    {
        hasSaved = false;
    }

    private void Update()
    {
        var player = Plugin.Instance.player;
        if (!player)
        {
            if (MonoSingleton<NewMovement>.Instance != null)
                Plugin.Instance.player = player = MonoSingleton<NewMovement>.Instance;
            else
            {
                ManageInputs();
                return;
            }
        }

        if (player.IsInvoking(nameof(NewMovement.NotJumping)))
        {
            playerVariables.timeUntilNotJumping += Time.deltaTime;
            Plugin.Instance.jumped = true;
        }

        if (player.IsInvoking(nameof(NewMovement.JumpReady)))
        {
            playerVariables.timeUntilJumpReady += Time.deltaTime;
            Plugin.Instance.jumped = true;
        }

        if (!player.jumping && Plugin.Instance.jumped)
        {
            playerVariables.timeUntilNotJumpingMax = playerVariables.timeUntilNotJumping;
            playerVariables.timeUntilJumpReadyMax = playerVariables.timeUntilJumpReady;
            playerVariables.timeUntilNotJumping = 0;
            playerVariables.timeUntilJumpReady = 0;
            Plugin.Instance.jumped = false;
        }

        if (MonoSingleton<CoinList>.Instance.revolverCoinsList.Count == 0)
        {
            ManageInputs();
            return;
        }

        if (Plugin.Instance.coin == null)
            Plugin.Instance.coin = FindObjectOfType<Revolver>().coin;
        foreach (var coin in MonoSingleton<CoinList>.Instance.revolverCoinsList)
        {
            var timer = coin.GetOrAddComponent<CoinTimers>();
            if (coin.IsInvoking(nameof(Coin.GetDeleted)))
                timer.deleteTimer += Time.deltaTime;

            if (coin.IsInvoking(nameof(Coin.StartCheckingSpeed)))
                timer.checkSpeedTimer += Time.deltaTime;

            if (coin.IsInvoking(nameof(Coin.TripleTime)))
                timer.tripleTimer += Time.deltaTime;

            if (coin.IsInvoking(nameof(Coin.TripleTimeEnd)))
                timer.tripleEndTimer += Time.deltaTime;

            if (coin.IsInvoking(nameof(Coin.DoubleTime)))
                timer.doubleTimer += Time.deltaTime;
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
            coinVariables.SaveVariables();
            ProjectileVariables.SaveVariables();
            v2Variables.SaveVariables();
            playerVariables.SaveVariables(Plugin.Instance.player);
            WeaponChargeVariables.SaveVariables();
            CannonBallVariables.SaveVariables();
            LoadedRoomsVariables.SaveVariables();
            OilVariables.SaveVariables();
            ObjectActivatorVariables.SaveVariables();

            if (MonoSingleton<StatsManager>.Instance != null) currentTime = MonoSingleton<StatsManager>.Instance.seconds;

            CreateSubtitle("Saved!");

            if (MonoSingleton<StatsManager>.Instance.currentCheckPoint) Plugin.Instance.atCheckpoint = MonoSingleton<StatsManager>.Instance.currentCheckPoint;

            hasSaved = true;
        }

        if (!hasSaved || !load.WasPeformed()) return;
        // hacky workaround that makes it so if we saved at a point before v2 existed it just resets the room the same way CheckPoints do it so that
        // we can practice 1-4 properly

        if (Plugin.Instance.atCheckpoint && FindObjectOfType<V2>())
        {
            if (v2Variables.states == null)
            {
                PseudoResetRoom();
            }
        }

        // this is technically wrong, it should instead save whatever the slomo attachments
        // hasBeenDone value was when we save but due to the way slomos are used
        // in game i havent seen a difference
        foreach (var attach in FindObjectsOfType<SlowMo>().Select(slomo => slomo.GetComponent<SlowMoAttachment>()).Where(attach => attach != null))
        {
            attach.hasBeenDone = false;
        }

        foreach (SlowMo slomo in FindObjectsOfType<SlowMo>())
        {
            SlowMoAttachment attach = slomo.GetComponent<SlowMoAttachment>();
            if (attach != null)
                attach.hasBeenDone = false;
        }

        v2Variables.SetVariables();
        grenadeVariables.SetVariables();
        ProjectileVariables.SetVariables();
        coinVariables.SetVariables();
        playerVariables.SetVariables(Plugin.Instance.player);
        WeaponChargeVariables.SetVariables();
        CannonBallVariables.SetVariables();
        OilVariables.SetVariables();
        LoadedRoomsVariables.SetVariables();
        ObjectActivatorVariables.SetVariables();

        if (MonoSingleton<StatsManager>.Instance != null) MonoSingleton<StatsManager>.Instance.seconds = currentTime;

        CreateSubtitle("Loaded!");

        if (MonoSingleton<CheatsController>.Instance != null && !MonoSingleton<CheatsController>.Instance.cheatsEnabled) MonoSingleton<CheatsController>.Instance.ActivateCheats();

    }

    public static void CreateSubtitle(string title)
    {
        SubtitleController subController = MonoSingleton<SubtitleController>.Instance;
        Subtitle subtitle = UnityEngine.Object.Instantiate(subController.subtitleLine, subController.container, worldPositionStays: true);
        subtitle.GetComponentInChildren<TMP_Text>().text = title;
        subtitle.fadeInSpeed = subtitle.fadeInSpeed * 7f;
        subtitle.fadeOutSpeed = subtitle.fadeOutSpeed * 7f;
        subtitle.holdForBase = 0.15f;
        subtitle.holdForPerChar = 0.05f;
        subtitle.gameObject.SetActive(value: true);
        if (!subController.previousSubtitle)
        {
            subtitle.ContinueChain();
        }
        else
        {
            subController.previousSubtitle.nextInChain = subtitle;
        }
        subController.previousSubtitle = subtitle;
    }


    // copy paste of CheckPoint.ResetRoom() but only the bit I care about (running the actual function makes it so your weapon "refreshes")
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

    public void SetVelocityAfterPly()
    {
        var ply = Plugin.Instance.player;
        Rigidbody rb = ply.gameObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = playerVariables.savedVel;
        }

        UpdateBehaviour.CopyValues(ply, playerVariables.savedVars);

        if (ply.jumping)
        {
            ply.Invoke(nameof(NewMovement.JumpReady), playerVariables.timeUntilJumpReadyMax - playerVariables.timeUntilJumpReady);
            ply.Invoke(nameof(NewMovement.NotJumping), playerVariables.timeUntilNotJumpingMax - playerVariables.timeUntilNotJumping);
        }
        ply.gc.StopForceOff();
    }

    public void SetVelocityAfterV2()
    {
        var states = v2Variables.states;
        for (int i = 0; i < states.Length; i++)
        {
            if (states[i].gameObject != null && states[i].backupObject != null)
            {
                Rigidbody rb = states[i].gameObject.GetComponent<Rigidbody>();
                rb.isKinematic = false;
                if (rb != null)
                {
                    rb.velocity = states[i].vel;
                    rb.isKinematic = states[i].kinematic;
                }
                states[i].gameObject.gc.CheckColsOnce();

                UpdateBehaviour.CopyScripts(states[i].backupObject, states[i].gameObject.gameObject);
            }
        }
    }

    public void RestoreCoinInvokes()
    {
        var states = coinVariables.states;
        for (int i = 0; i < states.Length; i++)
        {
            var component = states[i].gameObject.GetComponentInChildren<Coin>();
            if (component.checkingSpeed)
            {
                Collider[] array = component.GetComponents<Collider>();
                for (int j = 0; j < array.Length; j++)
                {
                    array[j].enabled = true;
                }
            }
            component.CancelInvoke("StartCheckingSpeed");
            component.CancelInvoke("GetDeleted");
            component.CancelInvoke("TripleTime");
            component.CancelInvoke("TripleTimeEnd");
            component.CancelInvoke("DoubleTime");
            if (states[i].invokingCheckingSpeed)
            {
                component.Invoke("StartCheckingSpeed", 0.1f - states[i].checkSpeedTimerSaved);
            }
            if (states[i].invokingTripleTime)
            {
                component.Invoke("TripleTime", 0.35f - states[i].deleteTimerSaved);
            }
            if (states[i].invokingDoubleTime)
            {
                component.Invoke("DoubleTime", 1f - states[i].deleteTimerSaved);
            }
            if (states[i].invokingTripleTimeEnd)
            {
                component.Invoke("TripleTimeEnd", 0.417f - states[i].deleteTimerSaved);
            }
            if (states[i].invokingDeletion)
            {
                component.Invoke("GetDeleted", 5f - states[i].deleteTimerSaved);
            }
        }
    }

    //uses 2 gameObjects and puts all of their components through CopyValues()
    public static void CopyScripts(GameObject source, GameObject target)
    {
        if (source == null || target == null) return;
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

    public static void CopyValues(object target, object source)
    {
        if (target == null || source == null)
            return;

        Type type = source.GetType();

        FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (FieldInfo field in fields)
        {
            try
            {
                if (typeof(Rigidbody).IsAssignableFrom(field.FieldType) ||
                    typeof(Component).IsAssignableFrom(field.FieldType) ||
                    typeof(GameObject).IsAssignableFrom(field.FieldType))
                    continue;

                FieldInfo targetField = target.GetType().GetField(field.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (targetField == null)
                    continue;

                if (targetField.FieldType != field.FieldType)
                    continue;

                object value = field.GetValue(source);

                if (value is UnityEngine.Object unityObj && unityObj == null)
                    continue;

                targetField.SetValue(target, value);
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"CopyValues: Failed to copy field '{field.Name}' ({field.FieldType}) - {ex.Message}");
            }
        }
    }
}