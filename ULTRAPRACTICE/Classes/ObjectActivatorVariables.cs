using UnityEngine;

namespace ULTRAPRACTICE.Classes;

internal class ObjectActivatorVariables
{
    public static ObjActVars[] oav;

    public static void SaveVariables()
    {
        if (oav != null)
        {
            for (int i = 0; i < oav.Length; i++)
            {
                Object.Destroy(oav[i]);
            }
        }

        ObjectActivator[] allObjs = Object.FindObjectsOfType<ObjectActivator>(true);
        oav = new ObjActVars[allObjs.Length];

        for (int i = 0; i < allObjs.Length; i++)
        {
            oav[i] = new GameObject().AddComponent<ObjActVars>();
            UpdateBehaviour.CopyScripts(allObjs[i].gameObject, oav[i].gameObject);
            oav[i].activator = allObjs[i];
        }
    }

    public static void SetVariables()
    {

        foreach (var activator in oav)
        {
            if (activator.activator.activated && !activator.activated && activator.activator.events != null) activator.activator.events.Revert();
        }
    }
}