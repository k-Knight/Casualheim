using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Casualheim.leveling {
    [HarmonyPatch]
    public class StatModificationPatch {
        public static Dictionary<int, float> stamina_values = new Dictionary<int, float>();

        public static bool TryGetPlayerZDO(ref Character character, out Player player, out ZDO zdo) {
            player = null;
            zdo = null;

            if (character.GetType() != typeof(Player))
                return false;

            player = character as Player;
            return TryGetPlayerZDO(ref player, out zdo);
        }

        public static bool TryGetPlayerZDO(ref Player player, out ZDO zdo) {
            zdo = player.m_nview.GetZDO();
            if (zdo == null) {
                if (ThisPlugin.DebugOutput.Value)
                    Debug.Log("Casualheim.StatModificationPatch | zdo of a player is null !!!");

                return false;
            }

            return true;
        }

        public static float GetLevelEffectStrength(ref ZDO zdo, float strength_at_100) {
            return ((float)zdo.GetInt("betterui_level")) / 100f / (1f / (strength_at_100 - 1f)) + 1f;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), "SetMaxHealth")]
        public static void CharacterSetMaxHealthPatch(ref Character __instance, ref float health) {
            if (!ThisPlugin.PluginEnabled.Value || !ThisPlugin.EnableLeveling.Value || ThisPlugin.HealthBoostMultiplier.Value == 0f)
                return;

            Player p;
            ZDO zdo;

            if (!TryGetPlayerZDO(ref __instance, out p, out zdo))
                return;

            float strength = GetLevelEffectStrength(ref zdo, 1.5f) * ThisPlugin.HealthBoostMultiplier.Value;

            if (ThisPlugin.DebugOutput.Value)
                Debug.Log("Casualheim.StatModificationPatch | mutiplying max health by [" + strength + "] for player " + p.m_name);

            health *= strength;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), "Heal")]
        public static void CharacterHealPatch(Character __instance, ref float hp) {
            if (!ThisPlugin.PluginEnabled.Value || !ThisPlugin.EnableLeveling.Value || ThisPlugin.HealthRegenBoostMultiplier.Value == 0f)
                return;

            Player p;
            ZDO zdo;

            if (!TryGetPlayerZDO(ref __instance, out p, out zdo))
                return;

            float strength = GetLevelEffectStrength(ref zdo, 1.5f) * ThisPlugin.HealthRegenBoostMultiplier.Value;

            if (ThisPlugin.DebugOutput.Value)
                Debug.Log("Casualheim.StatModificationPatch | mutiplying heal by [" + strength + "] for player " + p.m_name);

            hp *= strength;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Player), "SetMaxStamina")]
        public static void PlayerSetMaxStaminaPatch(Player __instance, ref float stamina) {
            if (!ThisPlugin.PluginEnabled.Value || !ThisPlugin.EnableLeveling.Value || ThisPlugin.StaminaBoostMultiplier.Value == 0f)
                return;

            ZDO zdo;

            if (!TryGetPlayerZDO(ref __instance, out zdo))
                return;

            float strength = GetLevelEffectStrength(ref zdo, 1.25f) * ThisPlugin.StaminaBoostMultiplier.Value;

            if (ThisPlugin.DebugOutput.Value)
                Debug.Log("Casualheim.StatModificationPatch | mutiplying max stamina by [" + strength + "] for player " + __instance.m_name);

            stamina *= strength;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Player), "UpdateStats", new Type[] { typeof(float) })]
        public static void PlayerUpdateStatsPatch_Prefix(Player __instance) {
            if (!ThisPlugin.PluginEnabled.Value || !ThisPlugin.EnableLeveling.Value || ThisPlugin.StaminaRegenBoostMultiplier.Value == 0f)
                return;

            stamina_values[__instance.GetHashCode()] = __instance.m_stamina;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "UpdateStats", new Type[] { typeof(float) })]
        public static void PlayerUpdateStatsPatch_Postfix(Player __instance) {
            if (!ThisPlugin.PluginEnabled.Value || !ThisPlugin.EnableLeveling.Value || ThisPlugin.StaminaRegenBoostMultiplier.Value == 0f)
                return;

            int p_hash = __instance.GetHashCode();

            if (!stamina_values.ContainsKey(p_hash)) {
                if (ThisPlugin.DebugOutput.Value)
                    Debug.Log("Casualheim.StatModificationPatch | stamina_values has no record for player " + __instance.m_name + " !!!");

                return;
            }

            ZDO zdo;

            if (!TryGetPlayerZDO(ref __instance, out zdo))
                return;

            float strength = GetLevelEffectStrength(ref zdo, 2f) * ThisPlugin.StaminaRegenBoostMultiplier.Value;
            float stam_diff = __instance.m_stamina - stamina_values[p_hash];

            if (stam_diff <= 0f)
                return;

            __instance.m_stamina = Mathf.Min(__instance.GetMaxStamina(), stam_diff * (strength - 1f) + __instance.m_stamina);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "GetJogSpeedFactor")]
        public static void PlayerGetJogSpeedFactorPatch(Player __instance, ref float __result) {
            if (!ThisPlugin.PluginEnabled.Value || !ThisPlugin.EnableLeveling.Value || ThisPlugin.SpeedBoostMultiplier.Value == 0f)
                return;

            ZDO zdo;

            if (!TryGetPlayerZDO(ref __instance, out zdo))
                return;

            float strength = GetLevelEffectStrength(ref zdo, 1.25f) * ThisPlugin.SpeedBoostMultiplier.Value;
            __result *= strength;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "GetRunSpeedFactor")]
        public static void PlayerGetRunSpeedFactorPatch(Player __instance, ref float __result) {
            if (!ThisPlugin.PluginEnabled.Value || !ThisPlugin.EnableLeveling.Value || ThisPlugin.SpeedBoostMultiplier.Value == 0f)
                return;

            ZDO zdo;

            if (!TryGetPlayerZDO(ref __instance, out zdo))
                return;

            // 125% of normal at level 100
            float strength = GetLevelEffectStrength(ref zdo, 1.25f) * ThisPlugin.SpeedBoostMultiplier.Value;
            __result *= strength;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), "UpdateWalking")]
        public static void CharacterUpdateWalkingPatch(Character __instance) {
            if (!ThisPlugin.PluginEnabled.Value || !ThisPlugin.EnableLeveling.Value || ThisPlugin.SpeedBoostMultiplier.Value == 0f)
                return;

            Player p;
            ZDO zdo;

            if (!TryGetPlayerZDO(ref __instance, out p, out zdo))
                return;

            // 125% of normal at level 100
            float strength = GetLevelEffectStrength(ref zdo, 1.25f) * ThisPlugin.SpeedBoostMultiplier.Value;
            p.m_crouchSpeed = 2f * strength;
        }
    }
}
