using System.Collections;
using UnityEngine;

namespace ULTRAPRACTICE.Classes;

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

public static class coinVariables
{
    public struct properties
    {
        public Vector3 pos;

        public Vector3 vel;

        public Quaternion rot;

        public GameObject gameObject;

        public float deleteTimerSaved;
        public float checkSpeedTimerSaved;
        public float tripleTimerSaved;
        public float tripleEndTimerSaved;
        public float doubleTimerSaved;

        public bool invokingDeletion;
        public bool invokingTripleTime;
        public bool invokingTripleTimeEnd;
        public bool invokingDoubleTime;
        public bool invokingCheckingSpeed;

        public Rigidbody rb;

        public bool checkingSpeed;

        public float timeToDelete;

        public Vector3 hitPoint;

        public Collider[] cols;

        public SphereCollider scol;

        public bool shot;

        [HideInInspector]
        public bool shotByEnemy;

        public float power;

        public bool quickDraw;

        public Material uselessMaterial;

        public GameObject coinHitSound;

        [HideInInspector]
        public int hitTimes;

        public bool doubled;

        public int ricochets;

        [HideInInspector]
        public int difficulty;

        public bool dontDestroyOnPlayerRespawn;

        public bool ignoreBlessedEnemies;
    }

    public static properties[] states;

    public static void SaveVariables()
    {

        Coin[] allObjs = Object.FindObjectsOfType<Coin>();
        states = new properties[allObjs.Length];

        for (int i = 0; i < allObjs.Length; i++)
        {
            states[i].power = allObjs[i].power;
            states[i].ricochets = allObjs[i].ricochets;
            states[i].shot = allObjs[i].shot;
            states[i].shotByEnemy = allObjs[i].shotByEnemy;
            states[i].timeToDelete = allObjs[i].timeToDelete;
            states[i].hitTimes = allObjs[i].hitTimes;
            states[i].doubled = allObjs[i].doubled;
            states[i].checkingSpeed = allObjs[i].checkingSpeed;

            states[i].pos = allObjs[i].transform.position;
            states[i].rot = allObjs[i].transform.rotation;
            states[i].vel = allObjs[i].rb.velocity;
            states[i].gameObject = allObjs[i].gameObject;

            states[i].checkSpeedTimerSaved = allObjs[i].GetComponent<CoinTimers>().checkSpeedTimer;
            states[i].deleteTimerSaved = allObjs[i].GetComponent<CoinTimers>().deleteTimer;
            states[i].tripleTimerSaved = allObjs[i].GetComponent<CoinTimers>().tripleTimer;
            states[i].tripleEndTimerSaved = allObjs[i].GetComponent<CoinTimers>().tripleEndTimer;
            states[i].doubleTimerSaved = allObjs[i].GetComponent<CoinTimers>().doubleTimer;
            states[i].invokingDeletion = allObjs[i].IsInvoking(nameof(Coin.GetDeleted));
            states[i].invokingTripleTime = allObjs[i].IsInvoking(nameof(Coin.TripleTime));
            states[i].invokingDoubleTime = allObjs[i].IsInvoking(nameof(Coin.DoubleTime));
            states[i].invokingTripleTimeEnd = allObjs[i].IsInvoking(nameof(Coin.TripleTimeEnd));
            states[i].invokingCheckingSpeed = allObjs[i].IsInvoking(nameof(Coin.StartCheckingSpeed));

        }
    }

    public static void SetVariables()
    {
        Coin[] coins = Object.FindObjectsOfType<Coin>();
        for (int i = 0; i < coins.Length; i++)
        {
            Object.Destroy(coins[i].gameObject);

        }

        for (int i = 0; i < states.Length; i++)
        {
            GameObject backupCopy = Object.Instantiate(Plugin.Instance.coin, states[i].pos, states[i].rot);
            Coin component = backupCopy.GetComponentInChildren<Coin>();
            states[i].gameObject = backupCopy;
            SetVars(component, i);
        }
    }

    public static void SetVars(Coin component, int i)
    {
        component.transform.position = states[i].pos;
        component.transform.rotation = states[i].rot;
        component.GetComponent<Rigidbody>().velocity = states[i].vel;

        component.timeToDelete = states[i].timeToDelete;
        component.power = states[i].power;
        component.ricochets = states[i].ricochets;
        component.shot = states[i].shot;
        component.shotByEnemy = states[i].shotByEnemy;
        component.hitTimes = states[i].hitTimes;
        component.doubled = states[i].doubled;
        component.checkingSpeed = states[i].checkingSpeed;

        MonoSingleton<UpdateBehaviour>.Instance.StartCoroutine(RestoreInvokes(component, i));
    }

    // we do this a frame later because otherwise the coins will start their invokes *after* we cancel them
    private static IEnumerator RestoreInvokes(Coin component, int i)
    {
        yield return new WaitForFixedUpdate();

        if (component.checkingSpeed)
        {
            Collider[] array = component.GetComponents<Collider>();
            for (int j = 0; j < array.Length; j++)
            {
                array[j].enabled = true;
            }
        }

        component.CancelInvoke(nameof(Coin.StartCheckingSpeed));
        component.CancelInvoke(nameof(Coin.GetDeleted));
        component.CancelInvoke(nameof(Coin.TripleTime));
        component.CancelInvoke(nameof(Coin.TripleTimeEnd));
        component.CancelInvoke(nameof(Coin.DoubleTime));

        if (states[i].invokingCheckingSpeed)
        {
            component.Invoke(nameof(Coin.StartCheckingSpeed), 0.1f - states[i].checkSpeedTimerSaved);
        }

        if (states[i].invokingTripleTime)
        {
            component.Invoke(nameof(Coin.TripleTime), 0.35f - states[i].deleteTimerSaved);
        }
        if (states[i].invokingDoubleTime)
        {
            component.Invoke(nameof(Coin.DoubleTime), 1f - states[i].deleteTimerSaved);
        }

        if (states[i].invokingTripleTimeEnd)
        {
            component.Invoke(nameof(Coin.TripleTimeEnd), 0.417f - states[i].deleteTimerSaved);
        }
        if (states[i].invokingDeletion)
        {
            component.Invoke(nameof(Coin.GetDeleted), 5f - states[i].deleteTimerSaved);
        }
    }
}