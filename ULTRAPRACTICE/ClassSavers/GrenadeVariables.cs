using System.Linq;
using ULTRAPRACTICE.Interfaces;
using UnityEngine;

namespace ULTRAPRACTICE.ClassSavers;

/// <summary>
/// grenades use the old implementation because otherwise your log gets spammed with null reference exceptions when using rockets at fixedUpdate despite the rockets
/// working just fine
/// </summary>
public sealed class GrenadeVariables : IVariableSaver
{
    public class Properties
    {
        public GameObject gameObject = null;

        public Grenade backupObject = null;
    }

    public static Properties[] states;

    public void SaveVariables()
    {
        if (states != null)
        {
            for (int i = 0; i < states.Length; i++)
            {
                Object.Destroy(states[i].backupObject);
            }
        }

        var allObjs = Object.FindObjectsOfType<Grenade>();
        states = new Properties[allObjs.Length];

        for (int i = 0; i < allObjs.Length; i++)
        {
            states[i] = new Properties
            {
                gameObject = allObjs[i].gameObject,
                backupObject = Object.Instantiate(allObjs[i],
                                                  allObjs[i].gameObject.transform.position,
                                                  allObjs[i].transform.rotation)
            };
            UpdateBehaviour.CopyScripts(allObjs[i].gameObject, states[i].backupObject.gameObject);
            states[i].backupObject.rb = states[i].backupObject.GetComponent<Rigidbody>();
            states[i].backupObject.rb.velocity = allObjs[i].rb.velocity;
            states[i].backupObject.gameObject.SetActive(false);

        }
    }

    public void SetVariables()
    {
        var Grenades = Object.FindObjectsOfType<Grenade>();
        foreach (var grenade in Grenades.Where(grenade => grenade.gameObject.activeSelf))
            Object.Destroy(grenade.gameObject);

        foreach (var state in states)
        {
            var backupCopy = Object.Instantiate(state.backupObject, state.backupObject.transform.position, state.backupObject.transform.rotation);
            UpdateBehaviour.CopyScripts(state.backupObject.gameObject, backupCopy.gameObject);
            state.gameObject = backupCopy.gameObject;
            backupCopy.rb = backupCopy.GetComponent<Rigidbody>();
            backupCopy.rb.velocity = state.backupObject.rb.velocity;
            backupCopy.gameObject.SetActive(true);
        }
    }
}