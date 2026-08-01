using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HeroDefense.Core;

namespace HeroDefense.Heroes.Skills
{
    /// <summary>Pools shared-canvas placeholder circles for skill, ultimate, burn, and respawn feedback.</summary>
    public sealed class HeroEffectPool:MonoBehaviour
    {
        private readonly Queue<Image> available=new();
        public void Show(Vector3 position,float radius,Color color,float duration=.45f){Image image=available.Count>0?available.Dequeue():Create();image.transform.SetParent(transform.parent,false);image.transform.localPosition=position;image.rectTransform.sizeDelta=Vector2.one*radius*2;image.color=color;image.gameObject.SetActive(true);StartCoroutine(Animate(image,duration));}
        public void ShowHero(string heroId,Vector3 position,float radius,Color color,float duration=.45f){Image image=available.Count>0?available.Dequeue():Create();image.transform.SetParent(transform.parent,false);image.transform.localPosition=position;image.rectTransform.sizeDelta=Vector2.one*radius*2;image.sprite=RuntimeArtworkCatalog.HeroEffect(heroId);image.preserveAspect=image.sprite!=null;image.color=image.sprite!=null?new Color(1,1,1,.9f):color;image.gameObject.SetActive(true);StartCoroutine(Animate(image,duration));}
        private Image Create(){var go=new GameObject("PooledHeroEffect",typeof(RectTransform),typeof(Image));return go.GetComponent<Image>();}
        private IEnumerator Animate(Image image,float duration){float elapsed=0;while(elapsed<duration){elapsed+=Time.deltaTime;float t=Mathf.Clamp01(elapsed/duration);image.transform.localScale=Vector3.one*(.5f+t*.7f);var c=image.color;c.a=1-t;image.color=c;yield return null;}image.gameObject.SetActive(false);image.sprite=null;image.transform.localScale=Vector3.one;available.Enqueue(image);}
    }
}
