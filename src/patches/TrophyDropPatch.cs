using HarmonyLib;
using System;
using UnityEngine;

namespace Casualheim.patches {
    [HarmonyPatch]
    public class TrophyDropPatch {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CharacterDrop), "GenerateDropList")]
        public static void GenerateDropListPatch(ref CharacterDrop __instance) {
            foreach (CharacterDrop.Drop drop in __instance.m_drops) {
                ItemDrop item;
                ItemDrop.ItemData.SharedData itemData;

                if (!drop.m_prefab.TryGetComponent<ItemDrop>(out item))
                    continue;

                itemData = item.m_itemData.m_shared;
                if (itemData == null)
                    continue;

                if (itemData.m_itemType == ItemDrop.ItemData.ItemType.Trophy) {
                    float prob = drop.m_chance * ThisPlugin.TrophyDropChanceMult.Value;
                    for (int i = 0; i < __instance.m_character.GetLevel(); i++)
                        prob *= ThisPlugin.TrophyLevelDropChanceMult.Value;

                    prob = Math.Min(prob, 1.0f);
                    drop.m_chance = prob;
                }
            }
        }
    }
}
