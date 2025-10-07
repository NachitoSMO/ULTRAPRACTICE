using System.Collections.Generic;
using UnityEngine;

namespace ULTRAPRACTICE.Classes;

/// <summary>
/// grenades use the old implementation because otherwise your log gets spammed with null reference exceptions when using rockets at fixedUpdate despite the rockets
/// working just fine
/// </summary>
public static class grenadeVariables
{
    public struct properties
    {
        public GameObject gameObject;

        public Grenade backupObject;
    }

    public static properties[] states;

    public static void SaveVariables()
    {
        if (states != null)
        {
            for (int i = 0; i < states.Length; i++)
            {
                Object.Destroy(states[i].backupObject.gameObject);
            }
        }

        Grenade[] allObjs = Object.FindObjectsOfType<Grenade>();
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

    public static void SetVariables()
    {
        Grenade[] Grenades = Object.FindObjectsOfType<Grenade>();
        for (int i = 0; i < Grenades.Length; i++)
        {
            if (Grenades[i].gameObject.activeSelf) Object.Destroy(Grenades[i].gameObject);
        }

        for (int i = 0; i < states.Length; i++)
        {
            Grenade backupCopy = Object.Instantiate(states[i].backupObject, states[i].backupObject.transform.position, states[i].backupObject.transform.rotation);
            UpdateBehaviour.CopyScripts(states[i].backupObject.gameObject, backupCopy.gameObject);
            states[i].gameObject = backupCopy.gameObject;
            backupCopy.rb = backupCopy.GetComponent<Rigidbody>();
            backupCopy.rb.velocity = states[i].backupObject.rb.velocity;
            backupCopy.gameObject.SetActive(true);
        }
    }
}