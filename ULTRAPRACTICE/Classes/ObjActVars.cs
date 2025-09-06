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