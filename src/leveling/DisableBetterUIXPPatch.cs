using BetterUI.Patches;
using HarmonyLib;

namespace Casualheim.leveling {
    [HarmonyPatch]
    public class DisableBetterUIXPPatch {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(XP), "Awake")]
        public static bool DisableXPAwake() {
            if (!ThisPlugin.EnableLeveling.Value)
                return true;

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(XP), "RaiseXP")]
        public static bool DisableXPRaiseXP() {
            if (!ThisPlugin.EnableLeveling.Value)
                return true;

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(XP), "UpdateLevelProgressPercentage")]
        public static bool DisableXPUpdateLevelProgressPercentage() {
            if (!ThisPlugin.EnableLeveling.Value)
                return true;

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(XP), "GetNextLevelRequirement")]
        public static bool DisableXPGetNextLevelRequirement(ref float __result) {
            if (!ThisPlugin.EnableLeveling.Value)
                return true;

            __result = 1f;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(XPBar), "Create")]
        public static bool DisableXPBarCreate() {
            if (!ThisPlugin.EnableLeveling.Value)
                return true;

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(XPBar), "UpdateLevelProgressPercentage")]
        public static bool DisableXPBarUpdateLevelProgressPercentage() {
            if (!ThisPlugin.EnableLeveling.Value)
                return true;

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(XPBar), "UpdatePosition")]
        public static bool DisableXPBarUpdatePosition() {
            if (!ThisPlugin.EnableLeveling.Value)
                return true;

            return false;
        }
    }
}
