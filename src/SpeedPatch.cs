using HarmonyLib;

namespace Casualheim {
    [HarmonyPatch(typeof(Humanoid), "GetAttackSpeedFactorMovement")]
    public class AttackMovementSpeedPatch {
        public static void Postfix(Humanoid __instance, ref float __result) {
            if (!ThisPlugin.PluginEnabled.Value || __instance == null)
                return;

            if (__result < 0.99f && __instance.GetType() == typeof(Player) && ThisPlugin.PluginEnabled.Value) {
                __result = ThisPlugin.PercentAttackMovement.Value / 100.0f;
            }
        }
    }

    [HarmonyPatch(typeof(Humanoid), "GetAttackSpeedFactorRotation")]
    public class AttackRotationSpeedPatch {
        public static void Postfix(Humanoid __instance, ref float __result) {
            if (!ThisPlugin.PluginEnabled.Value || __instance == null)
                return;

            if (__result < 0.99f && __instance.GetType() == typeof(Player) && ThisPlugin.PluginEnabled.Value) {
                __result = ThisPlugin.PercentAttackRotation.Value / 100.0f;
            }
        }
    }
}
