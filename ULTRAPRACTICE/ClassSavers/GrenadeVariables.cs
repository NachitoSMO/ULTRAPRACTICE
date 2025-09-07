using System;
using System.Collections.ObjectModel;
using System.Linq;
using HarmonyLib;
using ULTRAPRACTICE.Helpers;
using ULTRAPRACTICE.Interfaces;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ULTRAPRACTICE.ClassSavers;

/// <summary>
/// grenades use the old implementation because otherwise your log gets spammed with null reference exceptions when using rockets at fixedUpdate despite the rockets
/// working just fine
/// </summary>
public sealed class GrenadeVariables : VariableSaver<GrenadeVariables.GrenadeProps, Grenade>
{
    public sealed class GrenadeProps : ITypeProperties<Grenade, GrenadeProps>
    {
        public GameObject grenadeObj = null;

        public Grenade BackupObject { get; set; } = null;

        public GrenadeProps CopyFrom(Grenade grenade)
        {
            grenadeObj = grenade.gameObject;
            BackupObject = Object.Instantiate(grenade,
                                              grenade.gameObject.transform.position,
                                              grenade.transform.rotation);
            UpdateBehaviour.CopyScripts(grenade.gameObject, BackupObject.gameObject);
            BackupObject.rb = BackupObject.GetComponent<Rigidbody>();
            BackupObject.rb.velocity = grenade.rb.velocity;
            BackupObject.gameObject.SetActive(false);
            return this;
        }

        public void Restore()
        {
            var backupCopy = Object.Instantiate(BackupObject,
                                                BackupObject.transform.position,
                                                BackupObject.transform.rotation);
            UpdateBehaviour.CopyScripts(BackupObject.gameObject, backupCopy.gameObject);
            grenadeObj = backupCopy.gameObject;
            backupCopy.rb = backupCopy.GetComponent<Rigidbody>();
            backupCopy.rb.velocity = BackupObject.rb.velocity;
            backupCopy.gameObject.SetActive(true);
        }
    }

    public override ReadOnlyCollection<GrenadeProps> States { get; set; } = Array.Empty<GrenadeProps>().AsReadOnly();

    public override void SaveVariables()
    {
        foreach (var state in States)
            Object.Destroy(state.BackupObject);

        States = Object.FindObjectsOfType<Grenade>()
                       .Select(grenade =>
                        {
                            var prop = new GrenadeProps();
                            prop.CopyFrom(grenade);
                            return prop;
                        })
                       .ToArray()
                       .AsReadOnly();
    }

    public override void SetVariables()
    {
        foreach (var grenade in Object.FindObjectsOfType<Grenade>()
                                      .Where(grenade => grenade.gameObject.activeSelf))
            Object.Destroy(grenade.gameObject);

        States.Do(state => state.Restore());
    }
}