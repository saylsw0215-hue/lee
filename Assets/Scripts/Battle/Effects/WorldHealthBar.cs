using UnityEngine;
using UnityEngine.UI;
using HeroDefense.Save;

namespace HeroDefense.Battle.Effects
{
    /// <summary>Lightweight health bar rendered inside the shared battle canvas.</summary>
    public sealed class WorldHealthBar
    {
        private readonly Image fill;private readonly GameObject root;
        public WorldHealthBar(Transform parent, Vector2 offset, float width = 76f)
        {
            var background = new GameObject("HealthBar", typeof(RectTransform), typeof(Image));
            root=background;
            background.transform.SetParent(parent, false);
            var rect = background.GetComponent<RectTransform>(); rect.sizeDelta = new Vector2(width, 10f); rect.anchoredPosition = offset;
            background.GetComponent<Image>().color = new Color(.08f, .08f, .08f, .9f);
            var fillObject = new GameObject("Fill", typeof(RectTransform), typeof(Image)); fillObject.transform.SetParent(background.transform, false);
            fill = fillObject.GetComponent<Image>(); fill.color = new Color(.18f, .84f, .28f); fill.type = Image.Type.Filled; fill.fillMethod = Image.FillMethod.Horizontal;
            var fillRect = fillObject.GetComponent<RectTransform>(); fillRect.anchorMin = Vector2.zero; fillRect.anchorMax = Vector2.one; fillRect.offsetMin = fillRect.offsetMax = Vector2.zero;
        }
        public void Set(float current, float maximum){root.SetActive(SaveGameManager.Instance==null||SaveGameManager.Instance.Data.settings.healthBars);fill.fillAmount=maximum<=0f?0f:Mathf.Clamp01(current/maximum);}
    }
}
