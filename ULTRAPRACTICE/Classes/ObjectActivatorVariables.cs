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

        ObjectActivator[] allObjs = Object.FindObjectsOfType<ObjectActivator>();
        oav = new ObjActVars[allObjs.Length];

        for (int i = 0; i < allObjs.Length; i++)
        {
            oav[i] = new GameObject().AddComponent<ObjActVars>();
            UpdateBehaviour.CopyScripts(allObjs[i].gameObject, oav[i].gameObject);
            oav[i].nme = allObjs[i].name;
        }
    }

    public static void SetVariables()
    {
        ObjectActivator[] allObjs = Object.FindObjectsOfType<ObjectActivator>();

        foreach (var activator in allObjs)
        {
            foreach (var obj in oav)
            {
                if (obj != null && obj.nme == activator.name)
                {
                    UpdateBehaviour.CopyScripts(obj.gameObject, activator.gameObject);
                }
            }

            // we revert the events if theyre not supposed to be active

            if (!activator.activated && activator.events != null)
            {
                activator.events.Revert();
            }
        }
    }
}