using HarmonyLib;
using UnityEngine;
using static Skills;

namespace Casualheim.patches {
    [HarmonyPatch(typeof(Skill), "GetNextLevelRequirement")]
    public class SkillCurvePatch {
        public static bool Prefix(Skill __instance, ref float __result) {
            if (!ThisPlugin.PluginEnabled.Value || !ThisPlugin.EasierSkillCurveEnabled.Value || __instance == null)
                return true;

            __result = Mathf.Floor(__instance.m_level + 1f) * ThisPlugin.RequiredExpMultiplier.Value;
            return false;
        }
    }
}
