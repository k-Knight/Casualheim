using HarmonyLib;
using MonoMod.Utils;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Security.Policy;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

namespace Casualheim.attack_cancel {
    [HarmonyPatch]
    public class AttackCancelPatch {
        public static bool ApplyAnimSpeedup(ref Player p) {
            var wpn = p.GetCurrentWeapon();
            if (wpn != null && wpn.m_shared != null && wpn.m_shared.m_name != null) {
                string name = wpn.m_shared.m_name;

                if (name.Contains("spear"))
                    return false;
                else if (name.Contains("pickaxe"))
                    return false;
            }

            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "Awake")]
        public static void PlayerAwakePatch(ref Player __instance) {
            if (__instance.m_zanim == null) {
                if (ThisPlugin.DebugOutput.Value)
                    UnityEngine.Debug.Log("Casualheim.PlayerAwakePatch | player m_zanim is null !!!");

                return;
            }

            int p_hash = __instance.GetHashCode();
            State.player_in_attack_frame_cached[p_hash] = MonoUpdaters.UpdateCount;
            State.player_started_secondary[p_hash] = false;
            State.zanim_player_dict.Add(__instance.m_zanim.GetHashCode(), new WeakReference<Player>(__instance));
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ZSyncAnimation), "RPC_SetTrigger")]
        public static bool ZSyncAnimationRPC_SetTriggerPatch(ref ZSyncAnimation __instance, ref string name) {
            if (!name.StartsWith("csca!"))
                return true;

            int hash = __instance.GetHashCode();
            if (!State.zanim_player_dict.ContainsKey(hash))
                return true;

            Player p;
            if (!State.zanim_player_dict[hash].TryGetTarget(out p))
                return true;

            if (name == "csca!stop") {
                if (p.m_currentAttack != null)
                    p.m_currentAttack.Stop();

                return false;
            }

            var animator = p.m_animator;
            var animEvent = p.m_animEvent;

            animEvent.m_pauseTimer = -1f;

            if (ApplyAnimSpeedup(ref p))
                animator.speed = 1000f;
            else
                animator.speed = 1f;

            animator.ForceStateNormalizedTime(0.99f);

            //int l1_anim_hash = animator.GetCurrentAnimatorStateInfo(0).fullPathHash;
            //int l2_anim_hash = animator.GetCurrentAnimatorStateInfo(1).fullPathHash;

            //animator.Play(l1_anim_hash, 0, 0.9999f);
            //animator.Play(l2_anim_hash, 1, 0.9999f);

            return false;
        }

        public static void SkipCurrentAttackAnimation(ref Player p) {
            if (p.GetNextAnimHash() != Humanoid.s_animatorTagAttack && p.GetCurrentAnimHash() != Humanoid.s_animatorTagAttack) {
                if (ThisPlugin.DebugOutput.Value)
                    UnityEngine.Debug.Log("Casualheim.SkipCurrentAttackAnimation | no animation is an attack animation !!!");

                return;
            }

            p.m_nview.InvokeRPC(ZNetView.Everybody, "SetTrigger", new object[] { "csca!" });
        }

        public static bool CancelAttack(ref Player p, float attack_min_time) {
            if (p.m_currentAttack == null)
                return false;

            Attack atk = p.m_currentAttack;
            int p_hash = p.GetHashCode();
            int atk_hash = atk.GetHashCode();
            float m_time = atk.m_time;
            bool last_cancel_contains = State.last_attack_cancel_dict.ContainsKey(p_hash);
            bool already_cancelled = last_cancel_contains ? State.last_attack_cancel_dict[p_hash].atk == atk_hash : false;
            bool done = false;

            if (last_cancel_contains && already_cancelled) {
                done = State.last_attack_cancel_dict[p_hash].done;
                return true;
            }

            if (m_time <= attack_min_time)
                return false;

            if (ThisPlugin.DebugOutput.Value)
                UnityEngine.Debug.Log("Casualheim.CancelAttack | cancelling");

            atk.Abort();
            SkipCurrentAttackAnimation(ref p);
            atk.m_zanim.SetTrigger("attack_abort");
            atk.m_zanim.SetTrigger("detach");
            p.m_previousAttack = null;
            State.last_attack_cancel_dict[p_hash] = new AttackCancel {
                time = Time.fixedTime,
                atk = atk_hash,
                done = done
            };

            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Humanoid), "OnAttackTrigger")]
        public static void HumanoidOnAttackTriggerPatch(Humanoid __instance) {
            if (!ThisPlugin.PluginEnabled.Value || !ThisPlugin.EnableAttackMod.Value)
                return;

            if (__instance.GetType() != typeof(Player) || __instance.m_currentAttack == null)
                return;

            Player p = __instance as Player;
            Attack atk = p.m_currentAttack;

            //if (atk.m_abortAttack)
            //    return;

            int p_hash = p.GetHashCode();
            int atk_hash = atk.GetHashCode();

            if (!State.player_attack_damage_done_dict.ContainsKey(p_hash))
                State.player_attack_damage_done_dict.Add(p_hash, new DamageDone { atk = atk_hash, time = Time.fixedTime });
            else
                State.player_attack_damage_done_dict[p_hash] = new DamageDone { atk = atk_hash, time = Time.fixedTime };
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Player), "Dodge")]
        public static bool PlayerDodgePatch(Player __instance, ref Vector3 dodgeDir) {
            if (!ThisPlugin.PluginEnabled.Value || !ThisPlugin.PreventDodgeSpamming.Value)
                return true;

            if (!__instance.InDodge())
                __instance.m_queuedDodgeTimer = 0.5f;

            __instance.m_queuedDodgeDir = dodgeDir;

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Player), "UpdateDodge")]
        public static bool PlayerUpdateDodgePatch(Player __instance, ref float dt) {
            if (!ThisPlugin.PluginEnabled.Value || !ThisPlugin.EnableAttackMod.Value)
                return true;

            if (__instance.InDodge())
                return true;

            int p_hash = __instance.GetHashCode();

            if (!State.block_state_dict.ContainsKey(p_hash) || (Time.fixedTime - State.block_state_dict[p_hash].block_start_time) > 0.1f)
                return true;

            __instance.m_queuedDodgeTimer -= dt;
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "InAttack")]
        public static void PlayerInAttackCancelPatch(Player __instance, ref bool __result) {
            if (!ThisPlugin.PluginEnabled.Value || !ThisPlugin.EnableAttackMod.Value)
                return;

            int p_hash = __instance.GetHashCode();
            float time = Time.fixedTime;

            if (State.player_in_attack_frame_cached[p_hash] == MonoUpdaters.UpdateCount)
                return;

            DamageDone damageDone;
            bool damage_done_long_ago = false;
            bool same_attack = false;
            if (State.player_attack_damage_done_dict.TryGetValue(p_hash, out damageDone) && __instance.m_currentAttack != null) {
                same_attack = damageDone.atk == __instance.m_currentAttack.GetHashCode();
                float diff = (time - damageDone.time);

                damage_done_long_ago = diff > 1.5f;
            }

            if (State.last_attack_cancel_dict.ContainsKey(p_hash) && !(State.last_attack_cancel_dict[p_hash].done)) {
                float delta = time - State.last_attack_cancel_dict[p_hash].time;

                if (delta < 0.1f) {
                    if (ThisPlugin.DebugOutput.Value)
                        UnityEngine.Debug.Log("Casualheim | (in attack) attack canceled recently :: " + delta + "s  ---  " + MonoUpdaters.UpdateCount);

                    SkipCurrentAttackAnimation(ref __instance);
                    __result = false;
                    __instance.m_cachedAttack = __result;
                    State.player_in_attack_frame_cached[p_hash] = MonoUpdaters.UpdateCount;

                    return;
                }
                else {
                    var atk_cancel = State.last_attack_cancel_dict[p_hash];
                    atk_cancel.done = true;
                    State.last_attack_cancel_dict[p_hash] = atk_cancel;
                }
            }

            if (__instance.m_currentAttack == null)
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

                    if (!State.block_state_dict.ContainsKey(p_hash))
                        block_just_began = true;

                    if (!block_just_began) {
                        BlockInputState bs = State.block_state_dict[p_hash];
                        if (bs.attack_start_time < bs.block_start_time)
                            block_just_began = true;
                    }

                    if (block_just_began) {
                        cancel_attack = true;
                        trying_to_block = true;
                        //if (State.player_started_secondary[p_hash])
                        //    attack_min_time = 0.2f;
                        //else
                            attack_min_time = 0.1f;

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

            if (!cancel_attack) {
                PlayerAttackControls p_ctrl;
                bool in_attack_stop = false;

                State.player_attack_stop.TryGetValue(p_hash, out in_attack_stop);

                if (!in_attack_stop && __instance.m_currentAttack != null && State.player_controls.TryGetValue(p_hash, out p_ctrl)) {
                    Attack atk = __instance.m_currentAttack;
                    bool is_channeling_attack = atk.m_projectileBursts > 1;
                    bool channeling_done = atk.m_projectileBursts == atk.m_projectileBurstsFired;

                    if (!atk.m_attackDone && (p_ctrl.atkHold || p_ctrl.secAtkHold) && is_channeling_attack && !channeling_done) {
                        if (ThisPlugin.DebugOutput.Value)
                            UnityEngine.Debug.Log("PlayerInAttackCancelPatch() :: preventing channeling attack from ending !!!");

                        __result = true;

                        return;
                    }

                    float diff = 10f;
                    if (State.last_started_attack_time.TryGetValue(p_hash, out diff))
                        diff = time - diff;

                    if (diff > 0.2 && is_channeling_attack && ((!p_ctrl.atk && !p_ctrl.atkHold && !p_ctrl.secAtk && !p_ctrl.secAtkHold) || channeling_done)) {
                        if (ThisPlugin.DebugOutput.Value)
                            UnityEngine.Debug.Log("stopping burst attack ::"
                                + "\n\tm_projectileBurstsFired :: " + atk.m_projectileBurstsFired
                                + "\n\tm_projectileBursts :: " + atk.m_projectileBursts
                                + "\n\tm_projectileAttackStarted :: " + atk.m_projectileAttackStarted
                                + "\n\tm_projectileFireTimer :: " + atk.m_projectileFireTimer
                                + "\n\tm_attackType :: " + atk.m_attackType
                            );

                        State.player_attack_stop[p_hash] = true;
                        __instance.m_nview.InvokeRPC(ZNetView.Everybody, "SetTrigger", new object[] { "csca!stop" });
                        __instance.m_currentAttack = null;
                    }
                }
            }

            if (same_attack && !damage_done_long_ago && !trying_to_block)
                return;

            if (!cancel_attack || (attack_finished && !damage_done_long_ago))
                return;

            __instance.ClearActionQueue();
            __result = !CancelAttack(ref __instance, attack_min_time);
            __instance.m_cachedAttack = __result;

            if (!__result && trying_to_block) {
                __instance.m_nview.GetZDO().Set(ZDOVars.s_isBlockingHash, true);
                __instance.m_zanim.SetBool(Humanoid.s_blocking, true);
            }

            State.player_in_attack_frame_cached[p_hash] = MonoUpdaters.UpdateCount;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Player), "StartEmote")]
        public static bool PlayerStartEmotePatch(Player __instance) {
            if (!ThisPlugin.PluginEnabled.Value || !ThisPlugin.EnableAttackMod.Value)
                return true;

            bool attack_finished = (__instance.m_currentAttack == null) || (__instance.m_currentAttack.IsDone());
            float attack_start_time = -10f;
            int p_hash = __instance.GetHashCode();
            float time = Time.fixedTime;

            if (State.last_started_attack_time.ContainsKey(p_hash))
                attack_start_time = State.last_started_attack_time[p_hash];

            if (!attack_finished || ((time - attack_start_time) < 0.25f))
                return false;

            if (ThisPlugin.DebugOutput.Value)
                UnityEngine.Debug.Log("Casualheim | not cancelling StartEmote() !!!");

            State.last_started_emote_time[p_hash] = time;

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Humanoid), "StartAttack")]
        public static bool HumanoidStartAttackCancelPatch_Prefix(Humanoid __instance, ref bool __result) {
            if (!ThisPlugin.PluginEnabled.Value || !ThisPlugin.EnableAttackMod.Value)
                return true;

            if (__instance.GetType() != typeof(Player))
                return true;

            Player p = __instance as Player;
            int p_hash = p.GetHashCode();
            float time = Time.fixedTime;

            if (State.last_ended_emote_time.ContainsKey(p_hash) && (time - State.last_ended_emote_time[p_hash] < 0.25f)) {
                __result = false;
                return false;
            }

            if (p.InEmote() || (State.last_started_emote_time.ContainsKey(p_hash) && (time - State.last_started_emote_time[p_hash] < 0.25f))) {
                p.StopEmote();
                State.last_ended_emote_time[p_hash] = time;

                __result = false;
                return false;
            }

            if (State.last_attack_cancel_dict.ContainsKey(p_hash)) {
                float delta = time - State.last_attack_cancel_dict[p_hash].time;
                if (delta < 0.5f) {
                    if (ThisPlugin.DebugOutput.Value)
                        UnityEngine.Debug.Log("Casualheim.HumanoidStartAttackCancelPatch | attack canceled recently :: " + delta + "s");

                    __result = false;
                    return false;
                }
            }
            else if (State.block_state_dict.ContainsKey(p_hash)) {
                BlockInputState bs = State.block_state_dict[p_hash];

                if ((bs.block_state && bs.block_start_time > bs.attack_start_time) || bs.dodge_state || (time - bs.dodge_end_time < 0.15f) || bs.attack_state == false) {
                    if (ThisPlugin.DebugOutput.Value)
                        UnityEngine.Debug.Log("Casualheim.HumanoidStartAttackCancelPatch | block/dodge");

                    __result = false;
                    return false;
                }
            }

            if (ThisPlugin.DebugOutput.Value)
                UnityEngine.Debug.Log("Casualheim.HumanoidStartAttackCancelPatch | starting attack ...");

            State.last_started_attack_time[p_hash] = time;
            State.player_attack_stop[p_hash] = false;

            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Humanoid), "StartAttack")]
        public static void HumanoidStartAttackCancelPatch_Postfix(Humanoid __instance, ref bool __result, ref bool secondaryAttack) {
            if (!ThisPlugin.PluginEnabled.Value || !ThisPlugin.EnableAttackMod.Value)
                return;

            if (__result && __instance.IsPlayer())
                State.player_started_secondary[(__instance as Player).GetHashCode()] = secondaryAttack;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Player), "SetControls")]
        public static void PlayerSetControlsPatch(Player __instance, ref bool attack, ref bool attackHold, ref bool secondaryAttack, ref bool secondaryAttackHold, ref bool block, ref bool blockHold, ref bool dodge) {
            if (!ThisPlugin.PluginEnabled.Value || !ThisPlugin.EnableAttackMod.Value)
                return;

            int p_hash = __instance.GetHashCode();
            bool block_state = block || blockHold;
            bool attack_state = attack || attackHold || secondaryAttack || secondaryAttackHold || (__instance.m_attackDrawTime > 0.01f);
            bool doldge_state = __instance.m_inDodge;
            float block_start_time;
            float attack_start_time;
            float dodge_end_time;
            float time = Time.fixedTime;

            if (!State.block_state_dict.ContainsKey(p_hash)) {
                block_start_time = block_state ? time : -2f;
                dodge_end_time = doldge_state ? time : -2f;
                attack_start_time = attack_state ? time : -1f;

                State.block_state_dict.Add(p_hash, new BlockInputState {
                    block_state = block_state,
                    block_start_time = block_start_time,

                    attack_state = attack_state,
                    attack_start_time = attack_start_time,

                    dodge_state = doldge_state,
                    dodge_end_time = dodge_end_time
                });
            }
            else {
                BlockInputState bs = State.block_state_dict[p_hash];
                block_start_time = (!bs.block_state && block_state) ? time : bs.block_start_time;
                attack_start_time = (!bs.attack_state && attack_state) ? time : bs.attack_start_time;
                dodge_end_time = (!doldge_state) ? bs.dodge_end_time : time;

                State.block_state_dict[p_hash] = new BlockInputState {
                    block_state = block_state,
                    block_start_time = block_start_time,

                    attack_state = attack_state,
                    attack_start_time = attack_start_time,

                    dodge_state = doldge_state,
                    dodge_end_time = dodge_end_time
                };
            }

            bool skip = false;

            if ((block_state && block_start_time > attack_start_time) || (doldge_state || (time - dodge_end_time < 0.15f))) {
                if (ThisPlugin.DebugOutput.Value)
                    UnityEngine.Debug.Log("Casualheim.PlayerSetControlsPatch | stopping attack input due to block/dodge");

                attack = false;
                attackHold = false;
                secondaryAttack = false;
                secondaryAttackHold = false;

                skip = true;
            }

            if (!skip && !State.last_attack_cancel_dict.ContainsKey(p_hash))
                skip = true;

            if (!skip) {
                float delta = Time.fixedTime - State.last_attack_cancel_dict[p_hash].time;
                if (delta >= 0.1f)
                    skip = true;
            }

            if (!skip && ThisPlugin.DebugOutput.Value)
                UnityEngine.Debug.Log("Casualheim.PlayerSetControlsPatch | stopping attack input due recent cancelled attack");

            if (!skip) {
                attack = false;
                attackHold = false;
                secondaryAttack = false;
                secondaryAttackHold = false;
            }

            State.player_controls[p_hash] = new PlayerAttackControls {
                atk = attack,
                atkHold = attackHold,
                secAtk = secondaryAttack,
                secAtkHold = secondaryAttackHold
            };
        }
    }
}
