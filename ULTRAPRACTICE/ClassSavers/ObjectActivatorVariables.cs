using System.Linq;
using ULTRAKILL.Cheats;
using ULTRAPRACTICE.Interfaces;
using UnityEngine;

namespace ULTRAPRACTICE.ClassSavers;

internal sealed class ObjectActivatorVariables : IVariableSaver
{
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

        public ObjectActivator activator;
    }

    public static ObjActVars[] objActVars;

    public void SaveVariables()
    {
        foreach (var objActVar in objActVars ?? [])
            Object.Destroy(objActVar);

        var objectActivators = Object.FindObjectsOfType<ObjectActivator>(true);
        objActVars = new ObjActVars[objectActivators.Length];

        for (var i = 0; i < objectActivators.Length; i++)
        {
            objActVars[i] = new GameObject().AddComponent<ObjActVars>();
            UpdateBehaviour.CopyScripts(objectActivators[i].gameObject, objActVars[i].gameObject);
            objActVars[i].activator = objectActivators[i];
        }
    }

    public void SetVariables()
    {
        foreach (var activator in objActVars.Where(activator => activator.activator.activated)
                                            .Where(activator => !activator.activated))
            activator.activator.events?.Revert();
    }
}