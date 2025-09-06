using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ULTRAPRACTICE.Classes
{
    public static class CannonBallVariables
    {
        public struct properties
        {
            public GameObject gameObject;

            public Cannonball backupObject;
        }

        public static properties[] states;

        public static void SaveVariables()
        {
            if (states != null)
            {
                for (int i = 0; i < states.Length; i++)
                {
                    UnityEngine.Object.Destroy(states[i].backupObject);
                }
            }

            Cannonball[] allObjs = GameObject.FindObjectsOfType<Cannonball>();
            states = new properties[allObjs.Length];

            for (int i = 0; i < allObjs.Length; i++)
            {
                states[i].gameObject = allObjs[i].gameObject;
                states[i].backupObject = GameObject.Instantiate(allObjs[i], allObjs[i].gameObject.transform.position, allObjs[i].transform.rotation);

                UpdateBehaviour.CopyScripts(allObjs[i].gameObject, states[i].backupObject.gameObject);

                states[i].backupObject.rb = states[i].backupObject.GetComponent<Rigidbody>();
                states[i].backupObject.rb.velocity = allObjs[i].rb.velocity;
                states[i].backupObject.gameObject.SetActive(false);

            }
        }

        public static void SetVariables()
        {
            Cannonball[] projectiles = GameObject.FindObjectsOfType<Cannonball>();
            for (int i = 0; i < projectiles.Length; i++)
            {
                if (projectiles[i].gameObject.activeSelf) GameObject.Destroy(projectiles[i].gameObject);
            }

            for (int i = 0; i < states.Length; i++)
            {
                Cannonball backupCopy = GameObject.Instantiate(states[i].backupObject, states[i].backupObject.transform.position, states[i].backupObject.transform.rotation);
                UpdateBehaviour.CopyScripts(states[i].backupObject.gameObject, backupCopy.gameObject);
                states[i].gameObject = backupCopy.gameObject;
                backupCopy.rb = backupCopy.GetComponent<Rigidbody>();
                backupCopy.rb.velocity = states[i].backupObject.rb.velocity;
                backupCopy.gameObject.SetActive(true);
            }
        }
    }
}
