using System.Collections;
using System.Collections.Generic;
using HeroDefense.Battle.Combat;
using UnityEngine;
using UnityEngine.UI;

namespace HeroDefense.Battle.Projectiles
{
    /// <summary>Pools cosmetic ranged projectiles; combat damage remains deterministic at attack time.</summary>
    public sealed class ProjectilePool:MonoBehaviour
    {
        private readonly Queue<Image> available=new();
        public int ActiveCount{get;private set;}public int AvailableCount=>available.Count;
        public void Show(UnitVisualShape shape,Transform parent,Vector3 start,Vector3 end)
        {
            if(shape!=UnitVisualShape.Archer&&shape!=UnitVisualShape.Mage)return;
            Image image=available.Count>0?available.Dequeue():Create();ActiveCount++;image.transform.SetParent(parent,false);image.transform.localPosition=start;image.gameObject.SetActive(true);
            image.color=shape==UnitVisualShape.Archer?new Color(.95f,.78f,.3f):new Color(.72f,.35f,1f);image.rectTransform.sizeDelta=shape==UnitVisualShape.Archer?new Vector2(38,8):new Vector2(22,22);
            StartCoroutine(Fly(image,start,end));
        }
        private Image Create(){var go=new GameObject("PooledProjectile",typeof(RectTransform),typeof(Image));return go.GetComponent<Image>();}
        private IEnumerator Fly(Image image,Vector3 start,Vector3 end)
        {float elapsed=0f;while(elapsed<.18f){elapsed+=Time.deltaTime;image.transform.localPosition=Vector3.Lerp(start,end,elapsed/.18f);yield return null;}image.gameObject.SetActive(false);ActiveCount=Mathf.Max(0,ActiveCount-1);available.Enqueue(image);}
    }
}
