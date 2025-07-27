using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Casualheim {
    public class MaxHealthSetting {
        public string name;
        public ConfigEntry<int> percent;

        public MaxHealthSetting(string name) {
            this.name = name;
        }

        public static Dictionary<string, MaxHealthSetting> Settings = new Dictionary<string, MaxHealthSetting>();

        public static void CreateMapping(string id, string name, int default_percent) {
            MaxHealthSetting setting = new MaxHealthSetting(name);
            setting.percent = ThisPlugin.instance.Config.Bind("Mob Health", "Percent " + id + " health", default_percent, "Percent of normal health that " + name + " will have.");
            Settings.Add(id.ToLower(), setting);
        }
    };

    [BepInPlugin("Casualheim", "Casualheim", "1.2.6")]
    [BepInProcess("valheim.exe")]
    [BepInDependency("MK_BetterUI", BepInDependency.DependencyFlags.SoftDependency)]
    public class ThisPlugin : BaseUnityPlugin {
        public static Harmony harmony_instance = null;

        public static bool ModIsLoaded(string GUID) {
            foreach (var plugin in Chainloader.PluginInfos)
                if (plugin.Value.Metadata.GUID.Equals(GUID))
                    return true;

            return false;
        }

        public void Awake() {
            thisInstance = this;
            MaxHealthDictsInit();
            LoadSettings();

            if (PluginEnabled.Value) {
                harmony_instance = new Harmony("Casualheim");

                // gameplay patches
                harmony_instance.PatchAll(typeof(patches.AllowClearedBuildingPatch));
                harmony_instance.PatchAll(typeof(patches.AttackSlowdownPatch));
                harmony_instance.PatchAll(typeof(patches.DeathPenaltyPatch));
                harmony_instance.PatchAll(typeof(patches.EnemyLevelChancePatch));
                harmony_instance.PatchAll(typeof(patches.MaxHealthPatch));
                harmony_instance.PatchAll(typeof(patches.RegenPatch));
                harmony_instance.PatchAll(typeof(patches.SkillCurvePatch));
                harmony_instance.PatchAll(typeof(patches.MiningChoppingPatch));
                harmony_instance.PatchAll(typeof(patches.ShipHelpPatch));
                harmony_instance.PatchAll(typeof(patches.TrophyDropPatch));

                // attack cancel patches
                harmony_instance.PatchAll(typeof(attack_cancel.AttackCancelPatch));
                harmony_instance.PatchAll(typeof(attack_cancel.AttackPreventLogicPatch));

                // leveling patches
                harmony_instance.PatchAll(typeof(leveling.LevelPatch));
                harmony_instance.PatchAll(typeof(leveling.StatModificationPatch));
                if (ModIsLoaded("MK_BetterUI")) {
                    if (DebugOutput.Value)
                        Debug.Log("Casualheim | found optional BetterUI dependency, patching ...");

                    harmony_instance.PatchAll(typeof(leveling.DisableBetterUIXPPatch));
                }

                // gui things
                harmony_instance.PatchAll(typeof(gui.LevelIndicatorPatch));
            }
        }

        public static ThisPlugin instance {
            get {
                return thisInstance;
            }
        }

        public void OnDestroy() {
            if (harmony_instance != null)
                harmony_instance.UnpatchSelf();
        }

        public static void CreateMaxHealthUnitMapping(string id, string display_name) {
        }

        public static void MaxHealthDictsInit() {
            MaxHealthSetting.CreateMapping("Goblin", "Fuling", 100);
            MaxHealthSetting.CreateMapping("GoblinBrute", "Fuling berserker", 75);
            MaxHealthSetting.CreateMapping("GoblinShaman", "Fuling shaman", 100);
            MaxHealthSetting.CreateMapping("Lox", "Lox", 75);
            MaxHealthSetting.CreateMapping("Deathsquito", "Deathsquito", 100);
            MaxHealthSetting.CreateMapping("BlobTar", "Growth", 100);

            MaxHealthSetting.CreateMapping("Seeker", "Seeker", 100);
            MaxHealthSetting.CreateMapping("SeekerBrood", "Seeker brood", 100);
            MaxHealthSetting.CreateMapping("SeekerBrute", "Seeker soldier", 67);
            MaxHealthSetting.CreateMapping("Gjall", "Gjall", 67);
            MaxHealthSetting.CreateMapping("Tick", "Tick", 100);
            MaxHealthSetting.CreateMapping("Dverger", "Dvergr rogue", 100);
            MaxHealthSetting.CreateMapping("DvergerMage", "Dvergr mage", 100);
            MaxHealthSetting.CreateMapping("DvergerMageFire", "Fire Dvergr mage", 100);
            MaxHealthSetting.CreateMapping("DvergerMageIce", "Ice Dvergr mage", 100);
            MaxHealthSetting.CreateMapping("DvergerMageSupport", "Support Dvergr mage", 100);

            MaxHealthSetting.CreateMapping("FallenValkyrie", "Fallen Valkyrie", 67);
            MaxHealthSetting.CreateMapping("Morgen", "Morgen", 50);
            MaxHealthSetting.CreateMapping("BonemawSerpent", "Bonemaw Serpent", 67);
            MaxHealthSetting.CreateMapping("Asksvin", "Asksvin", 100);
            MaxHealthSetting.CreateMapping("Volture", "Volture", 100);
            MaxHealthSetting.CreateMapping("piece_Charred_Balista", "Skugg", 100);
            MaxHealthSetting.CreateMapping("BlobLava", "Skugg", 100);
            MaxHealthSetting.CreateMapping("Charred_Melee_Dyrnwyn", "Lord Reto", 100);
            MaxHealthSetting.CreateMapping("DvergerAshlands", "Ashlands Dvergr", 100);
            MaxHealthSetting.CreateMapping("Charred_Melee", "Charred Warrior", 67);
            MaxHealthSetting.CreateMapping("Charred_Archer", "Charred Marksman", 100);
            MaxHealthSetting.CreateMapping("Charred_Mage", "Charred Warlock", 67);
            MaxHealthSetting.CreateMapping("Charred_Twitcher", "Charred Twitcher", 85);

            MaxHealthPercentDict.Add("eikthyr", new WeakReference<ConfigEntry<int>>(PercentHealthEikthyr));
            MaxHealthPercentDict.Add("gdking", new WeakReference<ConfigEntry<int>>(PercentHealthElder));
            MaxHealthPercentDict.Add("bonemass", new WeakReference<ConfigEntry<int>>(PercentHealthBonemass));
            MaxHealthPercentDict.Add("dragon", new WeakReference<ConfigEntry<int>>(PercentHealthModer));
            MaxHealthPercentDict.Add("goblinking", new WeakReference<ConfigEntry<int>>(PercentHealthYagluth));
            MaxHealthPercentDict.Add("seekerqueen", new WeakReference<ConfigEntry<int>>(PercentHealthQueen));
            MaxHealthPercentDict.Add("fader", new WeakReference<ConfigEntry<int>>(PercentHealthFader));
            
            HealthRegenPercentDict.Add("eikthyr", new WeakReference<ConfigEntry<int>>(PercentRegenEikthyr));
            HealthRegenPercentDict.Add("gdking", new WeakReference<ConfigEntry<int>>(PercentRegenElder));
            HealthRegenPercentDict.Add("bonemass", new WeakReference<ConfigEntry<int>>(PercentRegenBonemass));
            HealthRegenPercentDict.Add("dragon", new WeakReference<ConfigEntry<int>>(PercentRegenModer));
            HealthRegenPercentDict.Add("goblinking", new WeakReference<ConfigEntry<int>>(PercentRegenYagluth));
            HealthRegenPercentDict.Add("seekerqueen", new WeakReference<ConfigEntry<int>>(PercentRegenQueen));
            HealthRegenPercentDict.Add("fader", new WeakReference<ConfigEntry<int>>(PercentRegenFader));
        }

        public static void LoadSettings() {
            PluginEnabled = instance.Config.Bind("General", "Enabled", true, "Enable/disable this pulgin.");
            DebugOutput = instance.Config.Bind("General", "Debug", false, "Enable/disable debug logging.");
            NumberOfPlayersMax = instance.Config.Bind("General", "Number of players max", 4, "Maximum number of active players to modify boss health and regen.");
            EnemyLevelChanceMultiplier = instance.Config.Bind("General", "Enemy level chance multiplier", 3f, "My how much the chance of leveling up enemy (stars) is multiplied (1.0 for default values).");
            AllowClearedBuilding = instance.Config.Bind("General", "Allow cleared dungeon building", true, "Allow building in dungeons/locations when all enemies are dead. May require a new world (kinda).");
            ChopMineDamageMultiplier = instance.Config.Bind("General", "Multiplier for chop/mine damage", 2f, "Multiplies the damage of chopping and mining (0 for disable).");
            TrophyDropChanceMult = instance.Config.Bind("General", "Trophy drop chance multiplier", 2f, "Multiplies the drop chance for the trophies (1.0 for no change).");
            TrophyLevelDropChanceMult = instance.Config.Bind("General", "Trophy drop chance multiplier per level", 2f, "Multiplies the drop chance for the trophies further for every creature star (1.0 for no change).");

            EasierSkillCurveEnabled = instance.Config.Bind("Skills", "Enable easier skill curve", true, "Enables/Disables easier skill curve.");
            RequiredExpMultiplier = instance.Config.Bind("Skills", "Required exp multiplier", 1f, "This changes the speed of arithmetic progression in required experience to reach next skill level.");
            EnableDeathPenaltyMod = instance.Config.Bind("Skills", "Enable death penalty changes", true, "Enables/Disables modifications of death penalty.");
            DeathPenaltyMultiplier = instance.Config.Bind("Skills", "Death penalty multiplier", 0f, "This changes the amount of skill loss by multiplying it with this value.");
            EnableSkillLevelProgressLoss = instance.Config.Bind("Skills", "Enable skill level progress loss", false, "Whether to reset the accumulated experience on the current skill level.");

            EnableAttackMod = instance.Config.Bind("Attacks", "Enable attack changes", true, "Enables/Disables attack cancellation and other related things.");
            PercentAttackMovement = instance.Config.Bind("Attacks", "Percent attack movement speed", 20, "Percent of normal movement speed that remains while attacking (20% is game's default).");
            PercentAttackRotation = instance.Config.Bind("Attacks", "Percent attack rotation speed", 20, "Percent of normal rotation speed that remains while attacking (20% is game's default).");
            PreventDodgeSpamming = instance.Config.Bind("Attacks", "Prevent dodge spamming", false, "Prevent dodge spamming if player continiously holds down the dodge key.");

            EnableEnemyHealthMod = instance.Config.Bind("Mob Health", "Enable enemy health change", true, "Enables/Disables the change of enemies' max health.");

            EnableBossHealthRegenMod = instance.Config.Bind("Boss Health", "Enable boss health change", true, "Enables/Disables the change of boss max health and regen.");
            PercentRegenEikthyr = instance.Config.Bind("Boss Health", "Percent regen Eikthyr", 0, "Percent of normal Eikthyr health regen.");
            PercentRegenElder = instance.Config.Bind("Boss Health", "Percent regen Elder", 0, "Percent of normal Elder health regen.");
            PercentRegenBonemass = instance.Config.Bind("Boss Health", "Percent regen Bonemass", 0, "Percent of normal Bonemass health regen.");
            PercentRegenModer = instance.Config.Bind("Boss Health", "Percent regen Moder", 0, "Percent of normal Moder health regen.");
            PercentRegenYagluth = instance.Config.Bind("Boss Health", "Percent regen Yagluth", 0, "Percent of normal Yagluth health regen.");
            PercentRegenQueen = instance.Config.Bind("Boss Health", "Percent regen Queen", 0, "Percent of normal Queen health regen.");
            PercentRegenFader = instance.Config.Bind("Boss Health", "Percent regen Fader", 0, "Percent of normal Fader health regen.");

            PercentHealthEikthyr = instance.Config.Bind("Boss Health", "Percent health Eikthyr", 150, "Percent of normal Eikthyr max health.");
            PercentHealthElder = instance.Config.Bind("Boss Health", "Percent health Elder", 100, "Percent of normal Elder max health.");
            PercentHealthBonemass = instance.Config.Bind("Boss Health", "Percent health Bonemass", 100, "Percent of normal Bonemass max health.");
            PercentHealthModer = instance.Config.Bind("Boss Health", "Percent health Moder", 100, "Percent of normal Moder max health.");
            PercentHealthYagluth = instance.Config.Bind("Boss Health", "Percent health Yagluth", 100, "Percent of normal Yagluth max health.");
            PercentHealthQueen = instance.Config.Bind("Boss Health", "Percent health Queen", 100, "Percent of normal Queen max health.");
            PercentHealthFader = instance.Config.Bind("Boss Health", "Percent health Fader", 60, "Percent of normal Fader max health.");

            EnableLeveling = instance.Config.Bind("Leveling", "Enable leveling system", true, "Enables/Disables the leveling system");
            HealthBoostMultiplier = instance.Config.Bind("Leveling", "Health boost strength multiplier", 1f, "Changes the strength of the health boost (0 for disable).");
            HealthRegenBoostMultiplier = instance.Config.Bind("Leveling", "Health regen boost strength multiplier", 1f, "Changes the strength of the health regeneration boost (0 for disable).");
            StaminaBoostMultiplier = instance.Config.Bind("Leveling", "Stamina boost strength multiplier", 1f, "Changes the strength of the stamina boost (0 for disable).");
            StaminaRegenBoostMultiplier = instance.Config.Bind("Leveling", "Stamina regen boost strength multiplier", 1f, "Changes the strength of the stamina regeneration boost (0 for disable).");
            EitrBoostMultiplier = instance.Config.Bind("Leveling", "Eitr boost strength multiplier", 1f, "Changes the strength of the eitr boost (0 for disable).");
            EitrRegenBoostMultiplier = instance.Config.Bind("Leveling", "Eitr regen boost strength multiplier", 1f, "Changes the strength of the eitr regeneration boost (0 for disable).");
            SpeedBoostMultiplier = instance.Config.Bind("Leveling", "Speed boost strength multiplier", 1f, "Changes the strength of the movement speed boost (0 for disable).");
            JumpHeightMultiplier = instance.Config.Bind("Leveling", "Jump height boost strength multiplier", 1f, "Changes the strength of the jump height boost (0 for disable).");
            FallWindowMultiplier = instance.Config.Bind("Leveling", "Allowed fall window increase multiplier", 1f, "Changes the strength of allowed fall window increase where player does not receive fall damage (0 for disable).");

            EnableShipHelp = instance.Config.Bind("Ship Help", "Enable tweaks for easier sailing", true, "Enables/Disables the sailing assistance system");
            ShipStabilization = instance.Config.Bind("Ship Help", "Ship stabilization assistance multiplier", 6f, "Changes the amount of ship stabilization (1.0 is vanilla).");
            SailForceMult = instance.Config.Bind("Ship Help", "Ship sail force multiplier", 6f, "Changes the amount of force sails generate (1.0 is vanilla).");
            RudderForceMult = instance.Config.Bind("Ship Help", "Ship rudder force multiplier", 4f, "Changes the amount of force rudder generates (0 for disable).");
        }

        public const string PluginName = "Casualheim";
        public const string PluginAuthor = "k-Knight";
        public const string PluginVersion = "1.1.2";
        public const string PluginGUID = "Casualheim";

        public static ConfigEntry<bool> PluginEnabled;
        public static ConfigEntry<bool> DebugOutput;
        public static ConfigEntry<int> NumberOfPlayersMax;
        public static ConfigEntry<bool> AllowClearedBuilding;
        public static ConfigEntry<float> ChopMineDamageMultiplier;
        public static ConfigEntry<float> TrophyDropChanceMult;
        public static ConfigEntry<float> TrophyLevelDropChanceMult;

        public static ConfigEntry<bool> EasierSkillCurveEnabled;
        public static ConfigEntry<float> RequiredExpMultiplier;
        public static ConfigEntry<bool> EnableDeathPenaltyMod;
        public static ConfigEntry<float> DeathPenaltyMultiplier;
        public static ConfigEntry<bool> EnableSkillLevelProgressLoss;

        public static ConfigEntry<float> EnemyLevelChanceMultiplier;

        public static ConfigEntry<bool> EnableAttackMod;
        public static ConfigEntry<int> PercentAttackMovement;
        public static ConfigEntry<int> PercentAttackRotation;
        public static ConfigEntry<bool> PreventDodgeSpamming;

        public static ConfigEntry<bool> EnableEnemyHealthMod;

        public static ConfigEntry<bool> EnableBossHealthRegenMod;
        public static ConfigEntry<int> RegenerationMultiplier;
        public static ConfigEntry<int> PercentRegenEikthyr;
        public static ConfigEntry<int> PercentRegenElder;
        public static ConfigEntry<int> PercentRegenBonemass;
        public static ConfigEntry<int> PercentRegenModer;
        public static ConfigEntry<int> PercentRegenYagluth;
        public static ConfigEntry<int> PercentRegenQueen;
        public static ConfigEntry<int> PercentRegenFader;
        public static ConfigEntry<int> PercentHealthEikthyr;
        public static ConfigEntry<int> PercentHealthElder;
        public static ConfigEntry<int> PercentHealthBonemass;
        public static ConfigEntry<int> PercentHealthModer;
        public static ConfigEntry<int> PercentHealthYagluth;
        public static ConfigEntry<int> PercentHealthQueen;
        public static ConfigEntry<int> PercentHealthFader;

        public static ConfigEntry<bool> EnableLeveling;
        public static ConfigEntry<float> HealthBoostMultiplier;
        public static ConfigEntry<float> HealthRegenBoostMultiplier;
        public static ConfigEntry<float> StaminaBoostMultiplier;
        public static ConfigEntry<float> StaminaRegenBoostMultiplier;
        public static ConfigEntry<float> EitrBoostMultiplier;
        public static ConfigEntry<float> EitrRegenBoostMultiplier;
        public static ConfigEntry<float> SpeedBoostMultiplier;
        public static ConfigEntry<float> JumpHeightMultiplier;
        public static ConfigEntry<float> FallWindowMultiplier;

        public static ConfigEntry<bool> EnableShipHelp;
        public static ConfigEntry<float> ShipStabilization;
        public static ConfigEntry<float> RudderForceMult;
        public static ConfigEntry<float> SailForceMult;

        public static Dictionary<string, WeakReference<ConfigEntry<int>>> MaxHealthPercentDict = new Dictionary<string, WeakReference<ConfigEntry<int>>>();
        public static Dictionary<string, WeakReference<ConfigEntry<int>>> HealthRegenPercentDict = new Dictionary<string, WeakReference<ConfigEntry<int>>>();
        private static ThisPlugin thisInstance;
    }


}