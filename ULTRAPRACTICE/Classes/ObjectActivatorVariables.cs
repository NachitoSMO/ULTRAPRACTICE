using ULTRAKILL.Cheats;
using UnityEngine;

namespace ULTRAPRACTICE.Classes;

public sealed class ObjActVars : MonoBehaviour
{
    public bool oneTime;

    public bool disableOnExit;

    public bool dontActivateOnEnable;

    public bool reactivateOnEnable;

    public bool activateOnDisable;

    public bool forEnemies;

    public bool notIfEnemiesDisabled;

    public bool onlyIfPlayerIsAlive;

    public bool dontUseEventsIfEnemiesDisabled;

    [HideInInspector]
    public bool activated;

    [HideInInspector]
    public bool activating;

    public float delay;

    public bool nonCollider;

    public int playerIn;

    [Space(20f)]
    public Collider[] ignoreColliders;

    [Space(20f)]
    public ObjectActivationCheck obac;

    public bool onlyCheckObacOnce;

    public bool disableIfObacOff;

    [Space(10f)]
    public UltrakillEvent events;

    private bool canUseEvents
    {
        get
        {
            if (DisableEnemySpawns.DisableArenaTriggers)
            {
                return !dontUseEventsIfEnemiesDisabled;
            }
            return true;
        }
    }

    public string nme;
}

internal class ObjectActivatorVariables
{
    public static ObjActVars[] oav;

    public static void SaveVariables()
    {
        if (oav != null)
        {
            for (int i = 0; i < oav.Length; i++)
            {
                UnityEngine.Object.Destroy(oav[i]);
            }
        }

        ObjectActivator[] allObjs = GameObject.FindObjectsOfType<ObjectActivator>();
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
        ObjectActivator[] allObjs = GameObject.FindObjectsOfType<ObjectActivator>();

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