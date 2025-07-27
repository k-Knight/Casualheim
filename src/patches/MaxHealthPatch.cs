using BepInEx.Configuration;
using HarmonyLib;
using System;
using UnityEngine;

namespace Casualheim.patches {
    [HarmonyPatch(typeof(Character), "SetMaxHealth")]
    public class MaxHealthPatch {
        public static void Prefix(Character __instance, ref float health) {
            if (!ThisPlugin.PluginEnabled.Value)
                return;

            if (ZNet.instance == null || !ZNet.instance.IsServer())
                return;

            if (__instance.IsBoss() && (Player.GetAllPlayers().Count > ThisPlugin.NumberOfPlayersMax.Value || !ThisPlugin.EnableBossHealthRegenMod.Value))
                return;

            if (!__instance.IsBoss() && !ThisPlugin.EnableEnemyHealthMod.Value)
                return;

            const string enemy_tag = "$enemy_";
            string text = __instance.m_name.ToLower();

            if (text.StartsWith(enemy_tag))
                text = text.Substring(enemy_tag.Length);

            WeakReference<ConfigEntry<int>> setting_ref = null;
            ConfigEntry<int> setting = null;


            if (text == "human")
                return;

            if (!ThisPlugin.MaxHealthPercentDict.TryGetValue(text, out setting_ref)) {
                if (MaxHealthSetting.Settings.ContainsKey(text))
                    setting = MaxHealthSetting.Settings[text].percent;
                else
                    return;
            }

            if (setting == null && setting_ref == null)
                return;

            if (setting == null && !setting_ref.TryGetTarget(out setting))
                return;

            health = health * ((float)setting.Value / 100.0f);
        }
    }
}
