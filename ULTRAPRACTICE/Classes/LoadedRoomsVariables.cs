using System.Collections.Generic;
using System.Linq;
using ULTRAPRACTICE.Interfaces;
using UnityEngine;
namespace ULTRAPRACTICE.Classes;

public sealed class LoadedRoomsVariables : IVariableSaver
{
    public static List<GameObject> ActiveObjs { get; } = [];
    public static List<GameObject> InactiveObjs { get; } = [];
    public static GameObject savedWeapon;
    public static bool doorOpened;

    public void SaveVariables()
    {
        ActiveObjs.Clear();
        InactiveObjs.Clear();

        var allObjs = Object.FindObjectsOfType<Door>(true);

        foreach (var room in allObjs
                    .SelectMany(door => door.activatedRooms
                                            .Where(room => room)))
            (room.activeInHierarchy
                 ? ActiveObjs
                 : InactiveObjs).Add(room);

        if (MonoSingleton<OutdoorLightMaster>.Instance)
            doorOpened = MonoSingleton<OutdoorLightMaster>.Instance.firstDoorOpened;
    }

    public void SetVariables()
    {
        foreach (var obj in ActiveObjs
                    .Where(obj => obj))
            obj.SetActive(true);

        // check if the first door was opened or else every room will despawn
        if (!doorOpened) return;
        foreach (var obj in InactiveObjs
                            .Where(obj => obj))
            obj.SetActive(false);
    }
}