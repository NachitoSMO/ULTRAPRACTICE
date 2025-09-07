using System.Collections.ObjectModel;
using System.Linq;
using HarmonyLib;
using JetBrains.Annotations;
using ULTRAPRACTICE.Helpers;
using ULTRAPRACTICE.Interfaces;
using UnityEngine;

namespace ULTRAPRACTICE.ClassSavers;

public sealed class CannonBallVariables : VariableSaver<CannonBallVariables.CannonballProps, Cannonball>
{
    public sealed class CannonballProps : ITypeProperties<Cannonball, CannonballProps>
    {
        [UsedImplicitly]
        public GameObject gameObject;

        public Cannonball BackupObject { get; set; }
        public CannonballProps CopyFrom(Cannonball other)
        {
            gameObject = other.gameObject;
            BackupObject = Object.Instantiate(other, other.gameObject.transform.position, other.transform.rotation);
            UpdateBehaviour.CopyScripts(other.gameObject, BackupObject.gameObject);
            BackupObject.rb = BackupObject.GetComponent<Rigidbody>();
            BackupObject.rb.velocity = other.rb.velocity;
            BackupObject.gameObject.SetActive(false);
            return this;
        }

        public void Restore()
        {
            var backupCopy = Object.Instantiate(BackupObject, BackupObject.transform.position, BackupObject.transform.rotation);
            UpdateBehaviour.CopyScripts(BackupObject.gameObject, backupCopy.gameObject);
            gameObject = backupCopy.gameObject;
            backupCopy.rb = backupCopy.GetComponent<Rigidbody>();
            backupCopy.rb.velocity = BackupObject.rb.velocity;
            backupCopy.gameObject.SetActive(true);
        }
    }
    public override ReadOnlyCollection<CannonballProps> States { get; set; }

    public override void SaveVariables()
    {
        if (States != null)
            foreach (var state in States)
                Object.Destroy(state.BackupObject);
        States = Object.FindObjectsOfType<Cannonball>()
                       .Select(obj => new CannonballProps().CopyFrom(obj))
                       .ToArray()
                       .AsReadOnly();
    }

    public override void SetVariables()
    {
        Object.FindObjectsOfType<Cannonball>()
              .Where(cannonball => cannonball.gameObject.activeSelf)
              .Do(cannonball => Object.Destroy(cannonball.gameObject));

        foreach (var state in States)
            state.Restore();
    }
}