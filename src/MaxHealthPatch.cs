using BepInEx.Configuration;
using HarmonyLib;
using System;
using UnityEngine;

namespace Casualheim {
    [HarmonyPatch(typeof(Character), "SetMaxHealth")]
    public class MaxHealthPatch {
        public static void Prefix(Character __instance, ref float health) {
            if (!ThisPlugin.PluginEnabled.Value || __instance == null)
                return;

            if (ZNet.instance == null || !ZNet.instance.IsServer())
                return;

            if (__instance.IsBoss() && Player.GetAllPlayers().Count > ThisPlugin.NumberOfPlayersMax.Value)
                return;

            const string enemy_tag = "$enemy_";
            string text = __instance.m_name.ToLower();

            if (text.StartsWith(enemy_tag))
                text = text.Substring(enemy_tag.Length);

            WeakReference<ConfigEntry<int>> setting_ref;
            ConfigEntry<int> setting;

            if (!ThisPlugin.MaxHealthPercentDict.TryGetValue(text, out setting_ref)) {
                if (ThisPlugin.DebugOutput.Value && text != "human")
                    Debug.Log("Casualheim | !!! unmatched enemy normal max health " + text + " :: " + health.ToString());

                return;
            }

            if (!setting_ref.TryGetTarget(out setting))
                return;

            health = health * ((float)setting.Value / 100.0f);
            if (ThisPlugin.DebugOutput.Value)
                Debug.Log("Casualheim.MaxHealthPatch :: " + text + " :: " + health.ToString());
        }
    }
}
