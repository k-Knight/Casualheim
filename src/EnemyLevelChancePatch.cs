using HarmonyLib;
using System;

namespace Casualheim {
    [HarmonyPatch(typeof(SpawnArea), "GetLevelUpChance")]
    public class SpawnAreaLevelUpChancePatch {
        public static void Postfix(ref SpawnArea __instance, ref float __result) {
            if (!ThisPlugin.PluginEnabled.Value || __instance == null)
                return;

            __result *= ThisPlugin.EnemyLevelChanceMultiplier.Value;
        }
    }

    [HarmonyPatch(typeof(SpawnSystem), "GetLevelUpChance", new Type[] { typeof(float) })]
    public class SpawnSystemLevelUpChancePatch {
        public static void Postfix(ref float __result) {
            if (!ThisPlugin.PluginEnabled.Value)
                return;

            __result *= ThisPlugin.EnemyLevelChanceMultiplier.Value;
        }
    }

    [HarmonyPatch(typeof(TriggerSpawner), "Awake")]
    public class TriggerSpawnerLevelUpChancePatch {
        public static void Postfix(ref TriggerSpawner __instance) {
            if (!ThisPlugin.PluginEnabled.Value || __instance == null)
                return;

            __instance.m_levelupChance *= ThisPlugin.EnemyLevelChanceMultiplier.Value;
        }
    }
}
