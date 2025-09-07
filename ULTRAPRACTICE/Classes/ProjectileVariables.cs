using ULTRAPRACTICE.Interfaces;
using UnityEngine;

namespace ULTRAPRACTICE.Classes;

public class ProjectileVariables : IVariableSaver
{
    public struct properties
    {
        public GameObject gameObject;

        public Projectile backupObject;
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

        Projectile[] allObjs = Object.FindObjectsOfType<Projectile>();
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
        Projectile[] projectiles = Object.FindObjectsOfType<Projectile>();
        for (int i = 0; i < projectiles.Length; i++)
        {
            if (projectiles[i].gameObject.activeSelf) Object.Destroy(projectiles[i].gameObject);
        }

        for (int i = 0; i < states.Length; i++)
        {
            Projectile backupCopy = Object.Instantiate(states[i].backupObject, states[i].backupObject.transform.position, states[i].backupObject.transform.rotation);
            UpdateBehaviour.CopyScripts(states[i].backupObject.gameObject, backupCopy.gameObject);
            states[i].gameObject = backupCopy.gameObject;
            backupCopy.rb = backupCopy.GetComponent<Rigidbody>();
            backupCopy.rb.velocity = states[i].backupObject.rb.velocity;
            backupCopy.gameObject.SetActive(true);
        }
    }
}