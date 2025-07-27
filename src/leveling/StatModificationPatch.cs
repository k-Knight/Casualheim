using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Casualheim.leveling {
    [HarmonyPatch]
    public class StatModificationPatch {
        public static Dictionary<int, float> stamina_values = new Dictionary<int, float>();
        public static Dictionary<int, float> eitr_values = new Dictionary<int, float>();

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

            if (!Util.TryGetPlayerZDO(ref __instance, out p, out zdo))
                return;

            float strength = GetLevelEffectStrength(ref zdo, 1.5f) * ThisPlugin.HealthBoostMultiplier.Value;
            health *= strength;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), "Heal")]
        public static void CharacterHealPatch(Character __instance, ref float hp) {
            if (!ThisPlugin.PluginEnabled.Value || !ThisPlugin.EnableLeveling.Value || ThisPlugin.HealthRegenBoostMultiplier.Value == 0f)
                return;

            Player p;
            ZDO zdo;

            if (!Util.TryGetPlayerZDO(ref __instance, out p, out zdo))
                return;

            float strength = GetLevelEffectStrength(ref zdo, 1.5f) * ThisPlugin.HealthRegenBoostMultiplier.Value;
            hp *= strength;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Player), "SetMaxStamina")]
        public static void PlayerSetMaxStaminaPatch(Player __instance, ref float stamina) {
            if (!ThisPlugin.PluginEnabled.Value || !ThisPlugin.EnableLeveling.Value || ThisPlugin.StaminaBoostMultiplier.Value == 0f)
                return;

            ZDO zdo;

            if (!Util.TryGetPlayerZDO(ref __instance, out zdo))
                return;

            float strength = GetLevelEffectStrength(ref zdo, 1.25f) * ThisPlugin.StaminaBoostMultiplier.Value;
            stamina *= strength;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Player), "SetMaxEitr")]
        public static void PlayerSetMaxEitrPatch(Player __instance, ref float eitr) {
            if (!ThisPlugin.PluginEnabled.Value || !ThisPlugin.EnableLeveling.Value || ThisPlugin.EitrBoostMultiplier.Value == 0f)
                return;

            ZDO zdo;

            if (!Util.TryGetPlayerZDO(ref __instance, out zdo))
                return;

            float strength = GetLevelEffectStrength(ref zdo, 1.5f) * ThisPlugin.EitrBoostMultiplier.Value;
            eitr *= strength;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Player), "UpdateStats", new Type[] { typeof(float) })]
        public static void PlayerUpdateStatsPatch_Prefix(Player __instance) {
            if (!ThisPlugin.PluginEnabled.Value || !ThisPlugin.EnableLeveling.Value)
                return;

            stamina_values[__instance.GetHashCode()] = __instance.m_stamina;
            eitr_values[__instance.GetHashCode()] = __instance.m_eitr;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "UpdateStats", new Type[] { typeof(float) })]
        public static void PlayerUpdateStatsPatch_Postfix(Player __instance) {
            if (!ThisPlugin.PluginEnabled.Value || !ThisPlugin.EnableLeveling.Value)
                return;

            int p_hash = __instance.GetHashCode();
            ZDO zdo;

            if (!Util.TryGetPlayerZDO(ref __instance, out zdo))
                return;

            if (stamina_values.ContainsKey(p_hash) && ThisPlugin.StaminaRegenBoostMultiplier.Value > 0.001f) {
                float strength = GetLevelEffectStrength(ref zdo, 2f) * ThisPlugin.StaminaRegenBoostMultiplier.Value;
                float stam_diff = __instance.m_stamina - stamina_values[p_hash];

                if (stam_diff > 0f)
                    __instance.m_stamina = Mathf.Min(__instance.GetMaxStamina(), stam_diff * (strength - 1f) + __instance.m_stamina);
            }

            if (eitr_values.ContainsKey(p_hash) && ThisPlugin.EitrRegenBoostMultiplier.Value > 0.001f) {
                float strength = GetLevelEffectStrength(ref zdo, 2f) * ThisPlugin.EitrRegenBoostMultiplier.Value;
                float eitr_diff = __instance.m_eitr - eitr_values[p_hash];

                if (eitr_diff > 0f)
                    __instance.m_eitr = Mathf.Min(__instance.GetMaxEitr(), eitr_diff * (strength - 1f) + __instance.m_eitr);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "GetJogSpeedFactor")]
        public static void PlayerGetJogSpeedFactorPatch(Player __instance, ref float __result) {
            if (!ThisPlugin.PluginEnabled.Value || !ThisPlugin.EnableLeveling.Value || ThisPlugin.SpeedBoostMultiplier.Value == 0f)
                return;

            ZDO zdo;

            if (!Util.TryGetPlayerZDO(ref __instance, out zdo))
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

            if (!Util.TryGetPlayerZDO(ref __instance, out zdo))
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

            if (!Util.TryGetPlayerZDO(ref __instance, out p, out zdo))
                return;

            // 125% of normal at level 100
            float strength = GetLevelEffectStrength(ref zdo, 1.25f) * ThisPlugin.SpeedBoostMultiplier.Value;
            float skill_factor = p.m_skills.GetSkillFactor(Skills.SkillType.Sneak) * 0.5f + 1f;
            float equipment_factor = 1f + p.GetEquipmentMovementModifier();
            p.m_crouchSpeed = 2f * strength * skill_factor * equipment_factor;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), "UpdateGroundContact")]
        public static void CharacteUpdateGroundContactPatch_Prefix(Character __instance) {
            if (!ThisPlugin.PluginEnabled.Value)
                return;

            if (!__instance.m_groundContact)
                return;

            Player p;
            ZDO zdo;

            if (!Util.TryGetPlayerZDO(ref __instance, out p, out zdo))
                return;

            float diff = p.m_maxAirAltitude - p.transform.position.y;
            if (diff <= 0f)
                return;

            // reduce percentage damage
            float skill_factor = p.m_skills.GetSkillFactor(Skills.SkillType.Jump) * 0.3f;
            p.m_maxAirAltitude -= diff * skill_factor;

            if (ThisPlugin.FallWindowMultiplier.Value == 0f)
                return;

            // extend fall damage window
            float level_reduction = (GetLevelEffectStrength(ref zdo, 3f) - 1f) * ThisPlugin.FallWindowMultiplier.Value;
            p.m_maxAirAltitude = Math.Max(p.m_maxAirAltitude - level_reduction, p.transform.position.y);
        }

        // increse jump boost
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Skills), "GetSkillFactor")]
        public static void SkillsGetSkillFactorPatch(ref Skills __instance, ref float __result, ref Skills.SkillType skillType) {
            if (!ThisPlugin.PluginEnabled.Value || ThisPlugin.JumpHeightMultiplier.Value == 0)
                return;

            if (__instance.m_player == null || skillType != Skills.SkillType.Jump)
                return;

            ZDO zdo;

            if (!Util.TryGetPlayerZDO(ref __instance.m_player, out zdo))
                return;

            if (!Util.check_caller(typeof(Character), "Jump"))
                return;

            // 140% of normal at level 100
            float strength = GetLevelEffectStrength(ref zdo, 1.4f) * ThisPlugin.JumpHeightMultiplier.Value;

            if (ThisPlugin.DebugOutput.Value)
                Debug.Log("Casualheim.StatModificationPatch | icreasing jump force " + strength + " times");

            __result *= strength;
        }
    }
}
