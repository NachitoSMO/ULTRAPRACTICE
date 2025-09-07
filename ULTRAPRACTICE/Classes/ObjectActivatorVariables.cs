using System.Linq;
using ULTRAPRACTICE.Interfaces;
using UnityEngine;

namespace ULTRAPRACTICE.Classes;

internal sealed class ObjectActivatorVariables : IVariableSaver
{
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