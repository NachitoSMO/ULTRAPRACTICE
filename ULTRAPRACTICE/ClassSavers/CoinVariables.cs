using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using ULTRAPRACTICE.Helpers;
using ULTRAPRACTICE.Interfaces;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ULTRAPRACTICE.ClassSavers;

public sealed class CoinVariables : VariableSaver<CoinVariables.CoinProps, Coin>
{
    /// <summary>
    /// coins use the old implementation with a hacky solution to the Invoke() issue because the way I had to get and spawn coins was really strange
    /// and hard to make it work otherwise
    /// </summary>
    public sealed class CoinTimers : MonoBehaviour
    {
        public float deleteTimer;

        public float checkSpeedTimer;

        public float tripleTimer;

        public float tripleEndTimer;

        public float doubleTimer;

    }
    public sealed class CoinProps : ITypeProperties<Coin, CoinProps>
    {
        public Vector3 pos = default;

        public Vector3 vel = default;

        public Quaternion rot = default;

        public GameObject gameObject = null;

        public float deleteTimerSaved = 0;
        public float checkSpeedTimerSaved = 0;
        public float tripleTimerSaved = 0;
        public float tripleEndTimerSaved = 0;
        public float doubleTimerSaved = 0;

        public bool invokingDeletion = false;
        public bool invokingTripleTime = false;
        public bool invokingTripleTimeEnd = false;
        public bool invokingDoubleTime = false;
        public bool invokingCheckingSpeed = false;

        public Rigidbody rb = null;

        public bool checkingSpeed = false;

        public float timeToDelete = 0;

        public Vector3 hitPoint = default;

        public Collider[] cols = null;

        public SphereCollider scol = null;

        public bool shot = false;

        [HideInInspector]
        public bool shotByEnemy = false;

        public float power = 0;

        public bool quickDraw = false;

        public Material uselessMaterial = null;

        public GameObject coinHitSound = null;

        [HideInInspector]
        public int hitTimes = 0;

        public bool doubled = false;

        public int ricochets = 0;

        [HideInInspector]
        public int difficulty = 0;

        public bool dontDestroyOnPlayerRespawn = false;

        public bool ignoreBlessedEnemies = false;

        [Obsolete("Don't use this, it's just here to satisfy the interface requirements")]
        public Coin BackupObject
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public CoinProps CopyFrom(Coin coin)
        {
            power = coin.power;
            ricochets = coin.ricochets;
            shot = coin.shot;
            shotByEnemy = coin.shotByEnemy;
            timeToDelete = coin.timeToDelete;
            hitTimes = coin.hitTimes;
            doubled = coin.doubled;
            checkingSpeed = coin.checkingSpeed;

            pos = coin.transform.position;
            rot = coin.transform.rotation;
            vel = coin.rb.velocity;
            gameObject = coin.gameObject;

            checkSpeedTimerSaved = coin.GetComponent<CoinTimers>().checkSpeedTimer;
            deleteTimerSaved = coin.GetComponent<CoinTimers>().deleteTimer;
            tripleTimerSaved = coin.GetComponent<CoinTimers>().tripleTimer;
            tripleEndTimerSaved = coin.GetComponent<CoinTimers>().tripleEndTimer;
            doubleTimerSaved = coin.GetComponent<CoinTimers>().doubleTimer;

            invokingDeletion = coin.IsInvoking(nameof(Coin.GetDeleted));
            invokingTripleTime = coin.IsInvoking(nameof(Coin.TripleTime));
            invokingDoubleTime = coin.IsInvoking(nameof(Coin.DoubleTime));
            invokingTripleTimeEnd = coin.IsInvoking(nameof(Coin.TripleTimeEnd));
            invokingCheckingSpeed = coin.IsInvoking(nameof(Coin.StartCheckingSpeed));
            return this;
        }

        public void Restore()
        {
            var backup = Object.Instantiate(Plugin.Instance.coin, pos, rot);
            gameObject = backup;
            var coin = backup.GetComponentInChildren<Coin>();
            coin.transform.position = pos;
            coin.transform.rotation = rot;
            coin.rb.velocity = vel;
            coin.timeToDelete = timeToDelete;
            coin.power = power;
            coin.ricochets = ricochets;
            coin.shot = shot;
            coin.shotByEnemy = shotByEnemy;
            coin.hitTimes = hitTimes;
            coin.doubled = doubled;
            coin.checkingSpeed = checkingSpeed;
        }
    }

    public override ReadOnlyCollection<CoinProps> States { get; set; }

    public override void SetVariables()
    {
        var coins = Object.FindObjectsOfType<Coin>();
        foreach (var coin in coins)
            Object.Destroy(coin.gameObject);

        foreach (var state in States)
        {
            state.Restore();
            MonoSingleton<UpdateBehaviour>.Instance.StartCoroutine(RestoreInvokes(state.gameObject.GetComponentInChildren<Coin>(), state));
        }
    }

    // we do this a frame later because otherwise the coins will start their invokes *after* we cancel them
    private static IEnumerator RestoreInvokes(Coin coin, CoinProps state)
    {
        yield return new WaitForFixedUpdate();

        if (coin.checkingSpeed)
            foreach (var collider in coin.GetComponents<Collider>())
                collider.enabled = true;

        coin.CancelInvoke(nameof(Coin.StartCheckingSpeed));
        coin.CancelInvoke(nameof(Coin.GetDeleted));
        coin.CancelInvoke(nameof(Coin.TripleTime));
        coin.CancelInvoke(nameof(Coin.TripleTimeEnd));
        coin.CancelInvoke(nameof(Coin.DoubleTime));

        if (state.invokingCheckingSpeed)
            coin.Invoke(nameof(Coin.StartCheckingSpeed), 0.1f - state.checkSpeedTimerSaved);
        if (state.invokingTripleTime)
            coin.Invoke(nameof(Coin.TripleTime), 0.35f - state.deleteTimerSaved);
        if (state.invokingDoubleTime)
            coin.Invoke(nameof(Coin.DoubleTime), 1f - state.deleteTimerSaved);
        if (state.invokingTripleTimeEnd)
            coin.Invoke(nameof(Coin.TripleTimeEnd), 0.417f - state.deleteTimerSaved);
        if (state.invokingDeletion)
            coin.Invoke(nameof(Coin.GetDeleted), 5f - state.deleteTimerSaved);
    }
}