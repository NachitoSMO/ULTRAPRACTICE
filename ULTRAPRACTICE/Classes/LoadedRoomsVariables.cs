using System.Collections.Generic;
using UnityEngine;
namespace ULTRAPRACTICE.Classes;

public static class LoadedRoomsVariables
{
    public static List<GameObject> activeObjs = new();
    public static List<GameObject> inactiveObjs = new();
    public static bool doorOpened;

    public static void SaveVariables()
    {
        activeObjs.Clear();
        inactiveObjs.Clear();

        Door[] allObjs = Object.FindObjectsOfType<Door>(true);
            
        foreach (Door door in allObjs)
        {
            foreach (GameObject room in door.activatedRooms)
            {
                if (room == null) continue;
                if (room.activeInHierarchy) activeObjs.Add(room);
                else inactiveObjs.Add(room);
            }
        }

        if (MonoSingleton<OutdoorLightMaster>.Instance != null)
            doorOpened = MonoSingleton<OutdoorLightMaster>.Instance.firstDoorOpened;

    }

    public static void SetVariables()
    {
        foreach (GameObject obj in activeObjs)
        {
            if (obj != null) obj.SetActive(true);
        }

        /// check if the first door was opened or else every room will despawn
        if (doorOpened)
        {
            foreach (GameObject obj in inactiveObjs)
            {
                if (obj != null) obj.SetActive(false);
            }
        }
    }
}