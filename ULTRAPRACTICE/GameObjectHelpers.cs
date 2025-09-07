using UnityEngine;

namespace ULTRAPRACTICE;

public static class GameObjectHelpers
{
    public static GameObject ReplaceWith(this GameObject obj, GameObject newObj)
    {
        obj.SetActive(false);
        Object.DestroyImmediate(obj);
        obj = newObj;
        obj.SetActive(true);
        return obj;
    }
}