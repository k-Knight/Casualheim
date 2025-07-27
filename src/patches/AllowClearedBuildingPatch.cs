using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;


namespace Casualheim.patches {
    [HarmonyPatch]
    public class AllowClearedBuildingPatch {
        public static int last_zone_location_hash = -1;
        public static int last_creature_spawner_hash = -1;
        public static int last_character_drop_hash = -1;
        public static Dictionary<int, int> loc_2_zloc_dict = new Dictionary<int, int>();

        public static bool AssignLocHashDeterministic<T>(ref T obj, out ZDO zdo, out int zloc_hash) where T : MonoBehaviour {
            zloc_hash = 0;
            zdo = null;
            ZNetView nview;

            if (!Util.check_caller("DMD<ZoneSystem::SpawnLocation>"))
                return false;

            if (!Util.GetSyncThings(ref obj, out nview, out zdo))
                return false;
            if (!nview.IsOwner())
                return false; ;

            bool loc_checked = zdo.GetBool("zloc_checked");
            if (loc_checked) {
                zloc_hash = zdo.GetInt("zloc_hash");
                return true;
            }

            zloc_hash = last_zone_location_hash;
            zdo.Set("zloc_checked", true);
            zdo.Set("zloc_hash", zloc_hash);

            if (ThisPlugin.DebugOutput.Value)
                Debug.Log("Casualheim | DETERMINISICALLY found zloc hash for " + obj.GetType() + " :: " + zloc_hash);

            return true;
        }

        // requires outside check of whether we are the owner of ZNetView
        public static bool AssignLocHashHeuristic<T>(ref T obj, ref ZDO zdo, out int zloc_hash) where T : MonoBehaviour {
            zloc_hash = 0;

            bool loc_checked = zdo.GetBool("zloc_checked");
            if (loc_checked) {
                zloc_hash = zdo.GetInt("zloc_hash");
                return true;
            }

            Location loc;
            Util.GetLocation(ref obj, out loc);
            if (loc == null)
                return false;

            if (!loc_2_zloc_dict.ContainsKey(loc.GetHashCode())) {
                if (ThisPlugin.DebugOutput.Value)
                    Debug.Log("Casualheim | CreatureSpawner.Spawn location is not in the dictrionary !!!");

                return false;
            }

            zloc_hash = loc_2_zloc_dict[loc.GetHashCode()];
            zdo.Set("zloc_checked", true);
            zdo.Set("zloc_hash", zloc_hash);

            if (ThisPlugin.DebugOutput.Value)
                Debug.Log("Casualheim | HEURISTICALLY found zloc hash for " + obj.GetType() + " :: " + zloc_hash);

            return true;
        }

