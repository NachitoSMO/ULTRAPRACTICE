using System.Collections.Generic;
using UnityEngine;

namespace ULTRAPRACTICE.Classes;

public static class LoadedRoomsVariables
{
    public static List<GameObject> activeObjs = new();
    public static List<GameObject> inactiveObjs = new();
    public static GameObject savedWeapon;
    public static bool doorOpened;

    public static void SaveVariables()
    {
        activeObjs.Clear();
        inactiveObjs.Clear();

        Door[] allObjs = GameObject.FindObjectsOfType<Door>(true);
            
        foreach (Door door in allObjs)
        {
            foreach (GameObject room in door.activatedRooms)
            {
                if (room.activeInHierarchy) activeObjs.Add(room);
                else inactiveObjs.Add(room);
            }
        }

        /// this doesnt work for the 4-2 skulls, for some reason
        /*var scene = SceneManager.GetActiveScene();
        var sceneRoots = scene.GetRootGameObjects();

        foreach (var o in sceneRoots)
        {
            if (o.name.Equals("6A - Indoor Garden") || o.name.Equals("6B - Outdoor Arena"))
            {
                if (o.activeSelf) activeObjs.Add(o);
                else inactiveObjs.Add(o);
            }
        }*/

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