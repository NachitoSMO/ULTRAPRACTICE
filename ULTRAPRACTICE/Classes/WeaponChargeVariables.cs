using System.Collections.Generic;
using UnityEngine;

namespace ULTRAPRACTICE.Classes;

//can't make a backup weaponcharges class since its a MonoSingleton thus we make our own
public sealed class WeaponChargesSaved : MonoBehaviour
{
    public GunControl gc;

    public float rev0charge = 100f;

    public bool rev0alt;

    public float rev1charge = 400f;

    public float rev2charge = 300f;

    public bool rev2alt;

    public float shoAltNadeCharge = 1f;

    public float shoSawCharge = 1f;

    public int shoSawAmount;

    public bool nai0set;

    public float naiHeatsinks = 2f;

    public float naiSawHeatsinks = 1f;

    public float naiheatUp;

    public float naiAmmo = 100f;

    public float naiSaws = 10f;

    public bool naiAmmoDontCharge;

    public List<Magnet> magnets = new();

    public float naiMagnetCharge = 3f;

    public float naiZapperRecharge = 5f;

    public float raicharge = 5f;

    public GameObject railCannonFullChargeSound;

    public bool railChargePlayed;

    public bool rocketset;

    public float rocketcharge;

    public bool rocketFrozen;

    public float rocketFreezeTime = 5f;

    public RocketLauncher rocketLauncher;

    public int rocketCount;

    public bool canAutoUnfreeze;

    public TimeSince timeSinceIdleFrozen;

    public float rocketCannonballCharge = 1f;

    public float rocketNapalmFuel = 1f;
    public bool infiniteRocketRide;

    public float[] revaltpickupcharges = new float[3];

    public float[] shoaltcooldowns = new float[3];

    public float punchStamina = 2f;
}

public static class WeaponChargeVariables
{
    public static WeaponChargesSaved wcs;

    public static void SaveVariables()
    {
        if (wcs != null) GameObject.Destroy(wcs.gameObject);
        wcs = new GameObject().AddComponent<WeaponChargesSaved>();
        UpdateBehaviour.CopyValues(wcs, MonoSingleton<WeaponCharges>.Instance);
    }

    public static void SetVariables()
    {
        UpdateBehaviour.CopyValues(MonoSingleton<WeaponCharges>.Instance, wcs);
    }
}