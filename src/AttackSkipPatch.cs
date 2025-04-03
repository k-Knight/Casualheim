using HarmonyLib;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;

namespace Casualheim {
    public struct BlockInputState {
        public float block_start_time;
        public float attack_start_time;
        public float dodge_end_time;

        public bool block_state;
        public bool attack_state;
        public bool dodge_state;
    };

    public struct AttackCancel {
        public float time;
        public WeakReference<Attack> atk;
    };
    public class AttackCancelPatch {
        static Dictionary<int, AttackCancel> last_attack_cancel_dict = new Dictionary<int, AttackCancel>();
        static Dictionary<int, BlockInputState> block_state_dict = new Dictionary<int, BlockInputState>();
        static Dictionary<int, int> player_attack_damage_done_dict = new Dictionary<int, int>();


        [HarmonyPatch(typeof(Humanoid), "OnAttackTrigger")]
        public class OnAttackTriggerPatch {
            public static void Postfix(Humanoid __instance) {
                if (!ThisPlugin.PluginEnabled.Value || __instance == null)
                    return;

                if (__instance.GetType() != typeof(Player) || __instance.m_currentAttack == null)
                    return;

                Player p = __instance as Player;
                int p_hash = p.GetHashCode();
                int atk_hash = p.m_currentAttack.GetHashCode();

                if (!player_attack_damage_done_dict.ContainsKey(p_hash))
                    player_attack_damage_done_dict.Add(p_hash, atk_hash);
                else
                    player_attack_damage_done_dict[p_hash] = atk_hash;
            }
        }

        [HarmonyPatch(typeof(Player), "InAttack")]
        public class InAttackCancelPatch {
            private static void CancelFreezeFrame(ref Player __instance) {
                if (__instance.GetNextOrCurrentAnimHash() != Humanoid.s_animatorTagAttack)
                    return;

                int p_hash = __instance.GetHashCode();

                if (!last_attack_cancel_dict.ContainsKey(p_hash))
                    return;

                Attack atk;
                if (!last_attack_cancel_dict[p_hash].atk.TryGetTarget(out atk))
                    return;

                if (ThisPlugin.DebugOutput.Value)
                    UnityEngine.Debug.Log("Casualheim | cancelling freeze frame !");

                atk.m_character.FreezeFrame(0f);
                __instance.m_zanim.SetSpeed(10000f);
            }
            private static bool CancelAttack(ref Player __instance, float attack_min_time) {
                if (__instance.m_currentAttack == null)
                    return false;

                float m_time = __instance.m_currentAttack.m_time;

                if (m_time <= attack_min_time)
                    return false;

                if (ThisPlugin.DebugOutput.Value)
                    UnityEngine.Debug.Log("Casualheim | cancelling attack !");

                __instance.m_currentAttack.Abort();
                __instance.m_attack = false;
                __instance.m_attackHold = false;
                __instance.m_secondaryAttack = false;
                __instance.m_secondaryAttackHold = false;

                __instance.m_zanim.SetSpeed(10000f);

                int hash = __instance.GetHashCode();

                if (!last_attack_cancel_dict.ContainsKey(hash))
                    last_attack_cancel_dict.Add(hash, new AttackCancel { time = Time.fixedTime, atk = new WeakReference<Attack>(__instance.m_currentAttack) });
                else
                    last_attack_cancel_dict[hash] = new AttackCancel { time = Time.fixedTime, atk = new WeakReference<Attack>(__instance.m_currentAttack) };

                __instance.m_currentAttack.m_character.FreezeFrame(0f);
                __instance.m_previousAttack = null;
                __instance.m_currentAttack = null;

                return true;
            }

            public static void Postfix(Player __instance, ref bool __result) {
                if (!ThisPlugin.PluginEnabled.Value || __instance == null)
                    return;

                int p_hash = __instance.GetHashCode();
                float time = Time.fixedTime;

                if (last_attack_cancel_dict.ContainsKey(p_hash)) {
                    float delta = time - last_attack_cancel_dict[p_hash].time;

                    if (delta < 0.1f) {
                        if (ThisPlugin.DebugOutput.Value)
                            UnityEngine.Debug.Log("Casualheim | (in attack) attack canceled recently :: " + delta + "s");

                        CancelFreezeFrame(ref __instance);
                        __result = false;

                        return;
                    }
                }

                if (__instance.m_currentAttack == null)
                    return;

                if (player_attack_damage_done_dict.ContainsKey(p_hash) && player_attack_damage_done_dict[p_hash] == __instance.m_currentAttack.GetHashCode())
                    return;


                bool attack_finished = (__instance.m_currentAttack == null) || (__instance.m_currentAttack.IsDone());
                StackFrame[] frames = new StackTrace(fNeedFileInfo: true).GetFrames();
                bool cancel_attack = false;
                bool trying_to_block = false;
                float attack_min_time = 0f;

                // skipping fist because it is from our class
                for (int i = 1; i < frames.Length; i++) {
                    MethodBase method = frames[i].GetMethod();

                    if (method == null)
                        continue;

                    Type decl_type = method.GetRealDeclaringType();
                    string name = method.Name;

                    if (decl_type == null || name == null)
                        continue;

                    if (decl_type == typeof(Character) && name.Contains("Jump")) {
                        cancel_attack = true;
                        attack_min_time = 0f;
                        break;
                    }
                    if (decl_type == typeof(Humanoid) && name.Contains("UpdateBlock")) {
                        bool block_just_began = false;

                        if (!block_state_dict.ContainsKey(p_hash))
                            block_just_began = true;

                        if (!block_just_began) {
                            BlockInputState bs = block_state_dict[p_hash];
                            if (bs.attack_start_time < bs.block_start_time)
                                block_just_began = true;
                        }

                        if (block_just_began) {
                            cancel_attack = true;
                            trying_to_block = true;
                            attack_min_time = 0f;
                        }

                        break;
                    }
                    if (decl_type == typeof(Player) && name.Contains("UpdateDodge")) {
                        float required_stam = __instance.m_dodgeStaminaUsage - __instance.m_dodgeStaminaUsage * __instance.GetEquipmentMovementModifier() + __instance.m_dodgeStaminaUsage * __instance.GetEquipmentDodgeStaminaModifier();
                        __instance.m_seman.ModifyDodgeStaminaUsage(required_stam, ref required_stam, true);

                        if (__instance.HaveStamina(required_stam)) {
                            cancel_attack = true;
                            attack_min_time = 0.1f;
                        }

                        break;
                    }
                }

                if (!cancel_attack || attack_finished)
                    return;

                __instance.ClearActionQueue();
                __result = !CancelAttack(ref __instance, attack_min_time);

                if (!__result && trying_to_block) {
                    __instance.m_nview.GetZDO().Set(ZDOVars.s_isBlockingHash, true);
                    __instance.m_zanim.SetBool(Humanoid.s_blocking, true);
                }
            }
        }

