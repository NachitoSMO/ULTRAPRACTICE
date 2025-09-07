using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using HarmonyLib;
using ULTRAPRACTICE.Interfaces;
using UnityEngine;

namespace ULTRAPRACTICE.ClassSavers;

public sealed class V2Variables : VariableSaver<V2Variables.V2Properties, V2>
{
    public sealed class V2Properties : ITypeProperties<V2, V2Properties>
    {
        public V2 v2Obj;

        public V2 BackupObject { get; set; }

        public Vector3 vel;

        public bool kinematic;

        public V2Properties CopyFrom(V2 other)
        {
            v2Obj = other;
            vel = other.rb.velocity;
            kinematic = other.rb.isKinematic;
            BackupObject = Object.Instantiate(other,
                                              other.gameObject.transform.position,
                                              other.gameObject.transform.rotation);
            BackupObject.gameObject.SetActive(false);
            UpdateBehaviour.CopyScripts(other.gameObject, BackupObject.gameObject);
            return this;
        }

        public void Restore()
        {
            v2Obj.gameObject.SetActive(false);
            v2Obj.gc.onGround = false;
            v2Obj.gameObject.transform.position = BackupObject.transform.position;
            v2Obj.gameObject.transform.rotation = BackupObject.transform.rotation;
            v2Obj.gameObject.SetActive(true);
        }
    }

    public override ReadOnlyCollection<V2Properties> States { get; set; }

    public override void SetVariables()
    {
        States.Where(state => state.v2Obj && state.BackupObject)
              .Do(state =>
               {
                   // we set v2 inactive and active again right after as a hacky way for v2
                   // to not immediately do a stomp after we teleport it mid air
                   state.Restore();
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

        UpdateBehaviour.CopyScripts(state.BackupObject.gameObject, state.v2Obj.gameObject);
    }
}