using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;

namespace Casualheim {
    public static class Util {
        public class CallStackPrinter {
            public HashSet<string> unique_callers = new HashSet<string>();
            public string caller_name;

            public CallStackPrinter(string caller_name) {
                this.caller_name = caller_name;
            }

            public void print_callstack(bool force_print = false) {
                StackFrame[] frames = new StackTrace(fNeedFileInfo: true).GetFrames();

                string accum = "\n";
                for (int i = 1; i < frames.Length; i++)
                    accum += "    " + new String(' ', i * 2) + frames[i].GetMethod().ToString() + "\n";

                if (!force_print && unique_callers.Contains(accum))
                    return;

                unique_callers.Add(accum);

                UnityEngine.Debug.Log("");
                UnityEngine.Debug.Log("in " + caller_name);
                UnityEngine.Debug.Log(accum);
            }
        }

        public static bool check_caller(string caller_method) {
            StackFrame[] frames = new StackTrace(fNeedFileInfo: true).GetFrames();

            for (int i = 2; i < frames.Length; i++)
                if (frames[i].GetMethod().Name == caller_method)
                    return true;

            return false;
        }

        public static bool check_caller(Type caller_type, string caller_name_part) {
            StackFrame[] frames = new StackTrace(fNeedFileInfo: true).GetFrames();

            for (int i = 2; i < frames.Length; i++) {
                MethodBase method = frames[i].GetMethod();
                Type decl_type = method.GetRealDeclaringType();

                if (decl_type == caller_type && method.Name.Contains(caller_name_part))
                    return true;
            }

            return false;
        }

        public static bool GetLocation<T>(ref T obj, out Location loc) where T : MonoBehaviour {
            loc = Location.GetLocation(obj.transform.position, true);

            if (loc == null)
                return false;

            return true;
        }

        public static bool GetSyncThings<T>(ref T obj, out ZNetView nview, out ZDO zdo) where T : MonoBehaviour {
            zdo = null;
            nview = null;

            if (typeof(CreatureSpawner).IsAssignableFrom(obj.GetType()))
                nview = (obj as CreatureSpawner).m_nview;
            else if (typeof(SpawnArea).IsAssignableFrom(obj.GetType()))
                nview = (obj as SpawnArea).m_nview;
            else if (typeof(Character).IsAssignableFrom(obj.GetType()))
                nview = (obj as Character).m_nview;

            if (nview == null)
                return false;

            zdo = nview.GetZDO();
            if (zdo == null) {
                if (ThisPlugin.DebugOutput.Value)
                    UnityEngine.Debug.Log("Casualheim | !!!   !!!   no ZDO for " + obj.GetType() + "   !!!   !!!");

                return false;
            }

            return true;
        }

        public static bool TryGetPlayerZDO(ref Character character, out Player player, out ZDO zdo) {
            player = null;
            zdo = null;

            if (character.GetType() != typeof(Player))
                return false;

            player = character as Player;
            return TryGetPlayerZDO(ref player, out zdo);
        }

        public static bool TryGetPlayerZDO(ref Player player, out ZDO zdo) {
            zdo = player.m_nview.GetZDO();
            if (zdo == null) {
                if (ThisPlugin.DebugOutput.Value)
                    UnityEngine.Debug.Log("Casualheim.StatModificationPatch | zdo of a player is null !!!");

                return false;
            }

            return true;
        }
    }
}
