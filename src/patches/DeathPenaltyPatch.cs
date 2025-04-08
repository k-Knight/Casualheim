using HarmonyLib;
using System.Collections.Generic;

namespace Casualheim.patches {
    [HarmonyPatch(typeof(Skills), "LowerAllSkills")]
    public class DeathPenaltyPatch {
        public static bool Prefix(ref Skills __instance, ref float factor) {
            if (!ThisPlugin.PluginEnabled.Value)
                return true;

            factor *= ThisPlugin.DeathPenaltyMultiplier.Value;

            foreach (KeyValuePair<Skills.SkillType, Skills.Skill> keyValuePair in __instance.m_skillData) {
                if (factor >= 0.01) {
                    float num = keyValuePair.Value.m_level * factor;
                    keyValuePair.Value.m_level -= num;
                }

                if (ThisPlugin.EnableSkillLevelProgressLoss.Value)
                    keyValuePair.Value.m_accumulator = 0f;
            }

            __instance.m_player.Message(MessageHud.MessageType.TopLeft, "$msg_skills_lowered", 0, null);

            return false;
        }
    }
}
