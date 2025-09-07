using System.Collections.ObjectModel;
using System.Linq;
using HarmonyLib;
using JetBrains.Annotations;
using ULTRAPRACTICE.Helpers;
using ULTRAPRACTICE.Interfaces;
using UnityEngine;

namespace ULTRAPRACTICE.ClassSavers;

public sealed class ProjectileVariables : VariableSaver<ProjectileVariables.ProjectileProps, Projectile>
{
    public sealed class ProjectileProps : ITypeProperties<Projectile, ProjectileProps>
    {
        [UsedImplicitly]
        public GameObject gameObject;

        public Projectile BackupObject { get; set; }
        public ProjectileProps CopyFrom(Projectile other)
        {
            gameObject = other.gameObject;
            BackupObject = Object.Instantiate(other,
                                              other.gameObject.transform.position,
                                              other.transform.rotation);
            UpdateBehaviour.CopyScripts(other.gameObject, BackupObject.gameObject);
            BackupObject.rb = BackupObject.GetComponent<Rigidbody>();
            BackupObject.rb.velocity = other.rb.velocity;
            BackupObject.gameObject.SetActive(false);
            return this;
        }

        public void Restore()
        {
            var backupCopy = Object.Instantiate(BackupObject,
                                                BackupObject.transform.position,
                                                BackupObject.transform.rotation);
            UpdateBehaviour.CopyScripts(BackupObject.gameObject, backupCopy.gameObject);
            gameObject = backupCopy.gameObject;
            backupCopy.rb = backupCopy.GetComponent<Rigidbody>();
            backupCopy.rb.velocity = BackupObject.rb.velocity;
            backupCopy.gameObject.SetActive(true);
        }
    }

    public override ReadOnlyCollection<ProjectileProps> States { get; set; }

    public override void SaveVariables()
    {
        if (States != null)
            foreach (var state in States)
                Object.Destroy(state.BackupObject);

        States = Object.FindObjectsOfType<Projectile>()
                       .Select(proj => new ProjectileProps().CopyFrom(proj))
                       .ToArray().AsReadOnly();
    }

    public override void SetVariables()
    {
        Object.FindObjectsOfType<Projectile>()
              .Where(proj => proj.gameObject.activeSelf)
              .Do(proj => Object.Destroy(proj.gameObject));
        foreach (var state in States)
            state.Restore();
    }
}