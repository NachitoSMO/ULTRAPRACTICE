using System.Collections.Generic;
using UnityEngine;

namespace ULTRAPRACTICE.Classes
{
    /// <summary>
    /// grenades use the old implementation because otherwise your log gets spammed with null reference exceptions when using rockets at fixedUpdate despite the rockets
    /// working just fine
    /// </summary>

    public static class grenadeVariables
    {
        public struct GrenadeProperties
        {
            public Vector3 pos;

            public Vector3 vel;

            public Quaternion rot;

            public GameObject gameObject;

            public GameObject backupObject;

            public string hitterWeapon;

            public GameObject sourceWeapon;

            public GameObject explosion;

            public GameObject harmlessExplosion;

            public GameObject superExplosion;

            public bool exploded;

            public bool enemy;

            [HideInInspector]
            public EnemyIdentifier originEnemy;

            public float totalDamageMultiplier;

            public bool rocket;

            [HideInInspector]
            public Rigidbody rb;

            [HideInInspector]
            public List<Magnet> magnets;

            [HideInInspector]
            public Magnet latestEnemyMagnet;

            public float rocketSpeed;

            public bool playerRiding;

            public bool playerInRidingRange;

            public float downpull;

            public GameObject playerRideSound;

            [HideInInspector]
            public bool rideable;

            [HideInInspector]
            public bool hooked;

            public bool hasBeenRidden;

            public EnemyTarget proximityTarget;

            public GameObject proximityWindup;

            [HideInInspector]
            public bool levelledUp;

            [HideInInspector]
            public float timeFrozen;

            public List<EnemyType> ignoreEnemyType;
        }

        public static GrenadeProperties[] grenadeStates;

        public static void SaveVariables()
        {
            if (grenadeStates != null)
            {
                for (int i = 0; i < grenadeStates.Length; i++)
                {
                    Object.Destroy(grenadeStates[i].backupObject);
                }
            }
            Grenade[] array = Object.FindObjectsOfType<Grenade>();
            grenadeStates = new GrenadeProperties[array.Length];
            for (int j = 0; j < array.Length; j++)
            {
                grenadeStates[j].enemy = array[j].enemy;
                grenadeStates[j].exploded = array[j].exploded;
                grenadeStates[j].totalDamageMultiplier = array[j].totalDamageMultiplier;
                grenadeStates[j].rocket = array[j].rocket;
                grenadeStates[j].rocketSpeed = array[j].rocketSpeed;
                grenadeStates[j].hasBeenRidden = array[j].hasBeenRidden;
                grenadeStates[j].downpull = array[j].downpull;
                grenadeStates[j].playerRiding = array[j].playerRiding;
                grenadeStates[j].pos = array[j].transform.position;
                grenadeStates[j].rot = array[j].transform.rotation;
                grenadeStates[j].vel = array[j].rb.velocity;
                grenadeStates[j].rb = array[j].rb;
                grenadeStates[j].gameObject = array[j].gameObject;
                grenadeStates[j].backupObject = Object.Instantiate(array[j].gameObject);
                grenadeStates[j].backupObject.SetActive(false);
            }
        }

        public static void SetVariables()
        {
            Grenade[] array = Object.FindObjectsOfType<Grenade>();
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].isActiveAndEnabled) Object.Destroy(array[i].gameObject);
            }
            for (int j = 0; j < grenadeStates.Length; j++)
            {
                GameObject val = Object.Instantiate(grenadeStates[j].backupObject, grenadeStates[j].pos, grenadeStates[j].rot);
                val.SetActive(true);
                grenadeStates[j].gameObject = val;
                Grenade component = val.GetComponent<Grenade>();
                SetGrenadeVars(component, j);
            }
        }

        public static void SetGrenadeVars(Grenade component, int i)
        {
            component.gameObject.transform.position = grenadeStates[i].pos;
            component.gameObject.transform.rotation = grenadeStates[i].rot;
            component.rb.velocity = grenadeStates[i].vel;
            component.enemy = grenadeStates[i].enemy;
            component.exploded = grenadeStates[i].exploded;
            component.totalDamageMultiplier = grenadeStates[i].totalDamageMultiplier;
            component.rocket = grenadeStates[i].rocket;
            component.rocketSpeed = grenadeStates[i].rocketSpeed;
            component.hasBeenRidden = grenadeStates[i].hasBeenRidden;
            component.downpull = grenadeStates[i].downpull;
            component.playerInRidingRange = grenadeStates[i].playerInRidingRange;
        }
    }
}

