using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace Casualheim {
    [BepInPlugin("Casualheim", "Casualheim", "0.1.0")]
    [BepInProcess("valheim.exe")]
    public class ThisPlugin : BaseUnityPlugin {
        public void Awake() {
            thisInstance = this;
            LoadSettings();

            if (PluginEnabled.Value)
                new Harmony("Casualheim").PatchAll();

            if (DebugOutput.Value)
                DumpConfiguration();
        }

        public static ThisPlugin instance {
            get {
                return thisInstance;
            }
        }

        public void OnDestroy() {
            new Harmony("Casualheim").UnpatchSelf();
        }

        public static void LoadSettings() {
            PluginEnabled                  = instance.Config.Bind("General",   "Enabled",                              true,  "Enable/disable this pulgin.");
            DebugOutput                    = instance.Config.Bind("General",   "Debug",                                false, "Enable/disable debug logging.");
            PercentAttackMovement          = instance.Config.Bind("Attacks",   "PercentAttackMovementSpeed",           20,    "Percent of normal movement speed that remains while attacking.");
            PercentAttackRotation          = instance.Config.Bind("Attacks",   "PercentAttackRotationSpeed",           20,    "Percent of normal rotation speed that remains while attacking.");
            PercentCharredTwitcher         = instance.Config.Bind("MobHealth", "PercentCharredTwitcherHealth",         100,   "Percent of normal health that Charred Twitcher will have.");
            PercentCharredArcher           = instance.Config.Bind("MobHealth", "PercentCharredArcherHealth",           100,   "Percent of normal health that Charred Marksman will have.");
            PercentCharredMelee            = instance.Config.Bind("MobHealth", "PercentCharredMeleeHealth",            66,    "Percent of normal health that Charred Warrior will have.");
            PercentCharredMage             = instance.Config.Bind("MobHealth", "PercentCharredMageHealth",             66, "Percent of normal health that Charred Warlock will have.");
            PercentCharredTwitcherSummoned = instance.Config.Bind("MobHealth", "PercentCharredTwitcherSummonedHealth", 100,   "Percent of normal health that Summoned Twitcher will have.");
            PercentFallenValkyrie          = instance.Config.Bind("MobHealth", "PercentFallenValkyrieHealth",          50,    "Percent of normal health that Fallen Valkyrie will have.");
            PercentBonemawSerpent          = instance.Config.Bind("MobHealth", "PercentBonemawSerpentHealth",          50,    "Percent of normal health that Bonemaw Serpent will have.");
            PercentMorgen                  = instance.Config.Bind("MobHealth", "PercentMorgenHealth",                  50,    "Percent of normal health that Morgen will have.");
            PercentVolture                 = instance.Config.Bind("MobHealth", "PercentVoltureHealth",                 100,   "Percent of normal health that Volture will have.");
            PercentPieceCharredBalista     = instance.Config.Bind("MobHealth", "PercentPieceCharredBalistaHealth",     100,   "Percent of normal health that Skugg will have.");
            PercentBlobLava                = instance.Config.Bind("MobHealth", "PercentBlobLavaHealth",                100,   "Percent of normal health that Lava Blob will have.");
            PercentDvergerAshlands         = instance.Config.Bind("MobHealth", "PercentDvergerAshlandsHealth",         100,   "Percent of normal health that Ashlands Dvergr will have.");
        }

        public static void DumpConfiguration() {
            Debug.Log("------------ Casualheim CFG START ------------");

            if (ZNet.instance != null)
                Debug.Log("Casualheim,IsServer," + ZNet.instance.IsServer().ToString());

            Debug.Log("Casualheim.GetAllPlayers,"              + Player.GetAllPlayers().Count.ToString());
            Debug.Log("Casualheim.PercentAttackMovement," + PercentAttackMovement.Value);
            Debug.Log("Casualheim.PercentAttackRotation," + PercentAttackRotation.Value);
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

            Debug.Log("------------- Casualheim CFG END -------------");
        }

        public const string PluginName = "Casualheim";
        public const string PluginAuthor = "k-Knight";
        public const string PluginVersion = "0.1.0";
        public const string PluginGUID = "Casualheim";

        public static ConfigEntry<bool> PluginEnabled;
        public static ConfigEntry<bool> DebugOutput;
        public static ConfigEntry<int> PercentAttackMovement;
        public static ConfigEntry<int> PercentAttackRotation;
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
        private static ThisPlugin thisInstance;
    }

    
}