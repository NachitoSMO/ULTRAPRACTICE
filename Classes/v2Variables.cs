using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ULTRAPRACTICE.Classes
{
    public static class v2Variables
    {
        public struct properties
        {
            public V2 gameObject;

            public GameObject backupObject;

            public Vector3 vel;

            public bool kinematic;
        }

        public static properties[] states;

        public static void SaveVariables()
        {
            V2[] allObjs = GameObject.FindObjectsOfType<V2>();
            states = new properties[allObjs.Length];

            for (int i = 0; i < allObjs.Length; i++)
            {
                states[i].gameObject = allObjs[i];

                states[i].backupObject = GameObject.Instantiate(allObjs[i].gameObject, allObjs[i].gameObject.transform.position, allObjs[i].gameObject.transform.rotation);
                states[i].backupObject.SetActive(false);
                states[i].vel = allObjs[i].rb.velocity;
                states[i].kinematic = allObjs[i].rb.isKinematic;

                UpdateBehaviour.CopyScripts(allObjs[i].gameObject, states[i].backupObject);

            }
        }

        public static void SetVariables()
        {
            for (int i = 0; i < states.Length; i++)
            {
                if (states[i].gameObject != null && states[i].backupObject != null)
                {
                    //we set v2 inactive and active again right after as a hacky way for v2 to not immediately do a stomp after we teleport it mid air
                    states[i].gameObject.gameObject.SetActive(false);
                    states[i].gameObject.gc.onGround = false;
                    states[i].gameObject.gameObject.transform.position = states[i].backupObject.transform.position;
                    states[i].gameObject.gameObject.transform.rotation = states[i].backupObject.transform.rotation;

                    states[i].gameObject.gameObject.SetActive(true);
                    // also found we might? need a single frame before we do the next part because otherwise weird stuff happens
                    MonoSingleton<UpdateBehaviour>.Instance.StartCoroutine(SetVelocityAfter(i));
                }
            }
        }

        public static IEnumerator SetVelocityAfter(int i)
        {
            yield return new WaitForFixedUpdate();
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
