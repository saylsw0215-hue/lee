using System;
using System.Collections;
using HeroDefense.Battle.Buildings;
using HeroDefense.Battle.Combat;
using UnityEngine;
using UnityEngine.UI;
using HeroDefense.Meta;

namespace HeroDefense.Battle.Production
{
    /// <summary>Owns one building's construction, level, production timer, and shared-canvas visual.</summary>
    public sealed class ProductionBuilding:MonoBehaviour
    {
        public event Action<ProductionBuilding> Changed;
        public BuildingRuntimeState Runtime{get;private set;} public BuildSlotView Slot{get;private set;}
        public float ProductionProgress=>timer.Progress(Runtime.Data.GetProductionInterval(Runtime.CurrentLevel));
        private readonly ProductionTimer timer=new();
        private Func<UnitData,Vector2,bool> tryProduce;
        private Func<bool> blocked;
        private Image body,progress,levelBadge; private Text levelText; private bool selected;

        public void Initialize(BuildingData data,BuildSlotView slot,Func<UnitData,Vector2,bool> produce,Func<bool> isBlocked)
        {
            Runtime=new BuildingRuntimeState(data);Slot=slot;tryProduce=produce;blocked=isBlocked;BuildVisual();StartCoroutine(Construct());
        }
        private void BuildVisual()
        {
            var rect=GetComponent<RectTransform>();rect.sizeDelta=Runtime.Data.BuildingSize;rect.anchoredPosition=Vector2.zero;
            body=gameObject.AddComponent<Image>();body.sprite=UI.GameArtwork.GetBuildingSprite(Runtime.Data.BuildingId);body.preserveAspect=body.sprite!=null;body.color=body.sprite!=null?Color.white:Runtime.Data.BuildingColor;body.raycastTarget=false;
            if(body.sprite==null){var roof=new GameObject("DistinctiveTop",typeof(RectTransform),typeof(Image));roof.transform.SetParent(transform,false);var rr=roof.GetComponent<RectTransform>();rr.sizeDelta=Runtime.Data.VisualShape switch{BuildingVisualShape.Barracks=>new Vector2(100,30),BuildingVisualShape.ArcheryRange=>new Vector2(75,75),_=>new Vector2(44,92)};rr.anchoredPosition=new Vector2(0,42);roof.GetComponent<Image>().color=Runtime.Data.VisualShape switch{BuildingVisualShape.Barracks=>new Color(.12f,.35f,.82f),BuildingVisualShape.ArcheryRange=>new Color(.48f,.27f,.1f),_=>new Color(.63f,.25f,.92f)};}
            var name=UI.UiFactory.Label(transform,"Name",Runtime.Data.DisplayName,20,TextAnchor.MiddleCenter,Color.white);name.raycastTarget=false;name.rectTransform.anchorMin=new Vector2(0,0);name.rectTransform.anchorMax=new Vector2(1,.3f);
            var bar=UI.UiFactory.Panel(transform,"ProductionBar",new Color(.06f,.06f,.06f,.9f),new Vector2(.08f,.82f),new Vector2(.92f,.93f));
            var fill=UI.UiFactory.Panel(bar,"Fill",new Color(.2f,.88f,.38f),Vector2.zero,Vector2.one);progress=fill.GetComponent<Image>();progress.type=Image.Type.Filled;progress.fillMethod=Image.FillMethod.Horizontal;
            levelBadge=UI.UiFactory.Panel(transform,"LevelBadge",new Color(.08f,.1f,.18f,.95f),new Vector2(.7f,.02f),new Vector2(.98f,.28f)).GetComponent<Image>();
            levelText=UI.UiFactory.Label(levelBadge.transform,"Level","1",18,TextAnchor.MiddleCenter,Color.white);ResetVisual();
        }
        private IEnumerator Construct()
        {
            float elapsed=0f;transform.localScale=Vector3.zero;body.color=new Color(body.color.r,body.color.g,body.color.b,.45f);
            while(elapsed<.7f){elapsed+=Time.deltaTime;transform.localScale=Vector3.one*Mathf.Clamp01(elapsed/.7f);progress.fillAmount=Mathf.Clamp01(elapsed/.7f);yield return null;}
            transform.localScale=Vector3.one;var c=body.color;c.a=1f;body.color=c;Runtime.IsConstructing=false;timer.Reset();Changed?.Invoke(this);
        }
        private void Update()
        {
            if(Runtime==null||Runtime.IsSold||Runtime.IsConstructing||blocked())return;
            float interval=Runtime.Data.GetProductionInterval(Runtime.CurrentLevel)*(HeroDefense.Progression.BattleModifierRepository.Current?.ProductionIntervalMultiplier??1f)*MetaRuntimeModifierProvider.ProductionIntervalMultiplier;
            if(timer.Tick(Time.deltaTime,interval)&&tryProduce(Runtime.Data.ProducedUnit,Slot.RectTransform.anchoredPosition+new Vector2(90,-25)))timer.Consume();
            progress.fillAmount=timer.Progress(interval);
        }
        public bool Upgrade()
        {
            float oldInterval=Runtime.Data.GetProductionInterval(Runtime.CurrentLevel);if(!Runtime.TryUpgrade())return false;
            timer.PreserveProgress(oldInterval,Runtime.Data.GetProductionInterval(Runtime.CurrentLevel));RefreshLevel();Changed?.Invoke(this);return true;
        }
        public void RefreshAfterExternalUpgrade(int previousLevel)
        {
            timer.PreserveProgress(Runtime.Data.GetProductionInterval(previousLevel),Runtime.Data.GetProductionInterval(Runtime.CurrentLevel));
            RefreshLevel();Changed?.Invoke(this);
        }
        public void SetSelected(bool value){selected=value;ResetVisual();}
        private void ResetVisual(){if(body==null)return;transform.localScale=selected?Vector3.one*1.08f:Vector3.one;Color normal=body.sprite!=null?Color.white:Runtime.Data.BuildingColor;body.color=selected?Color.Lerp(normal,new Color(1f,.78f,.25f),.28f):normal;}
        private void RefreshLevel(){levelText.text=Runtime.CurrentLevel.ToString();transform.localScale=Vector3.one*(1f+(Runtime.CurrentLevel-1)*.05f);}
        public void StopImmediately(){StopAllCoroutines();enabled=false;}
    }
}
