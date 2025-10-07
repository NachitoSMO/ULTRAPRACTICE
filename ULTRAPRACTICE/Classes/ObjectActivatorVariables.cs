using System.Collections.Generic;
using UnityEngine;

namespace ULTRAPRACTICE.Classes;

internal static class ObjectActivatorVariables
{
    public struct properties
    {
        public ObjectActivator gameObject;

        public ObjectActivator backupObject;

        public Vector3 pos;

        public Quaternion rot;

        public Vector3 scale;
    }

    public static properties[] states;

    public static List<GameObject> toActivateObjects = [];

    public static List<GameObject> toDisActivateObjects = [];

    public static void SaveVariables()
    {
        toActivateObjects.Clear();
        toDisActivateObjects.Clear();

        if (states != null)
        {
            for (int i = 0; i < states.Length; i++)
            {
                if (states[i].backupObject != null) Object.DestroyImmediate(states[i].backupObject.gameObject);
            }
        }

        ObjectActivator[] allObjs = Object.FindObjectsOfType<ObjectActivator>(true);
        states = new properties[allObjs.Length];

        for (int i = 0; i < allObjs.Length; i++)
        {
            states[i].gameObject = allObjs[i];
            states[i].backupObject = new GameObject().AddComponent<ObjectActivator>();
            states[i].backupObject.gameObject.SetActive(false);

            states[i].pos = allObjs[i].gameObject.transform.position;
            states[i].rot = allObjs[i].gameObject.transform.rotation;
            states[i].scale = allObjs[i].gameObject.transform.localScale;

            UpdateBehaviour.CopyScripts(states[i].gameObject.gameObject, states[i].backupObject.gameObject);

            foreach (GameObject obj in allObjs[i].events?.toActivateObjects)
            {
                if (obj == null) continue;
                if (obj.activeSelf) toActivateObjects.Add(obj);
                else toDisActivateObjects.Add(obj);
            }

            foreach (GameObject obj in allObjs[i].events?.toDisActivateObjects)
            {
                if (obj == null) continue;
                if (obj.activeSelf) toActivateObjects.Add(obj);
                else toDisActivateObjects.Add(obj);
            }

        }
    }

    public static void SetVariables()
    {

        for (int i = 0; i < states.Length; i++)
        {
            if (states[i].gameObject != null)
            {
                UpdateBehaviour.CopyScripts(states[i].backupObject.gameObject, states[i].gameObject.gameObject);
            }
            else
            {
                ObjectActivator backup = Object.Instantiate(states[i].backupObject, states[i].pos, states[i].rot);
                states[i].gameObject = backup;
                UpdateBehaviour.CopyScripts(states[i].backupObject.gameObject, backup.gameObject);
                backup.gameObject.transform.localScale = states[i].scale;
            }

            foreach (GameObject obj in toActivateObjects)
            {
                if (obj != null) obj.SetActive(true);
            }
            foreach (GameObject obj in toDisActivateObjects)
            {
                if (obj != null) obj.SetActive(false);
            }

        }
    }
}