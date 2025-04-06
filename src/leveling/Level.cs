using Casualheim.gui;
using System;
using System.Reflection.Emit;
using UnityEngine;

namespace Casualheim.leveling {
    public static class Level {
        public static Player player;
        public static float total_exp;
        public static int level = -1;
        public static float next_level_progress;
        public static float curr_level_required_xp = 0f;
        public static float next_level_required_xp = 1f;

        public static void Initialize(ref Player player) {
            Level.player = player;
            level = -1;

            Update();
        }

        public static void Update() {
            if (Level.player == null)
                return;

            int current_level = Level.level;

            Level.UpdateTotalExp();
            Level.UpdateLevel();
            Level.UpdateLevelProgress();
            LevelIndicatorPatch.Update(Level.level, Level.next_level_progress);

            if (current_level != -1 && current_level < Level.level) {
                Level.player.m_skillLevelupEffects.Create(Level.player.m_head.position, Level.player.m_head.rotation, Level.player.m_head, 1.5f);
                Level.player.Message(
                    MessageHud.MessageType.Center,
                    $"<size=60><color=#ffffffff>New Level</color></size>\n<size=30>Reached level {Level.level} </size>"
                );
            }
        }

        public static void UpdateTotalExp() {
            if (Level.player == null)
                return;

            float progress = 0;
            int num_of_skills = 0;

            foreach (Skills.Skill skill in Level.player.m_skills.m_skillData.Values) {
                num_of_skills++;
                progress += skill.m_level;
            }

            Level.total_exp = (progress / (num_of_skills * 100f)) * 5050f;
        }

        public static void UpdateLevel() {
            if (Level.player == null) {
                if (ThisPlugin.DebugOutput.Value)
                    Debug.Log("Casualheim.Level | local player is null !!!");

                return;
            }

            double estimated_lvl = (-1.0 + Math.Sqrt(8.0 * (double)Level.total_exp + 1.0)) / 2.0;
            Level.level = (int)Math.Floor(Math.Round(estimated_lvl, 1));

            curr_level_required_xp = Level.GetNextLevelRequirement(Level.level - 1);
            next_level_required_xp = Level.GetNextLevelRequirement(Level.level);

            if (!ThisPlugin.PluginEnabled.Value)
                return;

            if (!Level.player.m_nview.IsOwner()) {
                if (ThisPlugin.DebugOutput.Value)
                    Debug.Log("Casualheim.Level | we are not the owner of the local player !!!");

                return;
            }

            ZDO zdo = Level.player.m_nview.GetZDO();
            if (zdo == null) {
                if (ThisPlugin.DebugOutput.Value)
                    Debug.Log("Casualheim.Level | zdo of local player is null !!!");

                return;
            }

            if (ThisPlugin.DebugOutput.Value)
                Debug.Log("Casualheim.Level | current level for local player is :: " + Level.level);

            zdo.Set("betterui_level", Level.level);
        }

        public static void UpdateLevelProgress() {
            Level.next_level_progress = (Level.total_exp - Level.curr_level_required_xp) / (Level.next_level_required_xp - Level.curr_level_required_xp);
        }

        public static float GetNextLevelRequirement(int lvl) {
            return (lvl + 1f) * (2f + lvl) / 2f;
        }
    }
}
