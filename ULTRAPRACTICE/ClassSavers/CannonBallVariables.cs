using ULTRAPRACTICE.Interfaces;
using UnityEngine;

namespace ULTRAPRACTICE.ClassSavers;

public sealed class CannonBallVariables : IVariableSaver
{
    public struct properties
    {
        public GameObject gameObject;

        public Cannonball backupObject;
    }

    public static properties[] states;

    public void SaveVariables()
    {
        if (states != null)
        {
            for (int i = 0; i < states.Length; i++)
            {
                Object.Destroy(states[i].backupObject);
            }
        }

        Cannonball[] allObjs = Object.FindObjectsOfType<Cannonball>();
        states = new properties[allObjs.Length];

        for (int i = 0; i < allObjs.Length; i++)
        {
            states[i].gameObject = allObjs[i].gameObject;
            states[i].backupObject = Object.Instantiate(allObjs[i], allObjs[i].gameObject.transform.position, allObjs[i].transform.rotation);

            UpdateBehaviour.CopyScripts(allObjs[i].gameObject, states[i].backupObject.gameObject);

            states[i].backupObject.rb = states[i].backupObject.GetComponent<Rigidbody>();
            states[i].backupObject.rb.velocity = allObjs[i].rb.velocity;
            states[i].backupObject.gameObject.SetActive(false);

        }
    }

    public void SetVariables()
    {
        Cannonball[] projectiles = Object.FindObjectsOfType<Cannonball>();
        for (int i = 0; i < projectiles.Length; i++)
        {
            if (projectiles[i].gameObject.activeSelf) Object.Destroy(projectiles[i].gameObject);
        }

        for (int i = 0; i < states.Length; i++)
        {
            Cannonball backupCopy = Object.Instantiate(states[i].backupObject, states[i].backupObject.transform.position, states[i].backupObject.transform.rotation);
            UpdateBehaviour.CopyScripts(states[i].backupObject.gameObject, backupCopy.gameObject);
            states[i].gameObject = backupCopy.gameObject;
            backupCopy.rb = backupCopy.GetComponent<Rigidbody>();
            backupCopy.rb.velocity = states[i].backupObject.rb.velocity;
            backupCopy.gameObject.SetActive(true);
        }
    }
}