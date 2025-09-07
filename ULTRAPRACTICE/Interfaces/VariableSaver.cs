using System.Collections.ObjectModel;
using System.Linq;
using ULTRAPRACTICE.Helpers;
using UnityEngine;

namespace ULTRAPRACTICE.Interfaces;

public abstract class VariableSaver<TProps, T> : IVariableSaver
    where TProps : ITypeProperties<T, TProps>, new()
    where T : Object
{
    public abstract ReadOnlyCollection<TProps> States { get; set; }

    public virtual void SaveVariables()
    {
        States =
            Object.FindObjectsOfType<T>()
                  .Select(obj => new TProps().CopyFrom(obj))
                  .ToArray()
                  .AsReadOnly();
    }

    public abstract void SetVariables();
}

public interface IVariableSaver
{
    void SaveVariables();
    void SetVariables();
}