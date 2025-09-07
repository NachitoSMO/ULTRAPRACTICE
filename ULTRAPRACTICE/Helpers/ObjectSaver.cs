using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;

namespace ULTRAPRACTICE;

public class ObjectSaver<T>(T @object)
    where T : class
{
    public readonly WeakReference<T> reference = new(@object);
    public Dictionary<string, object> savedValues = new();

    public void Save([CanBeNull] Func<FieldInfo, bool> predicate = null)
    {
        if (!reference.TryGetTarget(out var obj)) return;
        var fields = AccessTools.GetDeclaredFields(typeof(T))
                                .Where(f => predicate == null || predicate(f));
        foreach (var field in fields)
        {
            savedValues[field.Name] = field.GetValue(obj);
        }
    }

    public void Load()
    {
        if (!reference.TryGetTarget(out var obj)) return;
        foreach (var (fieldName, value) in savedValues)
        {
            var field = AccessTools.Field(typeof(T), fieldName);
            if (field == null) continue;
            field.SetValue(obj, value);
        }
    }
}