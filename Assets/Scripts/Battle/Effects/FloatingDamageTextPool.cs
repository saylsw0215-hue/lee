using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HeroDefense.Save;

namespace HeroDefense.Battle.Effects
{
    /// <summary>Pools short-lived damage labels; animation uses scaled time so pause freezes it.</summary>
    public sealed class FloatingDamageTextPool : MonoBehaviour
    {
        private readonly Queue<Text> available = new();
        private readonly List<Text> activeLabels = new();
        private int created,active;
        public int ActiveCount=>active;public int AvailableCount=>available.Count;
        public void Show(Transform parent, Vector3 localPosition, float amount)
            =>ShowAdvanced(parent,localPosition,amount,false,Combat.DamageType.Physical);
        public void ShowAdvanced(Transform parent,Vector3 localPosition,float amount,bool critical,Combat.DamageType type)
        {
            if(SaveGameManager.Instance!=null&&!SaveGameManager.Instance.Data.settings.damageNumbers)return;
            Text label = Acquire(parent);if(label==null)return;
            label.transform.SetParent(parent, false); label.transform.localPosition = localPosition + new Vector3(0f, 55f);
            string prefix=critical?"CRIT ":type==Combat.DamageType.Magical?"MAGIC ":type==Combat.DamageType.True?"TRUE ":"";label.text=$"{prefix}-{Mathf.RoundToInt(amount)}";label.color=critical?new Color(1f,.4f,.1f):type==Combat.DamageType.Magical?new Color(.65f,.45f,1f):new Color(1f,.92f,.42f);label.fontSize=critical?36:28;label.gameObject.SetActive(true);
            StartCoroutine(Animate(label));
        }
        public void ShowText(Transform parent,Vector3 localPosition,string value,Color color,int size=28){if(SaveGameManager.Instance!=null&&!SaveGameManager.Instance.Data.settings.damageNumbers)return;Text label=Acquire(parent);if(label==null)return;label.transform.SetParent(parent,false);label.transform.localPosition=localPosition+new Vector3(0,55);label.text=value;label.color=color;label.fontSize=size;label.gameObject.SetActive(true);StartCoroutine(Animate(label));}
        private Text Acquire(Transform parent){int maximum=SaveGameManager.Instance?.Data.settings.graphicsQuality==GraphicsQualityOption.Low?24:48;Text label;if(available.Count>0)label=available.Dequeue();else{if(created>=maximum)return null;created++;label=Create(parent);}active++;activeLabels.Add(label);return label;}
        private Text Create(Transform parent)
        {
            var go = new GameObject("DamageText", typeof(RectTransform), typeof(Text)); go.transform.SetParent(parent, false);
            var text = go.GetComponent<Text>(); text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = 28; text.alignment = TextAnchor.MiddleCenter;
            go.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 44); return text;
        }
        private IEnumerator Animate(Text label)
        {
            float elapsed = 0f;
            while (elapsed < .65f) { elapsed += Time.deltaTime; label.transform.localPosition += Vector3.up * (55f * Time.deltaTime); var c = label.color; c.a = 1f - elapsed / .65f; label.color = c; yield return null; }
            Return(label);
        }
        public void ReturnAll(){StopAllCoroutines();for(int i=activeLabels.Count-1;i>=0;i--)Return(activeLabels[i]);}
        private void Return(Text label){if(label==null||!activeLabels.Remove(label))return;label.gameObject.SetActive(false);active=Mathf.Max(0,active-1);available.Enqueue(label);}
    }
}
