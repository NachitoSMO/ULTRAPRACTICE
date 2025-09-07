using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using HarmonyLib;
using ULTRAPRACTICE.Helpers;
using ULTRAPRACTICE.Interfaces;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ULTRAPRACTICE.ClassSavers;

public sealed class V2Variables : IVariableSaver
{
    public sealed class V2Properties : ITypeProperties<V2>
    {
        public V2 v2Obj;

        public GameObject backupObject;

        public Vector3 vel;

        public bool kinematic;

        public void CopyFrom(V2 other)
        {
            v2Obj = other;
            vel = other.rb.velocity;
            kinematic = other.rb.isKinematic;
        }

        public void CopyTo(V2 other)
        {
            v2Obj.gameObject.SetActive(false);
            v2Obj.gc.onGround = false;
            v2Obj.gameObject.transform.position = backupObject.transform.position;
            v2Obj.gameObject.transform.rotation = backupObject.transform.rotation;
            v2Obj.gameObject.SetActive(true);
        }
    }

    public static ReadOnlyCollection<V2Properties> states;

    public void SaveVariables()
    {
        states =
            Object.FindObjectsOfType<V2>()
                  .Select(v2 =>
                   {
                       var props = new V2Properties();
                       props.CopyFrom(v2);
                       props.backupObject = Object.Instantiate(v2.gameObject,
                                                               v2.gameObject.transform.position,
                                                               v2.gameObject.transform.rotation);
                       props.backupObject.SetActive(false);
                       UpdateBehaviour.CopyScripts(v2.gameObject, props.backupObject);
                       return props;
                   })
                  .ToArray()
                  .AsReadOnly();

    }

    public void SetVariables()
    {
        states.Where(state => state.v2Obj && state.backupObject)
              .Do(state =>
               {
                   // we set v2 inactive and active again right after as a hacky way for v2
                   // to not immediately do a stomp after we teleport it mid air
                   state.CopyTo(state.v2Obj);
                   // also found we might? need a single frame before we do the next part because otherwise weird stuff happens
                   MonoSingleton<UpdateBehaviour>.Instance.StartCoroutine(SetVelocityAfter(state));
               });
    }

    public static IEnumerator SetVelocityAfter(V2Properties state)
    {
        yield return new WaitForFixedUpdate();
        var rb = state.v2Obj.GetComponent<Rigidbody>();
        rb.isKinematic = false;
        if (rb)
        {
            rb.velocity = state.vel;
            rb.isKinematic = state.kinematic;
        }
        state.v2Obj.gc.CheckColsOnce();

        UpdateBehaviour.CopyScripts(state.backupObject, state.v2Obj.gameObject);
    }
}