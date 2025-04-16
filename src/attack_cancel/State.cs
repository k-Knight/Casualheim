﻿using System.Collections.Generic;
using System;

namespace Casualheim.attack_cancel {
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
        public int atk;
        public bool done;
    };

    public struct DamageDone {
        public int atk;
        public float time;
    };

    public static class State {
        public static Dictionary<int, AttackCancel> last_attack_cancel_dict = new Dictionary<int, AttackCancel>();
        public static Dictionary<int, BlockInputState> block_state_dict = new Dictionary<int, BlockInputState>();
        public static Dictionary<int, DamageDone> player_attack_damage_done_dict = new Dictionary<int, DamageDone>();
        public static Dictionary<int, int> player_in_attack_frame_cached = new Dictionary<int, int>();
        public static Dictionary<int, bool> player_started_secondary = new Dictionary<int, bool>();
        public static Dictionary<int, WeakReference<Player>> zanim_player_dict = new Dictionary<int, WeakReference<Player>>();
    };
}
