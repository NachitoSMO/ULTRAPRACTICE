using ULTRAPRACTICE;
using ULTRAPRACTICE.Classes;
using UnityEngine;

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

        private bool wasShotByEnemy;

        public float power;

        private EnemyIdentifier eid;

        public bool quickDraw;

        public Material uselessMaterial;

        private GameObject altBeam;

        public GameObject coinHitSound;

        [HideInInspector]
        public int hitTimes;

        public bool doubled;

        private StyleHUD shud;

        public int ricochets;

        [HideInInspector]
        public int difficulty;

        public bool dontDestroyOnPlayerRespawn;

        public bool ignoreBlessedEnemies;
    }

    public static properties[] states;

    public static void SaveVariables()
    {
        Coin[] array = Object.FindObjectsOfType<Coin>();
        states = new properties[array.Length];
        for (int i = 0; i < array.Length; i++)
        {
            states[i].power = array[i].power;
            states[i].ricochets = array[i].ricochets;
            states[i].shot = array[i].shot;
            states[i].shotByEnemy = array[i].shotByEnemy;
            states[i].timeToDelete = array[i].timeToDelete;
            states[i].hitTimes = array[i].hitTimes;
            states[i].doubled = array[i].doubled;
            states[i].checkingSpeed = array[i].checkingSpeed;
            states[i].pos = array[i].transform.position;
            states[i].rot = array[i].transform.rotation;
            states[i].vel = array[i].rb.velocity;
            states[i].gameObject = array[i].gameObject;
            states[i].checkSpeedTimerSaved = array[i].GetComponent<CoinTimers>().checkSpeedTimer;
            states[i].deleteTimerSaved = array[i].GetComponent<CoinTimers>().deleteTimer;
            states[i].tripleTimerSaved = array[i].GetComponent<CoinTimers>().tripleTimer;
            states[i].tripleEndTimerSaved = array[i].GetComponent<CoinTimers>().tripleEndTimer;
            states[i].doubleTimerSaved = array[i].GetComponent<CoinTimers>().doubleTimer;
            states[i].invokingDeletion = array[i].IsInvoking("GetDeleted");
            states[i].invokingTripleTime = array[i].IsInvoking("TripleTime");
            states[i].invokingDoubleTime = array[i].IsInvoking("DoubleTime");
            states[i].invokingTripleTimeEnd = array[i].IsInvoking("TripleTimeEnd");
            states[i].invokingCheckingSpeed = array[i].IsInvoking("StartCheckingSpeed");
        }
    }

    public static void SetVariables()
    {
        for (int j = 0; j < states.Length; j++)
        {
            Object.Destroy(states[j].gameObject);
            GameObject gameObject = Object.Instantiate(Plugin.Instance.coin, states[j].pos, states[j].rot);
            Coin componentInChildren = gameObject.GetComponentInChildren<Coin>();
            states[j].gameObject = gameObject;
            SetVars(componentInChildren, j);
        }

        MonoSingleton<UpdateBehaviour>.Instance.Invoke("RestoreCoinInvokes", 0.01f);
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
    }
}