        [HarmonyPatch(typeof(Humanoid), "StartAttack")]
        public class StartAttackCancelPatch {
            public static bool Prefix(Humanoid __instance, ref bool __result) {
                if (!ThisPlugin.PluginEnabled.Value || __instance == null)
                    return true;

                if (__instance.GetType() != typeof(Player))
                    return true;

                Player p = __instance as Player;
                int p_hash = p.GetHashCode();
                float time = Time.fixedTime;

                if (last_attack_cancel_dict.ContainsKey(p_hash)) {
                    float delta = time - last_attack_cancel_dict[p_hash].time;
                    if (delta < 0.1f) {
                        if (ThisPlugin.DebugOutput.Value)
                            UnityEngine.Debug.Log("Casualheim | (start attack) attack canceled recently :: " + delta + "s");

                        __result = false;
                        return false;
                    }
                }
                else if (block_state_dict.ContainsKey(p_hash)) {
                    BlockInputState bs = block_state_dict[p_hash];

                    if ((bs.block_state && bs.block_start_time > bs.attack_start_time) || bs.dodge_state || (time - bs.dodge_end_time < 0.15f) || bs.attack_state == false) {
                        __result = false;
                        return false;
                    }
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(Player), "SetControls")]
        public class SetControlsPatch {
            public static void Prefix(Player __instance, ref bool attack, ref bool attackHold, ref bool secondaryAttack, ref bool secondaryAttackHold, ref bool block, ref bool blockHold, ref bool dodge) {
                if (!ThisPlugin.PluginEnabled.Value || __instance == null)
                    return;

                int p_hash = __instance.GetHashCode();
                bool block_state = block || blockHold;
                bool attack_state = attack || attackHold || secondaryAttack || secondaryAttackHold;
                bool doldge_state = __instance.m_inDodge;
                float block_start_time;
                float attack_start_time;
                float dodge_end_time;
                float time = Time.fixedTime;

                if (!block_state_dict.ContainsKey(p_hash)) {
                    block_start_time = block_state ? time : -2f;
                    dodge_end_time = doldge_state ? time : -2f;
                    attack_start_time = attack_state ? time : -1f;

                    block_state_dict.Add(p_hash, new BlockInputState {
                        block_state = block_state,
                        block_start_time = block_start_time,

                        attack_state = attack_state,
                        attack_start_time = attack_start_time,

                        dodge_state = doldge_state,
                        dodge_end_time = dodge_end_time
                    });
                }
                else {
                    BlockInputState bs = block_state_dict[p_hash];
                    block_start_time = (!bs.block_state && block_state) ? time : bs.block_start_time;
                    attack_start_time = (!bs.attack_state && attack_state) ? time : bs.attack_start_time;
                    dodge_end_time = (!doldge_state) ? bs.dodge_end_time : time;

                    block_state_dict[p_hash] = new BlockInputState {
                        block_state = block_state,
                        block_start_time = block_start_time,

                        attack_state = attack_state,
                        attack_start_time = attack_start_time,

                        dodge_state = doldge_state,
                        dodge_end_time = dodge_end_time
                    };
                }

                if ((block_state && block_start_time > attack_start_time) || (doldge_state || (time - dodge_end_time < 0.15f))) {
                    attack = false;
                    attackHold = false;
                    secondaryAttack = false;
                    secondaryAttackHold = false;

                    return;
                }

                if (!last_attack_cancel_dict.ContainsKey(p_hash))
                    return;

                float delta = Time.fixedTime - last_attack_cancel_dict[p_hash].time;
                if (delta >= 0.1f)
                    return;

                attack = false;
                attackHold = false;
                secondaryAttack = false;
                secondaryAttackHold = false;
            }
        }
    }
}
