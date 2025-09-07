using System.Linq;
using Configgy;
using System.Reflection;
using ULTRAPRACTICE.ClassSavers;
using ULTRAPRACTICE.Interfaces;
using ULTRAPRACTICE.Patches;
using UnityEngine;

namespace ULTRAPRACTICE;

public sealed class UpdateBehaviour : MonoSingleton<UpdateBehaviour>
{
    [Configgable] // ReSharper disable once FieldCanBeMadeReadOnly.Local
    private static ConfigKeybind save = new(KeyCode.F1); // configgy doesn't like mice in-game :(

    [Configgable] // ReSharper disable once FieldCanBeMadeReadOnly.Local
    private static ConfigKeybind load = new(KeyCode.F2);

    private static bool hasSaved = false;
    private static readonly IVariableSaver[] savers =
    [
        new GrenadeVariables(),
        new CoinVariables(),
        new ProjectileVariables(),
        new V2Variables(),
        new PlayerVariables(),
        new WeaponChargeVariables(),
        new CannonBallVariables(),
        new ObjectActivatorVariables(),
        new LoadedRoomsVariables()
    ];

    private void OnLevelWasLoaded(int level)
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
            PlayerVariables.timeUntilNotJumping += Time.deltaTime;
            Plugin.Instance.jumped = true;
        }

        if (player.IsInvoking(nameof(NewMovement.JumpReady)))
        {
            PlayerVariables.timeUntilJumpReady += Time.deltaTime;
            Plugin.Instance.jumped = true;
        }

        if (!player.jumping && Plugin.Instance.jumped)
        {
            PlayerVariables.timeUntilNotJumpingMax = PlayerVariables.timeUntilNotJumping;
            PlayerVariables.timeUntilJumpReadyMax = PlayerVariables.timeUntilJumpReady;
            PlayerVariables.timeUntilNotJumping = 0;
            PlayerVariables.timeUntilJumpReady = 0;
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
            var timer = coin.GetOrAddComponent<CoinVariables.CoinTimers>();
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
            // TODO: perhaps refactor each method to be non-static and implement VariableSaver or something
            foreach (var saver in savers)
                saver.SaveVariables();

            if (MonoSingleton<StatsManager>.Instance.currentCheckPoint != null && ((V2Variables)savers.First(ivs => ivs is V2Variables)).States.Count == 0)
                Plugin.Instance.atCheckpoint = MonoSingleton<StatsManager>.Instance.currentCheckPoint;
            else Plugin.Instance.atCheckpoint = null;

            hasSaved = true;
        }

        if (!hasSaved || !load.WasPeformed()) return;
        // Hacky workaround that makes it so if we saved at a point before v2 existed,
        // it just resets the room the same way CheckPoints do it so that
        // we can practice 1-4 properly
        if (Plugin.Instance.atCheckpoint && FindObjectOfType<V2>())
            PseudoResetRoom();

        // This is technically wrong, it should instead save whatever the slomo attachments
        // hasBeenDone value was when we save but due to the way slomos are used
        // in game I haven't seen a difference
        foreach (var attach in FindObjectsOfType<SlowMo>()
                              .Select(slomo => slomo.GetComponent<SlowMoAttachment>())
                              .Where(attach => attach != null))
            attach.hasBeenDone = false;
        foreach (var saver in savers)
            saver.SetVariables();
    }

    // copy and paste of CheckPoint.ResetRoom() but only the bit I care about (running the actual function makes it so your weapon "refreshes")
    public static void PseudoResetRoom()
    {
        var checkP = Plugin.Instance.atCheckpoint;
        for (; checkP.i < checkP.defaultRooms.Count - 1; checkP.i++)
        {
            checkP = Plugin.Instance.atCheckpoint;
            var defaultRoom = checkP.defaultRooms[checkP.i];

            var position = checkP.newRooms[checkP.i].transform.position;
            checkP.newRooms[checkP.i] = checkP.newRooms[checkP.i]
                                              .ReplaceWith(Instantiate(defaultRoom, position,
                                                                       defaultRoom.transform.rotation,
                                                                       defaultRoom.transform.parent));
            var bonuses = checkP.newRooms[checkP.i]
                                .GetComponentsInChildren<Bonus>(includeInactive: true);
            foreach (var bonus in bonuses)
                bonus.UpdateStatsManagerReference();
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
            if (targetField == null || targetField.FieldType != field.FieldType) continue;
            var value = field.GetValue(source);
            targetField.SetValue(target, value);
        }
    }
}