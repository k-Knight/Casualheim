using BepInEx.Configuration;
using HarmonyLib;
using System;
using UnityEngine;

namespace Casualheim.patches {
    [HarmonyPatch(typeof(Character), "Heal")]
    public class RegenPatch {
        public static void Prefix(Character __instance, ref float hp) {
            if (!ThisPlugin.PluginEnabled.Value)
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

            if (!ThisPlugin.HealthRegenPercentDict.TryGetValue(text, out setting_ref))
                return;

            if (!setting_ref.TryGetTarget(out setting))
                return;

            hp = hp * ((float)setting.Value / 100.0f);
            if (ThisPlugin.DebugOutput.Value)
                Debug.Log("Casualheim.RegenPatch :: " + text + " :: " + __instance.GetHealth() + " / " + __instance.GetMaxHealth() + "(+" + hp + ")");
        }
    }
}
