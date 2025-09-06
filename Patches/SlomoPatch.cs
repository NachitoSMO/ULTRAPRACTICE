using HarmonyLib;
using UnityEngine;
namespace ULTRAPRACTICE.Patches
{
    // i am unsure if this is necessary honestly, but the SloMo class usually kills itself after existing once i think so this should prevent that
    public class SlowMoAttachment : MonoBehaviour
    {
        public bool hasBeenDone = false;
    }

    [HarmonyPatch(typeof(SlowMo))]
    class SlomoPatch
    {
        [HarmonyPatch(nameof(SlowMo.OnEnable))]
        [HarmonyPrefix]
        private static bool OnEnablePrefix(SlowMo __instance)
        {
            if (__instance.GetComponent<SlowMoAttachment>() != null)
            {
                if (!__instance.GetComponent<SlowMoAttachment>().hasBeenDone)
                    MonoSingleton<TimeController>.Instance.SlowDown(__instance.amount);
            }
            else
            {
                __instance.GetOrAddComponent<SlowMoAttachment>().hasBeenDone = true;
                MonoSingleton<TimeController>.Instance.SlowDown(__instance.amount);
            }
            return false;
        }
    }
}
