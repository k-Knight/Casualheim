using HarmonyLib;

namespace Casualheim.patches {
    [HarmonyPatch]
    public class AttackSlowdownPatch {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Humanoid), "GetAttackSpeedFactorMovement")]
        public static void AttackMovementSpeedPatch(Humanoid __instance, ref float __result) {
            if (!ThisPlugin.PluginEnabled.Value)
                return;

            if (__result < 0.99f && __instance.GetType() == typeof(Player) && ThisPlugin.PluginEnabled.Value) {
                __result = ThisPlugin.PercentAttackMovement.Value / 100.0f;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Humanoid), "GetAttackSpeedFactorRotation")]
        public static void AttackRotationSpeedPatch(Humanoid __instance, ref float __result) {
            if (!ThisPlugin.PluginEnabled.Value)
                return;

            if (__result < 0.99f && __instance.GetType() == typeof(Player) && ThisPlugin.PluginEnabled.Value) {
                __result = ThisPlugin.PercentAttackRotation.Value / 100.0f;
            }
        }
    }
}
