using HarmonyLib;

namespace Casualheim.patches {
    [HarmonyPatch]
    public class MiningChoppingPatch {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HitData.DamageTypes), "GetTotalDamage")]
        public static bool DamageTypesGetTotalDamagePatch(ref HitData.DamageTypes __instance, ref float __result) {
            float mult = ThisPlugin.ChopMineDamageMultiplier.Value;
            if (mult < 0.001)
                return true;

            __result = __instance.m_damage +
                __instance.m_blunt +
                __instance.m_slash +
                __instance.m_pierce +
                (__instance.m_chop * mult) +
                (__instance.m_pickaxe * mult) +
                __instance.m_fire +
                __instance.m_frost +
                __instance.m_lightning +
                __instance.m_poison +
                __instance.m_spirit;

            return false;
        }
    }
}
