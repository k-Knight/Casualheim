using Casualheim.leveling;
using HarmonyLib;
using UnityEngine;

namespace Casualheim.patches {
    [HarmonyPatch]
    public class ShipHelpPatch {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Ship), "GetSailForce")]
        public static void ShipGetSailForcePostfixPatch(Ship __instance, ref Vector3 __result) {
            if (!ThisPlugin.EnableShipHelp.Value)
                return;

            __result *= ThisPlugin.SailForceMult.Value;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Ship), "Start")]
        public static void ShipStartPatch(Ship __instance) {
            if (!ThisPlugin.EnableShipHelp.Value)
                return;

            __instance.m_sailForceOffset *= (1.0f / ThisPlugin.ShipStabilization.Value);
            __instance.m_backwardForce *= ThisPlugin.RudderForceMult.Value;

            if (ThisPlugin.DebugOutput.Value) {
                Debug.Log("Casualheim.ShipStartPatch :: tm_sailForceOffset = " + __instance.m_sailForceOffset);
                Debug.Log("Casualheim.ShipStartPatch :: m_backwardForce = " + __instance.m_backwardForce);
            }
        }
    }
}
