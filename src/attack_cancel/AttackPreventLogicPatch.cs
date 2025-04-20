using HarmonyLib;
using UnityEngine;

namespace Casualheim.attack_cancel {
    [HarmonyPatch]
    public class AttackPreventLogicPatch {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Attack), "Update")]
        [HarmonyPatch(typeof(Attack), "OnAttackTrigger")]
        [HarmonyPatch(typeof(Attack), "ConsumeItem")]
        [HarmonyPatch(typeof(Attack), "UseAmmo")]
        [HarmonyPatch(typeof(Attack), "FireProjectileBurst")]
        [HarmonyPatch(typeof(Attack), "DoNonAttack")]
        [HarmonyPatch(typeof(Attack), "DoAreaAttack")]
        [HarmonyPatch(typeof(Attack), "AddHitPoint")]
        [HarmonyPatch(typeof(Attack), "DoMeleeAttack")]
        [HarmonyPatch(typeof(Attack), "SpawnOnHit")]
        [HarmonyPatch(typeof(Attack), "TryAttach")]
        [HarmonyPatch(typeof(Attack), "UpdateAttach")]
        public static bool PreventLogic(ref Attack __instance) {
            if (__instance.m_abortAttack && ThisPlugin.PluginEnabled.Value && ThisPlugin.EnableAttackMod.Value)
                return false;

            return true;
        }
    };
}
