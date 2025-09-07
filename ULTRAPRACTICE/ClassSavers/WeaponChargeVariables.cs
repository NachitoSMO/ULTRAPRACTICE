using ULTRAPRACTICE.Interfaces;
using UnityEngine;

namespace ULTRAPRACTICE.ClassSavers;

//can't make a backup weaponcharges class since its a MonoSingleton thus we make our own

public sealed partial class WeaponChargeVariables : IVariableSaver
{
    public static WeaponChargesSaved wcs;

    public void SaveVariables()
    {
        if (wcs != null) Object.Destroy(wcs.gameObject);
        wcs = new GameObject().AddComponent<WeaponChargesSaved>();
        UpdateBehaviour.CopyValues(wcs, MonoSingleton<WeaponCharges>.Instance);
    }

    public void SetVariables()
    {
        UpdateBehaviour.CopyValues(MonoSingleton<WeaponCharges>.Instance, wcs);
    }
}