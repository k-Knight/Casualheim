using HarmonyLib;
using UnityEngine;

namespace Casualheim
{
    [HarmonyPatch(typeof(Character), "SetMaxHealth")]
    public class MaxHealthPatch {
        public static void Prefix(Character __instance, ref float health) {
            if (ThisPlugin.PluginEnabled.Value) {
                if (__instance != null) {
                    string text = __instance.m_name.ToLower();
                    float? new_health = null;

                    if (text == "$enemy_charred_melee")
                        new_health = health * ((float)ThisPlugin.PercentCharredMelee.Value / 100.0f);
                    else if (text == "$enemy_charred_twitcher")
                        new_health = health * ((float)ThisPlugin.PercentCharredTwitcher.Value / 100.0f);
                    else if (text == "$enemy_charred_archer")
                        new_health = health * ((float)ThisPlugin.PercentCharredArcher.Value / 100.0f);
                    else if (text == "$enemy_charred_mage")
                        new_health = health * ((float)ThisPlugin.PercentCharredMage.Value / 100.0f);
                    else if (text == "$enemy_charred_twitcher_summoned")
                        new_health = health * ((float)ThisPlugin.PercentCharredTwitcherSummoned.Value / 100.0f);
                    else if (text == "$enemy_morgen")
                        new_health = health * ((float)ThisPlugin.PercentMorgen.Value / 100.0f);
                    else if (text == "$enemy_fallenvalkyrie")
                        new_health = health * ((float)ThisPlugin.PercentFallenValkyrie.Value / 100.0f);
                    else if (text == "$enemy_bonemawserpent")
                        new_health = health * ((float)ThisPlugin.PercentBonemawSerpent.Value / 100.0f);
                    else if (text == "$enemy_volture")
                        new_health = health * ((float)ThisPlugin.PercentVolture.Value / 100.0f);
                    else if (text == "$enemy_piece_charred_balista")
                        new_health = health * ((float)ThisPlugin.PercentPieceCharredBalista.Value / 100.0f);
                    else if (text == "$enemy_bloblava")
                        new_health = health * ((float)ThisPlugin.PercentBlobLava.Value / 100.0f);
                    else if (text == "$enemy_dvergerashlands")
                        new_health = health * ((float)ThisPlugin.PercentDvergerAshlands.Value / 100.0f);

                    if (!new_health.HasValue && ThisPlugin.DebugOutput.Value && text != "human")
                        Debug.Log("Casualheim | !!! unmatched enemy normal max health " + text + " :: " + health.ToString());

                    if (new_health.HasValue) {
                        health = new_health.Value;
                        if (ThisPlugin.DebugOutput.Value)
                            Debug.Log("Casualheim.MaxHealthPatch :: " + text + " :: " + health.ToString());
                    }
                }
            }
        }
    }
}
