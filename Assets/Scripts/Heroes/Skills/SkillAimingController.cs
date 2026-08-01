using System;
using HeroDefense.Battle.Combat;
using HeroDefense.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HeroDefense.Heroes.Skills
{
    public static class SkillAimMath{public static Vector3 Clamp(Vector3 origin,Vector3 point,float rangePixels){Vector3 delta=point-origin;return rangePixels>0&&delta.sqrMagnitude>rangePixels*rangePixels?origin+delta.normalized*rangePixels:point;}}
    /// <summary>One pointer-driven aiming overlay shared by active and ultimate skills.</summary>
    public sealed class SkillAimingController:MonoBehaviour,IPointerClickHandler
    {
        public bool IsAiming=>overlay!=null&&overlay.activeSelf;public Vector3 CurrentPoint{get;private set;}public event Action<bool> AimingChanged;
        private HeroController hero;private RectTransform world,overlayRect,preview;private GameObject overlay;private Text hint;private HeroSkillData skill;private bool ultimate;
        public void Initialize(RectTransform safe,RectTransform battleWorld,HeroController controller)
        {
            hero=controller;world=battleWorld;overlay=UiFactory.Panel(safe,"SkillAimOverlay",new Color(0,0,0,.12f),Vector2.zero,Vector2.one).gameObject;overlayRect=overlay.GetComponent<RectTransform>();overlay.AddComponent<SkillAimRaycaster>().Owner=this;
            preview=UiFactory.Panel(overlay.transform,"SkillAimPreview",new Color(.2f,.8f,1f,.28f),new Vector2(.5f,.5f),new Vector2(.5f,.5f));preview.pivot=new Vector2(.5f,.5f);var outline=preview.gameObject.AddComponent<Outline>();outline.effectColor=new Color(.65f,1f,1f,.95f);outline.effectDistance=new Vector2(3,-3);outline.useGraphicAlpha=false;
            hint=UiFactory.Label(overlay.transform,"AimHint","전장을 터치해 발동",28,TextAnchor.MiddleCenter,Color.white);hint.rectTransform.anchorMin=new Vector2(.3f,.88f);hint.rectTransform.anchorMax=new Vector2(.7f,.97f);hint.rectTransform.offsetMin=hint.rectTransform.offsetMax=Vector2.zero;
            Button cancel=UiFactory.Button(overlay.transform,"CancelAim","조준 취소",new Color(.55f,.16f,.16f));var cr=cancel.GetComponent<RectTransform>();cr.anchorMin=new Vector2(.8f,.05f);cr.anchorMax=new Vector2(.97f,.16f);cr.offsetMin=cr.offsetMax=Vector2.zero;cancel.onClick.AddListener(Cancel);overlay.SetActive(false);
        }
        public bool Begin(HeroSkillData data,bool isUltimate)
        {if(data==null||hero==null||!hero.IsAlive||hero.Statuses.IsSilenced)return false;skill=data;ultimate=isUltimate;overlay.SetActive(true);CurrentPoint=Clamp(hero.transform.localPosition+Vector3.right*data.Range*CombatUnit.PixelsPerUnit*.65f);UpdatePreview(CurrentPoint);AimingChanged?.Invoke(true);return true;}
        public void OnPointerClick(PointerEventData eventData){if(!IsAiming)return;Vector2 point;if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(world,eventData.position,eventData.pressEventCamera,out point))return;Confirm(Clamp(point));}
        public bool Confirm(Vector3 point){if(!IsAiming)return false;CurrentPoint=Clamp(point);bool used=ultimate?hero.UseUltimateAt(CurrentPoint):hero.UseActiveSkillAt(CurrentPoint);if(used)Close();return used;}
        public void Cancel(){if(!IsAiming)return;Close();}
        public Vector3 Clamp(Vector3 point){Vector3 origin=hero.transform.localPosition;float max=skill!=null?skill.Range*CombatUnit.PixelsPerUnit:0;return SkillAimMath.Clamp(origin,point,max);}
        private void UpdatePreview(Vector3 point){preview.localPosition=point;float radius=skill.Radius*CombatUnit.PixelsPerUnit;preview.sizeDelta=skill.TargetingMode==SkillTargetingMode.Cone?new Vector2(radius*2.2f,radius):new Vector2(radius*2,radius*2);hint.text=skill.TargetingMode==SkillTargetingMode.Cone?"방향을 터치해 부채꼴 발동":"위치를 터치해 원형 발동";}
        private void Close(){overlay.SetActive(false);skill=null;AimingChanged?.Invoke(false);}
        private void Update(){if(!IsAiming)return;if(hero==null||!hero.IsAlive||Time.timeScale<=0){Cancel();return;}UpdatePreview(CurrentPoint);}
        private sealed class SkillAimRaycaster:MonoBehaviour,IPointerClickHandler{public SkillAimingController Owner;public void OnPointerClick(PointerEventData eventData)=>Owner.OnPointerClick(eventData);}
    }
}
