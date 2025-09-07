using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ULTRAPRACTICE.Classes;

internal static class ObjectActivatorVariables
{
    public struct properties
    {
        public ObjectActivator gameObject;

        public ObjectActivator backupObject;
    }

    public static properties[] states;

    public static void SaveVariables()
    {
        if (states != null)
        {
            for (int i = 0; i < states.Length; i++)
            {
                if (states[i].backupObject == null) continue;
                Object.Destroy(states[i].backupObject.gameObject);
            }
        }

        ObjectActivator[] allObjs = Object.FindObjectsOfType<ObjectActivator>();
        states = new properties[allObjs.Length];

        for (int i = 0; i < allObjs.Length; i++)
        {
            states[i].gameObject = allObjs[i];
            states[i].backupObject = Object.Instantiate(states[i].gameObject, states[i].gameObject.transform.position, states[i].gameObject.transform.rotation);

            UpdateBehaviour.CopyScripts(states[i].gameObject.gameObject, states[i].backupObject.gameObject);

            states[i].backupObject.gameObject.SetActive(false);

        }
    }

    public static void SetVariables()
    {

        for (int i = 0; i < states.Length; i++)
        {
            if (states[i].backupObject.activated) continue;
            states[i].backupObject.events?.Revert();
        }
    }
}