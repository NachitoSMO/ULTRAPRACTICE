using UnityEngine;

namespace ULTRAPRACTICE.Classes;

//can't make a backup weaponcharges class since its a MonoSingleton thus we make our own

public static class WeaponChargeVariables
{
    public static WeaponChargesSaved wcs;

    public static void SaveVariables()
    {
        if (wcs != null) Object.Destroy(wcs.gameObject);
        wcs = new GameObject().AddComponent<WeaponChargesSaved>();
        UpdateBehaviour.CopyValues(wcs, MonoSingleton<WeaponCharges>.Instance);
    }

    public static void SetVariables()
    {
        UpdateBehaviour.CopyValues(MonoSingleton<WeaponCharges>.Instance, wcs);
    }
}