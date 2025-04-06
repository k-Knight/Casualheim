
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace Casualheim.gui {
    [HarmonyPatch]
    public class LevelIndicatorPatch {
        public const float max_bar_length = 97f;
        public static TextMeshProUGUI level_indicator_text = null;
        public static RectTransform level_indicator_bar = null;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(InventoryGui), "Awake")]
        public static void InventoryGuiAwakePatch() {
            level_indicator_text = null;
            level_indicator_bar = null;

            var info = InventoryGui.m_instance.m_inventoryRoot.Find("Info") as RectTransform;
            var title = info.Find("TitlePanel") as RectTransform;
            var texts = info.Find("Texts") as RectTransform;
            var skills = info.Find("Skills") as RectTransform;
            var trophies = info.Find("Trophies") as RectTransform;
            var pvp = info.Find("PVP") as RectTransform;

            var char_name = title.Find("charactername") as RectTransform;
            var line_l = title.Find("BraidLineHorisontalMedium (1)") as RectTransform;
            var line_r = title.Find("BraidLineHorisontalMedium (2)") as RectTransform;

            line_l.anchoredPosition = new Vector2(line_l.anchoredPosition.x, -13);
            line_r.anchoredPosition = new Vector2(line_r.anchoredPosition.x, -13);

            texts.anchoredPosition = new Vector2(texts.anchoredPosition.x, -24);
            skills.anchoredPosition = new Vector2(skills.anchoredPosition.x, -24);
            trophies.anchoredPosition = new Vector2(trophies.anchoredPosition.x, -24);
            pvp.anchoredPosition = new Vector2(pvp.anchoredPosition.x, -24);

            GameObject level_bar_prot = null;
            GameObject level_bgr_prot = null;

            foreach (var obj in Resources.FindObjectsOfTypeAll(typeof(GuiBar)) as GuiBar[]) {
                if (obj.name.Equals("levelbar")) {
                    level_bar_prot = obj.gameObject;
                    break;
                }
            }

            foreach (var obj in Resources.FindObjectsOfTypeAll(typeof(ButtonSfx)) as ButtonSfx[]) {
                if (obj.name.Equals("Craft")) {
                    level_bgr_prot = obj.gameObject;
                    break;
                }
            }

            var level_bgr = title.Find("cslh_lvl_indicator") as RectTransform;
            if (level_bgr != null) {
                GameObject.Destroy(level_bgr.gameObject);
            }

            var level_bgr_go = Object.Instantiate<GameObject>(level_bgr_prot, title);
            level_bgr = level_bgr_go.GetComponent(typeof(RectTransform)) as RectTransform;

            GameObject.Destroy(level_bgr.Find("Text").gameObject);
            GameObject.Destroy(level_bgr.Find("Selected").gameObject);
            GameObject.Destroy(level_bgr.Find("gamepad_hint").gameObject);
            GameObject.Destroy(level_bgr_go.GetComponent(typeof(ButtonSfx)));
            GameObject.Destroy(level_bgr_go.GetComponent(typeof(UIGamePad)));
            GameObject.Destroy(level_bgr_go.GetComponent(typeof(ButtonTextColor)));

            var level_bar_go = Object.Instantiate<GameObject>(level_bar_prot, level_bgr.transform);
            var level_bar = level_bar_go.GetComponent(typeof(RectTransform)) as RectTransform;
            Debug.Log(level_bar);

            GameObject.Destroy(level_bar.Find("bkg").gameObject);
            GameObject.Destroy(level_bar.Find("bonustext").gameObject);
            GameObject.Destroy(level_bar_go.GetComponent(typeof(GuiBar)));

            level_bgr.localPosition = new Vector3(0f, 0f, 0f);
            level_bgr.anchoredPosition = new Vector2(level_bgr.anchoredPosition.x, level_bgr.anchoredPosition.y - 26f);
            level_bgr.sizeDelta = new Vector2(100f, 22f);
            level_bgr_go.name = "cslh_lvl_indicator";

            level_bar.anchoredPosition = new Vector2(0f, 0f);
            level_bar.sizeDelta = new Vector2(98.5f, 20f);
            level_bar_go.name = "cslh_lvl_bar_container";

            var level_bar_bar = level_bar.Find("bar") as RectTransform;
            level_bar.anchoredPosition = new Vector2(0f, 0f);
            level_bar_bar.sizeDelta = new Vector2(max_bar_length, 0f);
            level_bar_bar.gameObject.name = "cslh_lvl_bar";

            var level_bar_text_old = level_bar.Find("leveltext") as RectTransform;
            var level_bar_text_go = Object.Instantiate<GameObject>(char_name.gameObject, level_bar);
            var level_bar_text_tm = level_bar_text_go.GetComponent(typeof(TextMeshProUGUI)) as TextMeshProUGUI;
            var level_bar_text = level_bar_text_go.GetComponent(typeof(RectTransform)) as RectTransform;

            var anchor = level_bar_text_old.anchoredPosition;
            var offset_max = level_bar_text_old.offsetMax;
            var offset_min = level_bar_text_old.offsetMin;
            var size = level_bar_text_old.sizeDelta;

            level_bar_text.anchoredPosition = anchor;
            level_bar_text.offsetMax = offset_max;
            level_bar_text.offsetMin = offset_min;
            level_bar_text.sizeDelta = size;

            GameObject.Destroy(level_bar_text_old.gameObject);

            level_bar_text_go.name = "cslh_lvl_text";
            level_bar_text_tm.color = new Color(1f, 0.8667f, 0.6784f, 1f);
            level_bar_text_tm.fontSizeMax = 20f;

            level_indicator_text = level_bar_text_tm;
            level_indicator_bar = level_bar_bar;
        }

        public static void Update(int level, float progress) {
            if (level_indicator_text == null || level_indicator_bar == null)
                return;

            if (progress < 0f)
                progress = 0f;

            if (progress > 1f)
                progress = 1f;

            level_indicator_text.text = "Level  " + level.ToString();
            level_indicator_bar.sizeDelta = new Vector2(max_bar_length * progress, level_indicator_bar.sizeDelta.y);
        }
    }
}
