using System.Collections;
using System.Linq;
using ULTRAPRACTICE.Interfaces;
using UnityEngine;

namespace ULTRAPRACTICE.Classes;

public static class CoinVariables
{
    public sealed class CoinProps : ITypeProperties<Coin>
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

        public void CopyFrom(Coin coin)
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
        }

        public void CopyTo(Coin coin)
        {
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

    public static CoinProps[] states;

    public static void SaveVariables()
    {
        var coins = Object.FindObjectsOfType<Coin>();
        states = coins
                .Select(coin =>
                 {
                     var prop = new CoinProps();
                     prop.CopyFrom(coin);
                     return prop;
                 })
                .ToArray();
    }

    public static void SetVariables()
    {
        var coins = Object.FindObjectsOfType<Coin>();
        foreach (var coin in coins)
            Object.Destroy(coin.gameObject);

        for (var i = 0; i < states.Length; i++)
        {
            var backupCopy = Object.Instantiate(Plugin.Instance.coin, states[i].pos, states[i].rot);
            var coinComp = backupCopy.GetComponentInChildren<Coin>();
            states[i].gameObject = backupCopy;
            SetVars(coinComp, i);
        }
    }

    public static void SetVars(Coin coin, int i)
    {
        states[i].CopyTo(coin);
        MonoSingleton<UpdateBehaviour>.Instance.StartCoroutine(RestoreInvokes(coin, i));
    }

    // we do this a frame later because otherwise the coins will start their invokes *after* we cancel them
    private static IEnumerator RestoreInvokes(Coin coin, int i)
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

        if (states[i].invokingCheckingSpeed)
            coin.Invoke(nameof(Coin.StartCheckingSpeed), 0.1f - states[i].checkSpeedTimerSaved);
        if (states[i].invokingTripleTime)
            coin.Invoke(nameof(Coin.TripleTime), 0.35f - states[i].deleteTimerSaved);
        if (states[i].invokingDoubleTime)
            coin.Invoke(nameof(Coin.DoubleTime), 1f - states[i].deleteTimerSaved);
        if (states[i].invokingTripleTimeEnd)
            coin.Invoke(nameof(Coin.TripleTimeEnd), 0.417f - states[i].deleteTimerSaved);
        if (states[i].invokingDeletion)
            coin.Invoke(nameof(Coin.GetDeleted), 5f - states[i].deleteTimerSaved);
    }
}