        public static void BeforeSpawnerUnitSpawn<T>(ref T obj) where T : MonoBehaviour {
            if (!ThisPlugin.PluginEnabled.Value || !ThisPlugin.AllowClearedBuilding.Value)
                return;

            last_creature_spawner_hash = 0;

            ZDO zdo;
            ZNetView nview;

            if (!Util.GetSyncThings(ref obj, out nview, out zdo))
                return;

            last_creature_spawner_hash = zdo.GetInt("zloc_hash");
            if (last_creature_spawner_hash != 0)
                return;

            int zloc_hash;
            if (nview.IsOwner() && AssignLocHashHeuristic(ref obj, ref zdo, out zloc_hash))
                last_creature_spawner_hash = zloc_hash;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Location), "IsInside")]
        public static void LocationIsInsidePatch(ref Location __instance, ref bool buildCheck, ref Vector3 point) {
            if (__instance == null || !buildCheck || !__instance.m_noBuild)
                return;

            if (!ThisPlugin.PluginEnabled.Value || !ThisPlugin.AllowClearedBuilding.Value)
                return;


            if (!loc_2_zloc_dict.ContainsKey(__instance.GetHashCode())) {
                if (ThisPlugin.DebugOutput.Value)
                    Debug.Log("Casualheim | Location.IsInside location is not in the dictrionary !!!");

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
                        Debug.Log("Casualheim | creaturespawner [" + cs.GetHashCode() + "] says char still exists for zloc [" + zloc_hash + "] !");

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
                        Debug.Log("Casualheim | found char " + character.m_name.ToLower() + " that is still alive for zloc [" + zloc_hash + "] !");

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

            if (!Util.check_caller("DMD<ZoneSystem::SpawnLocation>"))
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

            if (Util.check_caller("DMD<CreatureSpawner::Spawn>"))
                from_cs = true;
            else if (Util.check_caller("DMD<CharacterDrop::OnDeath>"))
                from_cd = true;

            if (!from_cs && !from_cd)
                return;

            if (__instance.m_nview == null || !__instance.m_nview.IsOwner())
                return;

            ZDO zdo = __instance.m_nview.GetZDO();
            if (zdo == null)
                return;

            if (ThisPlugin.DebugOutput.Value)
                Debug.Log("Casualheim | setting zloc_hash [" + (from_cs ? last_creature_spawner_hash : (from_cd ? last_character_drop_hash : -1)) + "] for char :: " + __instance.m_name.ToLower());

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

            ZDO zdo;
            int zloc_hash;
            AssignLocHashDeterministic(ref __instance, out zdo, out zloc_hash);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SpawnArea), "Awake")]
        public static void SpawnAreaAwakePatch(ref SpawnArea __instance) {
            if (!ThisPlugin.PluginEnabled.Value || !ThisPlugin.AllowClearedBuilding.Value)
                return;

            ZDO zdo;
            int zloc_hash;
            AssignLocHashDeterministic(ref __instance, out zdo, out zloc_hash);
        }

        // Propogation mechanism for zloc_hash into spawners inside the group (probably not needed)
        // most likely not needed
        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(CreatureSpawner), "UpdateSpawner")]
        //public static void CreatureSpawnerUpdateSpawnerPatch(ref CreatureSpawner __instance) {
        //    if (!ThisPlugin.PluginEnabled.Value || !ThisPlugin.AllowClearedBuilding.Value || __instance.m_nview == null || __instance.m_spawnGroup == null)
        //        return;
        //
        //    ZDO zdo = __instance.m_nview.GetZDO();
        //    if (zdo == null)
        //        return;
        //
        //    int zloc_hash = zdo.GetInt("zloc_hash");
        //    if (zloc_hash == 0)
        //        return;
        //
        //    Location loc;
        //    if (GetLocation(ref __instance, out loc))
        //        return;
        //
        //    List<CreatureSpawner> group_spawners = new List<CreatureSpawner>();
        //    foreach (CreatureSpawner creatureSpawner in __instance.m_spawnGroup)
        //        group_spawners.Add(creatureSpawner);
        //
        //    for (int i = 0; i < group_spawners.Count; i++) {
        //        try {
        //            CreatureSpawner cs = group_spawners[i];
        //            if (cs == null || cs.m_lastSpawnID == null)
        //                continue;
        //
        //            if (cs.m_lastSpawnID.IsNone() || (cs.CanRespawnNow(cs.m_lastSpawnID) && !cs.SpawnedCreatureStillExists(cs.m_lastSpawnID))) {
        //                if (cs.m_nview == null || !cs.m_nview.IsOwner())
        //                    return;
        //
        //                ZDO zdo_group = cs.m_nview.GetZDO();
        //                if (zdo_group == null)
        //                    continue;
        //
        //                int zloc_hash_group = zdo_group.GetInt("zloc_hash");
        //                if (zloc_hash_group != 0)
        //                    continue;
        //
        //                Location group_loc;
        //                if (GetLocation(ref cs, out group_loc))
        //                    continue;
        //
        //                if (ThisPlugin.DebugOutput.Value)
        //                    UnityEngine.Debug.Log("Casualheim | !!!   !!!   no ZDO for CreatureSpawner   !!!   !!!");
        //
        //                if (group_loc.GetHashCode() == loc.GetHashCode())
        //                    zdo_group.Set("zloc_hash", zloc_hash);
        //            }
        //        } catch (Exception e) {
        //            continue;
        //        }
        //    }
        //}

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CreatureSpawner), "Spawn")]
        public static void CreatureSpawnerSpawnPatch(ref CreatureSpawner __instance) {
            if (!ThisPlugin.PluginEnabled.Value || !ThisPlugin.AllowClearedBuilding.Value)
                return;

            BeforeSpawnerUnitSpawn(ref __instance);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SpawnArea), "SpawnOne")]
        public static void SpawnAreaSpawnOnePatch(ref SpawnArea __instance) {
            if (!ThisPlugin.PluginEnabled.Value || !ThisPlugin.AllowClearedBuilding.Value)
                return;

            BeforeSpawnerUnitSpawn(ref __instance);
        }
    }
}
