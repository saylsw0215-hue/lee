using System.Collections;
using HeroDefense.Audio;
using HeroDefense.Save;
using UnityEngine;

namespace HeroDefense.Battle.Effects
{
    public sealed class ScreenShakeController:MonoBehaviour
    {
        private RectTransform target;private Vector2 origin;public void Initialize(RectTransform value){target=value;origin=value.anchoredPosition;}public void Play(float duration=.35f,float strength=10){if(target==null||SaveGameManager.Instance?.Data.settings.screenShake==false)return;StopAllCoroutines();StartCoroutine(Shake(duration,strength));}
        private IEnumerator Shake(float duration,float strength){float elapsed=0;while(elapsed<duration){elapsed+=Time.unscaledDeltaTime;target.anchoredPosition=origin+Random.insideUnitCircle*strength*(1-elapsed/duration);yield return null;}target.anchoredPosition=origin;}
    }
}
