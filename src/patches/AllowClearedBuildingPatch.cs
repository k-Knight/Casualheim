using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Policy;
using HarmonyLib;
using UnityEngine;


namespace Casualheim.patches {
    [HarmonyPatch]
    public class AllowClearedBuildingPatch {
        public static int last_zone_location_hash = -1;
        public static int last_creature_spawner_hash = -1;
        public static int last_character_drop_hash = -1;
        public static Dictionary<int, int> loc_2_zloc_dict = new Dictionary<int, int>();

        public static bool check_caller(string caller_method) {
            StackFrame[] frames = new StackTrace(fNeedFileInfo: true).GetFrames();

            for (int i = 1; i < frames.Length; i++)
                if (frames[i].GetMethod().Name == caller_method)
                    return true;

            return false;
        }

        public static bool GetLocation(ref CreatureSpawner cs, out Location loc) {
            if (cs.m_checkedLocation)
                loc = cs.m_location;
            else {
                cs.m_location = Location.GetLocation(cs.transform.position, true);
                cs.m_checkedLocation = true;
                loc = cs.m_location;
            }

            if (loc == null)
                return false;

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Location), "IsInside")]
        public static void LocationIsInsideLocationPatch(ref Location __instance, ref bool __result, ref bool buildCheck, ref Vector3 point) {
            if (__instance == null || !buildCheck || !__instance.m_noBuild || !__result)
                return;

            if (!ThisPlugin.PluginEnabled.Value || !ThisPlugin.AllowClearedBuilding.Value)
                return;

            if (!loc_2_zloc_dict.ContainsKey(__instance.GetHashCode())) {
                if (ThisPlugin.DebugOutput.Value)
                    UnityEngine.Debug.Log("Casualheim | Location.IsInside location is not in the dictrionary !!!");

                return;
            }

            bool units_dead = true;
            int zloc_hash = loc_2_zloc_dict[__instance.GetHashCode()];
            ZoneSystem.ZoneLocation zloc = ZoneSystem.m_instance.GetLocation(zloc_hash);
            if (zloc == null)
                return;


            foreach (var cs in CreatureSpawner.m_creatureSpawners) {
                if (cs == null || cs.m_nview == null)
                    continue;

                ZDO zdo = cs.m_nview.GetZDO();
                if (zdo == null)
                    continue;

                int zdo_zloc_hash = zdo.GetInt("zloc_hash");
                ZDOID connectionZDOID = zdo.GetConnectionZDOID(ZDOExtraData.ConnectionType.Spawned);

                if (zloc_hash == zdo_zloc_hash && cs.SpawnedCreatureStillExists(connectionZDOID)) {
                    units_dead = false;
                    if (ThisPlugin.DebugOutput.Value)
                        UnityEngine.Debug.Log("Casualheim | creaturespawner [" + cs.GetHashCode() + "] says char still exists for zloc [" + zloc_hash + "] !");

                    break;
                }

            }

            if (!units_dead)
                return;

            foreach (var character in Character.s_characters) {
                if (character == null || character.m_nview == null)
                    continue;

                ZDO zdo = character.m_nview.GetZDO();
                if (zdo == null)
                    continue;

                int zdo_zloc_hash = zdo.GetInt("zloc_hash");

                if (zloc_hash == zdo_zloc_hash) {
                    units_dead = false;
                    if (ThisPlugin.DebugOutput.Value)
                        UnityEngine.Debug.Log("Casualheim | found char " + character.m_name.ToLower() + " that is still alive for zloc [" + zloc_hash + "] !");

                    break;
                }

            }

            if (!units_dead)
                return;

            __instance.m_noBuild = false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Location), "Awake")]
        public static void LocatioAwakenPatch(ref Location __instance) {
            if (!ThisPlugin.PluginEnabled.Value || !ThisPlugin.AllowClearedBuilding.Value)
                return;

            if (!check_caller("DMD<ZoneSystem::SpawnLocation>"))
                return;

            loc_2_zloc_dict.Add(__instance.GetHashCode(), last_zone_location_hash);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharacterDrop), "OnDeath")]
        public static void CharacterDropOnDeathPatch(ref CharacterDrop __instance) {
            if (!ThisPlugin.PluginEnabled.Value || !ThisPlugin.AllowClearedBuilding.Value)
                return;

            last_character_drop_hash = 0;

            if (__instance.m_character == null || __instance.m_character.m_nview == null)
                return;

            ZDO zdo = __instance.m_character.m_nview.GetZDO();
            if (zdo == null)
                return;

            last_character_drop_hash = zdo.GetInt("zloc_hash");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ZoneSystem), "SpawnLocation")]
        public static void ZoneSystemSpawnLocationPatch(ref ZoneSystem.ZoneLocation location) {
            last_zone_location_hash = 0; // just in case next line fails
            last_zone_location_hash = location.Hash;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Character), "Awake")]
        public static void CharacterAwakePatch(ref Character __instance) {
            if (!ThisPlugin.PluginEnabled.Value || !ThisPlugin.AllowClearedBuilding.Value)
                return;

            bool from_cs = false;
            bool from_cd = false;

            if (check_caller("DMD<CreatureSpawner::Spawn>"))
                from_cs = true;
            else if (check_caller("DMD<CharacterDrop::OnDeath>"))
                from_cd = true;

            if (!from_cs && !from_cd)
                return;

            if (__instance.m_nview == null || !__instance.m_nview.IsOwner())
                return;

            ZDO zdo = __instance.m_nview.GetZDO();
            if (zdo == null)
                return;

            if (ThisPlugin.DebugOutput.Value)
                UnityEngine.Debug.Log("Casualheim | setting zloc_hash [" + (from_cs ? last_creature_spawner_hash : (from_cd ? last_character_drop_hash : -1)) + "] for char :: " + __instance.m_name.ToLower());

            if (from_cs)
                zdo.Set("zloc_hash", last_creature_spawner_hash);
            else if (from_cd)
                zdo.Set("zloc_hash", last_character_drop_hash);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CreatureSpawner), "Awake")]
        public static void CreatureSpawnerAwakePatch(ref CreatureSpawner __instance) {
            if (!ThisPlugin.PluginEnabled.Value || !ThisPlugin.AllowClearedBuilding.Value)
                return;

            if (!check_caller("DMD<ZoneSystem::SpawnLocation>"))
                return;

            if (__instance.m_nview == null || !__instance.m_nview.IsOwner())
                return;

            ZDO zdo = __instance.m_nview.GetZDO();

            if (zdo == null) {
                if (ThisPlugin.DebugOutput.Value)
                    UnityEngine.Debug.Log("Casualheim | !!!   !!!   no ZDO for CreatureSpawner   !!!   !!!");

                return;
            }

            zdo.Set("zloc_hash", last_zone_location_hash);
        }

        // Propogation mechanism for zloc_hash into spawners inside the group (probably not needed)
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CreatureSpawner), "UpdateSpawner")]
        public static void CreatureSpawnerUpdateSpawnerPatch(ref CreatureSpawner __instance) {
            if (!ThisPlugin.PluginEnabled.Value || !ThisPlugin.AllowClearedBuilding.Value || __instance == null || __instance.m_nview == null || __instance.m_spawnGroup == null)
                return;

            ZDO zdo = __instance.m_nview.GetZDO();
            if (zdo == null)
                return;

            int zloc_hash = zdo.GetInt("zloc_hash");
            if (zloc_hash == 0)
                return;

            Location loc;
            if (GetLocation(ref __instance, out loc))
                return;

            List<CreatureSpawner> group_spawners = new List<CreatureSpawner>();
            foreach (CreatureSpawner creatureSpawner in __instance.m_spawnGroup)
                group_spawners.Add(creatureSpawner);

            for (int i = 0; i < group_spawners.Count; i++) {
                try {
                    CreatureSpawner cs = group_spawners[i];
                    if (cs == null || cs.m_lastSpawnID == null)
                        continue;

                    if (cs.m_lastSpawnID.IsNone() || (cs.CanRespawnNow(cs.m_lastSpawnID) && !cs.SpawnedCreatureStillExists(cs.m_lastSpawnID))) {
                        if (cs.m_nview == null || !cs.m_nview.IsOwner())
                            return;

                        ZDO zdo_group = cs.m_nview.GetZDO();
                        if (zdo_group == null)
                            continue;

                        int zloc_hash_group = zdo_group.GetInt("zloc_hash");
                        if (zloc_hash_group != 0)
                            continue;

                        Location group_loc;
                        if (GetLocation(ref cs, out group_loc))
                            continue;

                        if (ThisPlugin.DebugOutput.Value)
                            UnityEngine.Debug.Log("Casualheim | !!!   !!!   no ZDO for CreatureSpawner   !!!   !!!");

                        if (group_loc.GetHashCode() == loc.GetHashCode())
                            zdo_group.Set("zloc_hash", zloc_hash);
                    }
                } catch (Exception e) {
                    continue;
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CreatureSpawner), "Spawn")]
        public static void CreatureSpawnerSpawnPatch(ref CreatureSpawner __instance) {
            if (!ThisPlugin.PluginEnabled.Value || !ThisPlugin.AllowClearedBuilding.Value)
                return;

            last_creature_spawner_hash = 0;

            if (__instance.m_nview == null)
                return;

            ZDO zdo = __instance.m_nview.GetZDO();
            if (zdo == null)
                return;

            last_creature_spawner_hash = zdo.GetInt("zloc_hash");
        }
    }
}
