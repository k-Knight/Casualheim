
using HarmonyLib;

namespace Casualheim.leveling {
    [HarmonyPatch]
    public class LevelPatch {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Skills), "LowerAllSkills")]
        public static void SkillstLowerAllSkillsPatch() {
            if (ThisPlugin.EnableLeveling.Value)
                Level.Update();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Skills), "CheatRaiseSkill")]
        public static void SkillsCheatRaiseSkillPatch() {
            if (ThisPlugin.EnableLeveling.Value)
                Level.Update();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Skills.Skill), "Raise")]
        public static void SkillRaisePatch() {
            if (ThisPlugin.EnableLeveling.Value)
                Level.Update();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Hud), "Update")]
        public static void HudUpdatePatch() {
            if (!ThisPlugin.EnableLeveling.Value)
                return;

            Player p = Player.m_localPlayer;

            if (p != null && (Level.player == null || Level.player.GetHashCode() != p.GetHashCode())) {
                Level.Initialize(ref p);
            }
        }
    }
}
