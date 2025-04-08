using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Casualheim {
    [BepInPlugin("Casualheim", "Casualheim", "1.1.0")]
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
                harmony_instance.PatchAll(typeof(patches.AttackCancelPatch));
                harmony_instance.PatchAll(typeof(patches.AttackSlowdownPatch));
                harmony_instance.PatchAll(typeof(patches.DeathPenaltyPatch));
                harmony_instance.PatchAll(typeof(patches.EnemyLevelChancePatch));
                harmony_instance.PatchAll(typeof(patches.MaxHealthPatch));
                harmony_instance.PatchAll(typeof(patches.RegenPatch));
                harmony_instance.PatchAll(typeof(patches.SkillCurvePatch));
                harmony_instance.PatchAll(typeof(patches.MiningChoppingPatch));

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

            if (DebugOutput.Value)
                DumpConfiguration();
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

        public static void MaxHealthDictsInit() {
            MaxHealthPercentDict.Add("charred_twitcher", new WeakReference<ConfigEntry<int>>(PercentCharredTwitcher));
            MaxHealthPercentDict.Add("charred_archer", new WeakReference<ConfigEntry<int>>(PercentCharredArcher));
            MaxHealthPercentDict.Add("charred_melee", new WeakReference<ConfigEntry<int>>(PercentCharredMelee));
            MaxHealthPercentDict.Add("charred_mage", new WeakReference<ConfigEntry<int>>(PercentCharredMage));
            MaxHealthPercentDict.Add("charred_twitcher_summoned", new WeakReference<ConfigEntry<int>>(PercentCharredTwitcherSummoned));
            MaxHealthPercentDict.Add("fallenvalkyrie", new WeakReference<ConfigEntry<int>>(PercentFallenValkyrie));
            MaxHealthPercentDict.Add("bonemawserpent", new WeakReference<ConfigEntry<int>>(PercentBonemawSerpent));
            MaxHealthPercentDict.Add("morgen", new WeakReference<ConfigEntry<int>>(PercentMorgen));
            MaxHealthPercentDict.Add("volture", new WeakReference<ConfigEntry<int>>(PercentVolture));
            MaxHealthPercentDict.Add("piece_charred_balista", new WeakReference<ConfigEntry<int>>(PercentPieceCharredBalista));
            MaxHealthPercentDict.Add("bloblava", new WeakReference<ConfigEntry<int>>(PercentBlobLava));
            MaxHealthPercentDict.Add("dvergerashlands", new WeakReference<ConfigEntry<int>>(PercentDvergerAshlands));

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
            NumberOfPlayersMax = instance.Config.Bind("General", "NumberOfPlayersMax", 4, "Maximum number of active players to modify boss health and regen.");
            EnemyLevelChanceMultiplier = instance.Config.Bind("General", "EnemyLevelChanceMultiplier", 3f, "My how much the chance of leveling up enemy (stars) is multiplied.");
            AllowClearedBuilding = instance.Config.Bind("General", "AllowClearedDungeonBuilding", true, "Allow building in dungeons/locations when all enemies are dead. May require a new world (kinda).");
            ChopMineDamageMultiplier = instance.Config.Bind("General", "ChopMineDamageMultiplier", 2f, "Multiplies the damage of chopping and mining (0 for disable).");

            EasierSkillCurveEnabled = instance.Config.Bind("Skills", "EasierSkillCurveEnabled", true, "Whether to enable easier skill curve.");
            RequiredExpMultiplier = instance.Config.Bind("Skills", "RequiredExpMultiplier", 1f, "This changes the speed of arithmetic progression in required experience to reach next skill level.");
            DeathPenaltyMultiplier = instance.Config.Bind("Skills", "DeathPenaltyMultiplier", 0f, "This changes the amount of skill loss by multiplying it with this value.");
            EnableSkillLevelProgressLoss = instance.Config.Bind("Skills", "EnableSkillLevelProgressLoss", false, "Whether to reset the accumulated experience on the current skill level.");

            PercentAttackMovement = instance.Config.Bind("Attacks", "PercentAttackMovementSpeed", 20, "Percent of normal movement speed that remains while attacking.");
            PercentAttackRotation = instance.Config.Bind("Attacks", "PercentAttackRotationSpeed", 20, "Percent of normal rotation speed that remains while attacking.");
            PreventDodgeSpamming = instance.Config.Bind("Attacks", "PreventDodgeSpamming", false, "Prevent dodge spamming if player continiously holds down the dodge key.");

            PercentCharredTwitcher = instance.Config.Bind("MobHealth", "PercentCharredTwitcherHealth", 100, "Percent of normal health that Charred Twitcher will have.");
            PercentCharredArcher = instance.Config.Bind("MobHealth", "PercentCharredArcherHealth", 100, "Percent of normal health that Charred Marksman will have.");
            PercentCharredMelee = instance.Config.Bind("MobHealth", "PercentCharredMeleeHealth", 66, "Percent of normal health that Charred Warrior will have.");
            PercentCharredMage = instance.Config.Bind("MobHealth", "PercentCharredMageHealth", 66, "Percent of normal health that Charred Warlock will have.");
            PercentCharredTwitcherSummoned = instance.Config.Bind("MobHealth", "PercentCharredTwitcherSummonedHealth", 100, "Percent of normal health that Summoned Twitcher will have.");
            PercentFallenValkyrie = instance.Config.Bind("MobHealth", "PercentFallenValkyrieHealth", 50, "Percent of normal health that Fallen Valkyrie will have.");
            PercentBonemawSerpent = instance.Config.Bind("MobHealth", "PercentBonemawSerpentHealth", 50, "Percent of normal health that Bonemaw Serpent will have.");
            PercentMorgen = instance.Config.Bind("MobHealth", "PercentMorgenHealth", 50, "Percent of normal health that Morgen will have.");
            PercentVolture = instance.Config.Bind("MobHealth", "PercentVoltureHealth", 100, "Percent of normal health that Volture will have.");
            PercentPieceCharredBalista = instance.Config.Bind("MobHealth", "PercentPieceCharredBalistaHealth", 100, "Percent of normal health that Skugg will have.");
            PercentBlobLava = instance.Config.Bind("MobHealth", "PercentBlobLavaHealth", 100, "Percent of normal health that Lava Blob will have.");
            PercentDvergerAshlands = instance.Config.Bind("MobHealth", "PercentDvergerAshlandsHealth", 100, "Percent of normal health that Ashlands Dvergr will have.");

            PercentRegenEikthyr = instance.Config.Bind("Boss Health", "PercentRegenEikthyr", 0, "Percent of normal Eikthyr health regen.");
            PercentRegenElder = instance.Config.Bind("Boss Health", "PercentRegenElder", 0, "Percent of normal Elder health regen.");
            PercentRegenBonemass = instance.Config.Bind("Boss Health", "PercentRegenBonemass", 0, "Percent of normal Bonemass health regen.");
            PercentRegenModer = instance.Config.Bind("Boss Health", "PercentRegenModer", 0, "Percent of normal Moder health regen.");
            PercentRegenYagluth = instance.Config.Bind("Boss Health", "PercentRegenYagluth", 0, "Percent of normal Yagluth health regen.");
            PercentRegenQueen = instance.Config.Bind("Boss Health", "PercentRegenQueen", 0, "Percent of normal Queen health regen.");
            PercentRegenFader = instance.Config.Bind("Boss Health", "PercentRegenFader", 0, "Percent of normal Fader health regen.");

            PercentHealthEikthyr = instance.Config.Bind("Boss Health", "PercentHealthEikthyr", 150, "Percent of normal Eikthyr max health.");
            PercentHealthElder = instance.Config.Bind("Boss Health", "PercentHealthElder", 100, "Percent of normal Elder max health.");
            PercentHealthBonemass = instance.Config.Bind("Boss Health", "PercentHealthBonemass", 100, "Percent of normal Bonemass max health.");
            PercentHealthModer = instance.Config.Bind("Boss Health", "PercentHealthModer", 100, "Percent of normal Moder max health.");
            PercentHealthYagluth = instance.Config.Bind("Boss Health", "PercentHealthYagluth", 100, "Percent of normal Yagluth max health.");
            PercentHealthQueen = instance.Config.Bind("Boss Health", "PercentHealthQueen", 100, "Percent of normal Queen max health.");
            PercentHealthFader = instance.Config.Bind("Boss Health", "PercentHealthFader", 60, "Percent of normal Fader max health.");

            EnableLeveling = instance.Config.Bind("Leveling", "Enable Leveling System", true, "Enables/Disables the leveling system");
            HealthBoostMultiplier = instance.Config.Bind("Leveling", "Health boost strength multiplier", 1f, "Changes the strength of the health boost (0 for disable).");
            HealthRegenBoostMultiplier = instance.Config.Bind("Leveling", "Health regen boost strength multiplier", 1f, "Changes the strength of the health regeneration boost (0 for disable).");
            StaminaBoostMultiplier = instance.Config.Bind("Leveling", "Stamina boost strength multiplier", 1f, "Changes the strength of the stamina boost (0 for disable).");
            StaminaRegenBoostMultiplier = instance.Config.Bind("Leveling", "Stamina regen boost strength multiplier", 1f, "Changes the strength of the stamina regeneration boost (0 for disable).");
            SpeedBoostMultiplier = instance.Config.Bind("Leveling", "Speed boost strength multiplier", 1f, "Changes the strength of the movement speed boost (0 for disable).");
            JumpHeightMultiplier = instance.Config.Bind("Leveling", "Jump height boost strength multiplier", 1f, "Changes the strength of the jump height boost (0 for disable).");
            FallWindowMultiplier = instance.Config.Bind("Leveling", "Allowed fall window increase multiplier", 1f, "Changes the strength of allowed fall window increase where player does not receive fall damage (0 for disable).");
        }

        public static void DumpConfiguration() {
            Debug.Log("------------ Casualheim CFG START ------------");

            if (ZNet.instance != null)
                Debug.Log("Casualheim,IsServer," + ZNet.instance.IsServer());

            Debug.Log("Casualheim.GetAllPlayers," + Player.GetAllPlayers().Count);
            Debug.Log("Casualheim.NumberOfPlayersMax," + NumberOfPlayersMax.Value);
            Debug.Log("Casualheim.AllowClearedBuilding," + AllowClearedBuilding.Value);
            Debug.Log("Casualheim.ChopMineDamageMultiplier," + ChopMineDamageMultiplier.Value);
            Debug.Log("Casualheim.EasierSkillCurveEnabled," + EasierSkillCurveEnabled.Value);
            Debug.Log("Casualheim.RequiredExpMultiplier," + RequiredExpMultiplier.Value);
            Debug.Log("Casualheim.DeathPenaltyMultiplier," + DeathPenaltyMultiplier.Value);
            Debug.Log("Casualheim.EnableSkillLevelProgressLoss," + EnableSkillLevelProgressLoss.Value);
            Debug.Log("Casualheim.PercentAttackMovement," + PercentAttackMovement.Value);
            Debug.Log("Casualheim.PercentAttackRotation," + PercentAttackRotation.Value);
            Debug.Log("Casualheim.PreventDodgeSpamming," + PreventDodgeSpamming.Value);
            Debug.Log("Casualheim.PercentCharredTwitcher," + PercentCharredTwitcher.Value);
            Debug.Log("Casualheim.PercentCharredArcher," + PercentCharredArcher.Value);
            Debug.Log("Casualheim.PercentCharredMelee," + PercentCharredMelee.Value);
            Debug.Log("Casualheim.PercentCharredMage," + PercentCharredMage.Value);
            Debug.Log("Casualheim.PercentCharredTwitcherSummoned," + PercentCharredTwitcherSummoned.Value);
            Debug.Log("Casualheim.PercentFallenValkyrie," + PercentFallenValkyrie.Value);
            Debug.Log("Casualheim.PercentBonemawSerpent," + PercentBonemawSerpent.Value);
            Debug.Log("Casualheim.PercentMorgen," + PercentMorgen.Value);
            Debug.Log("Casualheim.PercentVolture," + PercentVolture.Value);
            Debug.Log("Casualheim.PercentPieceCharredBalista," + PercentPieceCharredBalista.Value);
            Debug.Log("Casualheim.PercentBlobLava," + PercentBlobLava.Value);
            Debug.Log("Casualheim.PercentDvergerAshlands," + PercentDvergerAshlands.Value);
            Debug.Log("Casualheim.PercentRegenEikthyr," + PercentRegenEikthyr.Value);
            Debug.Log("Casualheim.PercentRegenElder," + PercentRegenElder.Value);
            Debug.Log("Casualheim.PercentRegenBonemass," + PercentRegenBonemass.Value);
            Debug.Log("Casualheim.PercentRegenModer," + PercentRegenModer.Value);
            Debug.Log("Casualheim.PercentRegenYagluth," + PercentRegenYagluth.Value);
            Debug.Log("Casualheim.PercentRegenQueen," + PercentRegenQueen.Value);
            Debug.Log("Casualheim.PercentRegenFader," + PercentRegenFader.Value);
            Debug.Log("Casualheim.PercentHealthEikthyr," + PercentHealthEikthyr.Value);
            Debug.Log("Casualheim.PercentHealthElder," + PercentHealthElder.Value);
            Debug.Log("Casualheim.PercentHealthBonemass," + PercentHealthBonemass.Value);
            Debug.Log("Casualheim.PercentHealthModer," + PercentHealthModer.Value);
            Debug.Log("Casualheim.PercentHealthYagluth," + PercentHealthYagluth.Value);
            Debug.Log("Casualheim.PercentHealthQueen," + PercentHealthQueen.Value);
            Debug.Log("Casualheim.PercentHealthFader," + PercentHealthFader.Value);
            Debug.Log("Casualheim.EnableLeveling," + EnableLeveling.Value);
            Debug.Log("Casualheim.HealthBoostMultiplier," + HealthBoostMultiplier.Value);
            Debug.Log("Casualheim.HealthRegenMultiplier," + HealthRegenBoostMultiplier.Value);
            Debug.Log("Casualheim.StaminaBoostMultiplier," + StaminaBoostMultiplier.Value);
            Debug.Log("Casualheim.StaminaRegenBoostMultiplier," + StaminaRegenBoostMultiplier.Value);
            Debug.Log("Casualheim.SpeedBoostMultiplier," + SpeedBoostMultiplier.Value);
            Debug.Log("Casualheim.JumpHeightMultiplier," + JumpHeightMultiplier.Value);
            Debug.Log("Casualheim.FallWindowMultiplier," + FallWindowMultiplier.Value);

            Debug.Log("------------- Casualheim CFG END -------------");
        }

        public const string PluginName = "Casualheim";
        public const string PluginAuthor = "k-Knight";
        public const string PluginVersion = "1.1.0";
        public const string PluginGUID = "Casualheim";

        public static ConfigEntry<bool> PluginEnabled;
        public static ConfigEntry<bool> DebugOutput;
        public static ConfigEntry<int> NumberOfPlayersMax;
        public static ConfigEntry<bool> AllowClearedBuilding;
        public static ConfigEntry<float> ChopMineDamageMultiplier;

        public static ConfigEntry<bool> EasierSkillCurveEnabled;
        public static ConfigEntry<float> RequiredExpMultiplier;
        public static ConfigEntry<float> DeathPenaltyMultiplier;
        public static ConfigEntry<bool> EnableSkillLevelProgressLoss;

        public static ConfigEntry<float> EnemyLevelChanceMultiplier;

        public static ConfigEntry<int> PercentAttackMovement;
        public static ConfigEntry<int> PercentAttackRotation;
        public static ConfigEntry<bool> PreventDodgeSpamming;

        public static ConfigEntry<int> PercentCharredTwitcher;
        public static ConfigEntry<int> PercentCharredArcher;
        public static ConfigEntry<int> PercentCharredMelee;
        public static ConfigEntry<int> PercentCharredMage;
        public static ConfigEntry<int> PercentCharredTwitcherSummoned;
        public static ConfigEntry<int> PercentFallenValkyrie;
        public static ConfigEntry<int> PercentBonemawSerpent;
        public static ConfigEntry<int> PercentMorgen;
        public static ConfigEntry<int> PercentVolture;
        public static ConfigEntry<int> PercentPieceCharredBalista;
        public static ConfigEntry<int> PercentBlobLava;
        public static ConfigEntry<int> PercentDvergerAshlands;

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
        public static ConfigEntry<float> SpeedBoostMultiplier;
        public static ConfigEntry<float> JumpHeightMultiplier;
        public static ConfigEntry<float> FallWindowMultiplier;


        public static Dictionary<string, WeakReference<ConfigEntry<int>>> MaxHealthPercentDict = new Dictionary<string, WeakReference<ConfigEntry<int>>>();
        public static Dictionary<string, WeakReference<ConfigEntry<int>>> HealthRegenPercentDict = new Dictionary<string, WeakReference<ConfigEntry<int>>>();
        private static ThisPlugin thisInstance;
    }


}