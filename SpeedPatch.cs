using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace Casualheim {
    [HarmonyPatch(typeof(Humanoid), "GetAttackSpeedFactorMovement")]
    public class AttackMovementSpeedPatch {
        public static void Postfix(Humanoid __instance, ref float __result) {
            if (__result < 0.99f && __instance.GetType() == typeof(Player) && ThisPlugin.PluginEnabled.Value) {
                //if (ThisPlugin.DebugOutput.Value)
                //    UnityEngine.Debug.Log("overriding result of GetAttackSpeedFactorMovement(), was :: " + __result.ToString());
                __result = ThisPlugin.PercentAttackMovement.Value / 100.0f;
            }
        }
    }

    [HarmonyPatch(typeof(Humanoid), "GetAttackSpeedFactorRotation")]
    public class AttackRotationSpeedPatch {
        public static void Postfix(Humanoid __instance, ref float __result) {
            if (__result < 0.99f && __instance.GetType() == typeof(Player) && ThisPlugin.PluginEnabled.Value) {
                //if (ThisPlugin.DebugOutput.Value)
                //    UnityEngine.Debug.Log("overriding result of GetAttackSpeedFactorRotation(), was :: " + __result.ToString());
                __result = ThisPlugin.PercentAttackRotation.Value / 100.0f;
            }
        }
    }
}
