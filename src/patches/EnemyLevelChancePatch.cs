using HarmonyLib;
using System;

namespace Casualheim.patches {
    [HarmonyPatch]
    public class EnemyLevelChancePatch {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SpawnArea), "GetLevelUpChance")]
        public static void SpawnAreaLevelUpChancePatch(ref SpawnArea __instance, ref float __result) {
            if (!ThisPlugin.PluginEnabled.Value || __instance == null)
                return;

            __result *= ThisPlugin.EnemyLevelChanceMultiplier.Value;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SpawnSystem), "GetLevelUpChance", new Type[] { typeof(float) })]
        public static void SpawnSystemLevelUpChancePatch(ref float __result) {
            if (!ThisPlugin.PluginEnabled.Value)
                return;

            __result *= ThisPlugin.EnemyLevelChanceMultiplier.Value;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(TriggerSpawner), "Awake")]
        public static void TriggerSpawnerLevelUpChancePatch(ref TriggerSpawner __instance) {
            if (!ThisPlugin.PluginEnabled.Value || __instance == null)
                return;

            __instance.m_levelupChance *= ThisPlugin.EnemyLevelChanceMultiplier.Value;
        }
    }
}
